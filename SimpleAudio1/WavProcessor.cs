using System;
using NAudio.Wave;

namespace SimpleAudio1
{
    class WindowFunction
    {
        float[] window;
        uint buffersize;
        public uint windowsize { get; private set; }
        public const int Hann = 0;
        public const int Sine = 1;
        public int type;
        public WindowFunction(uint size, int type = Hann)
        {
            buffersize = windowsize = size;
            window = new float[size];
            this.type = type;
        }
        public float this[int n]
        {
            get => window[n];
        }
        private void setWeight(uint n)
        {
            for (int i = 0; i < n; i++)
            {
                switch (type)
                {
                    case Hann:
                        window[i] = (float)(0.5 * (1.0 - Math.Cos(2 * Math.PI * (double)i / (double)n)));
                        break;
                    case Sine:
                        window[i] = (float)Math.Sin(Math.PI * (double)i / (double)n);
                        break;
                }
            }
        }
        public void reset(uint n)
        {
            if (n > buffersize)
            {
                window = new float[n];
                buffersize = windowsize = n;
                setWeight(n);
                return;
            }
            if (n != windowsize)
            {
                setWeight(windowsize);
                windowsize = n;
            } 
        }
        public void apply(float[] x)
        {
            for (int i = 0; i < windowsize; i++)
                x[i] *= window[i];
        }
    }
    class WavProcessor : ISampleProvider, IDisposable
    {
        private IWaveProvider reader;
        private byte[] bbuffer;
        private byte[] bbuffer0;
        private float[] fbuffer;
        private float[] fbuffer0;
        private WindowFunction window;
        private int buffersize = 20000;
        private int frameSize = 0;
        private int samplerate = 44100;
        //private DiscreteSignal signal;
        //private FirFilter filt;

        private SignalProc proc;
        public WavProcessor(IWaveProvider provider)
        {
            reader = provider;
            prepareBuffers(buffersize);
            window = new WindowFunction(0,WindowFunction.Sine);
            proc = new SignalProc();
            //signal = new DiscreteSignal(samplerate,fbuffer);
            //var kernel = DesignFilter.FirWinHp(111, 0.05);
            //filt = new FirFilter(kernel);
        }

        private void prepareBuffers(int size)
        {
            buffersize = size;
            bbuffer = new byte[size * 2];  // (size) samples
            bbuffer0 = new byte[size * 2];  // (size) samples
            fbuffer = new float[size * 2]; // (size*2) samples
            fbuffer0 = new float[size];     // (size) samples
        }

        WaveFormat ISampleProvider.WaveFormat
        {
            get
            {
                if (reader != null)
                {
                    samplerate = reader.WaveFormat.SampleRate;
                }
                return WaveFormat.CreateIeeeFloatWaveFormat(samplerate, 1);
            }
        }
        private static float byte2float(byte b1, byte b2)
        {
            float x = b1 * 256 + b2;
            if (x > 32767) x -= 65535;
            return (float)(x / 32768.0);
        }


        public int Read(float[] buffer, int offset, int count)
        {
            if (count > buffersize)
            {
                prepareBuffers(count);
                //throw new Exception("Internal buffer too small: size=" + bbuffersize.ToString() + "  required=" + c.ToString());
            }
            int r = reader.Read(bbuffer, 0, count * 2);
            if (r == 0)
                return 0;
            frameSize = r / 2;
            window.reset((uint)(frameSize * 2));
            proc.setLength(frameSize * 2);
            //Console.WriteLine(r.ToString() + " bytes read out of "+(count*2).ToString());
            for (int i = 0; i < r; i += 2)
            {
                fbuffer[i / 2] = byte2float(bbuffer0[i + 1], bbuffer0[i]);
                fbuffer[frameSize + i / 2] = byte2float(bbuffer[i + 1], bbuffer[i]);
                bbuffer0[i] = bbuffer[i];
                bbuffer0[i + 1] = bbuffer[i + 1];
            }
            window.apply(fbuffer);
            proc.process(fbuffer,frameSize*2);
            window.apply(fbuffer);
            for (int i = 0; i < frameSize; i++)
            {
                buffer[i + offset] = fbuffer[i] + fbuffer0[i];
                fbuffer0[i] = fbuffer[frameSize + i];
            }

            return frameSize;
        }


        public void Dispose()
        {

        }

    }
}
