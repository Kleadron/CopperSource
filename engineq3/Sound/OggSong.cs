// This file was written by Nick Gravelyn! =]

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NVorbis;
using Microsoft.Xna.Framework.Audio;
using System.Threading;
using Microsoft.Xna.Framework;

namespace XboxTest.Sound
{
    public class OggSong : IDisposable
    {
        private VorbisReader reader;
        private DynamicSoundEffectInstance effect;

        private Thread thread;
        //private EventWaitHandle threadRunHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        //private EventWaitHandle needBufferHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private ManualResetEvent threadRunEvent = new ManualResetEvent(false);
        private ManualResetEvent needBufferEvent = new ManualResetEvent(false);
        private byte[] buffer;
        private float[] nvBuffer;

        public SoundState State
        {
            get { return effect.State; }
        }

        public float Volume
        {
            get { return effect.Volume; }
            set { effect.Volume = MathHelper.Clamp(value, 0, 1); }
        }

        public float Pitch
        {
            get { return effect.Pitch; }
            set { effect.Pitch = MathHelper.Clamp(value, -1, 1); }
        }

        public bool IsLooped { get; set; }

        public OggSong(string oggFile)
        {
            reader = new VorbisReader(TitleContainer.OpenStream(oggFile), true);
            effect = new DynamicSoundEffectInstance(reader.SampleRate, (AudioChannels)reader.Channels);
            buffer = new byte[effect.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(500))];
            nvBuffer = new float[buffer.Length / 2];

            // when a buffer is needed, set our handle so the helper thread will read in more data
            effect.BufferNeeded += (s, e) => needBufferEvent.Set();
        }

        ~OggSong()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool isDisposing)
        {
            threadRunEvent.Set();
            lock (effect)
            {
                effect.Dispose();
            }
        }

        public void Play()
        {
            Stop();

            lock (effect)
            {
                effect.Play();
            }

            StartThread();
        }

        public void Pause()
        {
            lock (effect)
            {
                effect.Pause();
            }
        }

        public void Resume()
        {
            lock (effect)
            {
                effect.Resume();
            }
        }

        public void Stop()
        {
            lock (effect)
            {
                if (!effect.IsDisposed)
                {
                    effect.Stop();
                }
            }

            reader.DecodedTime = TimeSpan.Zero;

            if (thread != null)
            {
                // set the handle to stop our thread
                threadRunEvent.Set();
                thread = null;
            }
        }

        private void StartThread()
        {
            if (thread == null)
            {
                thread = new Thread(StreamThread);
                thread.Start();
            }
        }

        private void StreamThread()
        {
            while (!effect.IsDisposed)
            {
                // sleep until we need a buffer
                while (!effect.IsDisposed && !threadRunEvent.WaitOne(0) && !needBufferEvent.WaitOne(0))
                {
                    Thread.Sleep(50);
                    //FrameworkDispatcher.Update();
                }

                // if the thread is waiting to exit, leave
                if (threadRunEvent.WaitOne(0))
                {
                    break;
                }

                lock (effect)
                {
                    // ensure the effect isn't disposed
                    if (effect.IsDisposed) { break; }
                }

                // read the next chunk of data
                int samplesRead = reader.ReadSamples(nvBuffer, 0, nvBuffer.Length);

                // out of data and looping? reset the reader and read again
                if (samplesRead == 0 && IsLooped)
                {
                    reader.DecodedTime = TimeSpan.Zero;
                    samplesRead = reader.ReadSamples(nvBuffer, 0, nvBuffer.Length);
                }

                if (samplesRead > 0)
                {
                    for (int i = 0; i < samplesRead; i++)
                    {
                        short sValue = (short)Math.Max(Math.Min(short.MaxValue * nvBuffer[i], short.MaxValue), short.MinValue);
                        buffer[i * 2] = (byte)(sValue & 0xff);
                        buffer[i * 2 + 1] = (byte)((sValue >> 8) & 0xff);
                    }

                    // submit our buffers
                    lock (effect)
                    {
                        // ensure the effect isn't disposed
                        if (effect.IsDisposed) { break; }

                        effect.SubmitBuffer(buffer, 0, samplesRead);
                        effect.SubmitBuffer(buffer, samplesRead, samplesRead);
                    }
                }

                // reset our handle
                needBufferEvent.Reset();
            }
        }
    }
}
