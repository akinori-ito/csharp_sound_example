using System;
using NAudio.Wave;
using System.Threading;

namespace SimpleAudio1
{

    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine(WaveIn.DeviceCount.ToString() + " input devices found");
            for (int i = 0; i < WaveIn.DeviceCount; i++)
                Console.WriteLine("Input device = " + WaveIn.GetCapabilities(i).ProductName + " (" + i.ToString() + ")");
            Console.WriteLine(WaveOut.DeviceCount.ToString() + " output devices found");
            for (int i = 0; i < WaveOut.DeviceCount; i++)
                Console.WriteLine("Output device = " + WaveOut.GetCapabilities(i).ProductName + " (" + i.ToString() + ")");

            //using (var inputDevice = new WaveFileInputAttachment("test.wav"))
            //using (var outputDevice = new WaveFileOutputAttachment("out.wav"))
            using (var outputDevice = new WaveOutputDeviceAttachment(0, 50))
            using (var inputDevice = new WaveInputDeviceAttachment())
            {

                inputDevice.WaveFormat = new WaveFormat(44100,1);

                outputDevice.Init(new WavProcessor(inputDevice));
                inputDevice.StartRecording();
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
