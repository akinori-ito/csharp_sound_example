using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SimpleAudio1
{
    // IWaveProvider Attachment for devices and files
    abstract class WaveInputAttachment : IWaveProvider, IDisposable
    {
        public const int RecordingDevice = 0;
        public const int WavFile = 1;
        public abstract int type();
        public abstract int Read(byte[] b, int o, int c);
        public abstract WaveFormat WaveFormat { get; set; }
        public virtual void StartRecording() { }
        public virtual void Dispose() { }

    }
    class WaveFileInputAttachment : WaveInputAttachment
    {
        WaveFileReader reader;
        public WaveFileInputAttachment(string filename)
        {
            reader = new WaveFileReader(filename);
        }
        override public void Dispose()
        {
            reader.Close();
        }
        override public int type()
        {
            return WavFile;
        }
        override public int Read(byte[] buffer, int offset, int count)
        {
            int n = reader.Read(buffer, offset, count);
            return n;
        }
        override public WaveFormat WaveFormat
        {
            get { return reader.WaveFormat; }
            set { /* do nothing*/ }
        }

    }

    class WaveInputDeviceAttachment : WaveInputAttachment
    {
        WaveInEvent device;
        WaveInProvider provider;
        public WaveInputDeviceAttachment()
        {
            device = new WaveInEvent();
            provider = new WaveInProvider(device);
        }
        override public int type()
        {
            return RecordingDevice;
        }
        override public int Read(byte[] buffer, int offset, int count)
        {
            int n = provider.Read(buffer, offset, count);
            return n;
        }
        override public WaveFormat WaveFormat
        {
            get => device.WaveFormat;
            set { device.WaveFormat = value; }
        }
        override public void StartRecording()
        {
            device.StartRecording();
        }
    }

    abstract class WaveOutputAttachment : IDisposable
    {
        public const int PlayDevice = 0;
        public const int WavFile = 1;
        public virtual void Play() { }
        public abstract int type();
        public abstract void Init(IWaveProvider provider);
        public abstract void Init(ISampleProvider provider);
        public PlaybackState PlayBackState { get; set; }
        public virtual void Dispose() { }
    }
    class WaveFileOutputAttachment : WaveOutputAttachment
    {
        private string filename;
        WaveFileWriter writer;
        IWaveProvider wavein;
        public PlaybackState PlaybackState { get; set; }
        public WaveFileOutputAttachment(string filename)
        {
            this.filename = filename;
            PlayBackState = PlaybackState.Stopped;
        }
        override public void Init(IWaveProvider w_in)
        {
            writer = new WaveFileWriter(filename, w_in.WaveFormat);
            wavein = w_in;
        }
        override public void Init(ISampleProvider w_in)
        {
            Init(new SampleToWaveProvider16(w_in));
        }
        override public void Dispose()
        {
            writer.Close();
        }
        override public int type()
        {
            return WavFile;
        }
        public override void Play()
        {
            var buffer = new byte[2048];
            PlayBackState = PlaybackState.Playing;
            while (true)
            {
                int bytesRead = wavein.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    // end of source provider
                    break;
                }
                // Write will throw exception if WAV file becomes too large
                writer.Write(buffer, 0, bytesRead);
            }
            PlayBackState = PlaybackState.Stopped;

        }
    }

    class WaveOutputDeviceAttachment : WaveOutputAttachment {
        WaveOutEvent device;
        public WaveOutputDeviceAttachment(int devNo, int latency)
        {
            device = new WaveOutEvent();
            device.DeviceNumber = devNo;
            device.DesiredLatency = latency;
        }
        public WaveOutputDeviceAttachment(int devNo, int latency, IWaveProvider w_in)
        {
            device = new WaveOutEvent();
            device.DeviceNumber = devNo;
            device.DesiredLatency = latency;
            device.Init(w_in);
        }
        override public void Init(IWaveProvider w_in)
        {
            device.Init(w_in);
        }
        override public void Init(ISampleProvider w_in)
        {
            device.Init(new SampleToWaveProvider16(w_in));
        }
        public PlaybackState PlaybackState => device.PlaybackState;
        override public int type()
        {
            return PlayDevice;
        }
        public override void Play()
        {
            device.Play();
        }
    }
}
