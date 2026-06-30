using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using Android.Media;
//using Android.Nfc;
using Android.Util;

namespace ArduinoTone
{
    public class ArduinoToneGenerator
    {
        private AudioTrack audioTrack;
        private const string TAG = "[ToneGenerator]";

        public void tone(double freqHz)
        {
            noTone();

            int sampleRate = 44100;
            int cycleSamples = (int)(sampleRate / freqHz);
            short[] sample = new short[cycleSamples];
            for (int i = 0; i < cycleSamples; i++)
            {
                sample[i] = (short)(Math.Sin(2 * Math.PI * i / cycleSamples) * short.MaxValue);
            }
            var audioAttributes = new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Media)
                .SetContentType(AudioContentType.Music)
                .Build();
            var audioFormat = new AudioFormat.Builder()
                .SetEncoding(Encoding.Pcm16bit)
                .SetSampleRate(sampleRate)
                .SetChannelMask(ChannelOut.Mono)
                .Build();
            int bufferSize = sample.Length * sizeof(short);
            audioTrack = new AudioTrack(
                audioAttributes, audioFormat, bufferSize,
                AudioTrackMode.Static, AudioManager.AudioSessionIdGenerate);
            audioTrack.Write(sample, 0, sample.Length);
            audioTrack.SetLoopPoints(0, sample.Length, -1);
            audioTrack.Play();
            //System.Threading.Tasks.Task.Delay(durationMs).ContinueWith( t =>
            //{
            //    audioTrack.Stop();
            //    audioTrack.Release();
            //});
        }
        public void noTone()
        {
            if (audioTrack is not null)
            {
                try
                {
                    if (audioTrack.PlayState == PlayState.Playing)
                    {
                        audioTrack.Stop();
                    }
                }
                catch { Log.Info(TAG, "Error in noTone()"); }
                finally
                {
                    audioTrack.Release();
                    audioTrack = null;
                }
            }
        }

    }
}
