using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWaves.Transforms;

namespace SimpleAudio1
{
    class SpectrumStretcher
    {
        private int size;
        private int[] index;
        private float[] buffer;
        public SpectrumStretcher(int size, float rate)
        {
            this.size = size;
            buffer = new float[size];
            index = new int[size];
            for (int i = 0; i < size / 2; i++)
            {
                var k = (int)Math.Floor((double)i / rate);
                index[i] = k;
                index[size - 1 - i] = size - 1 - k;
            }
        }
        public void stretch(float[] x_in, float[] x_out)
        {
            for (int i = 0; i < size; i++)
                buffer[i] = x_in[index[i]];
            for (int i = 0; i < size; i++)
                x_out[i] = buffer[i];
        }
    }
    class SignalProc
    {
        private RealFft rfft;
        private float[] real;
        private float[] imag;
        private SpectrumStretcher stretcher;
        public SignalProc()
        {
            
        }

        private int twoPower(int orglen)
        {
            int p = 1;
            while (p < orglen)
                p <<= 1;
            //Console.WriteLine("twoPower(" + orglen.ToString() + ")=" + p.ToString());
            return p;
        }

        public void setLength(int length)
        {
            int size = twoPower(length);
            if (rfft == null || rfft.Size != size)
            {
                rfft = new RealFft(size);
                real = new float[size];
                imag = new float[size];
                stretcher = new SpectrumStretcher(size, 1.5f);
            }
        }
        public void process(float[] x, int length)
        {
            int i;
            for (i = 0; i < length; i++)
            {
                real[i] = x[i];
                imag[i] = 0;
            }
            for (; i < rfft.Size; i++)
            {
                real[i] = imag[i] = 0;
            }
            rfft.Direct(real, real, imag);
            stretcher.stretch(real, real);
            stretcher.stretch(imag, imag);
            rfft.Inverse(real, imag, real);
            for (i = 0; i < length; i++)
                x[i] = real[i]/rfft.Size;
        }
        
    }
}
