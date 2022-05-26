//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework.Audio;
//using System.Threading;
//using Microsoft.Xna.Framework;
//using OpenMpt;
//using System.IO;
//using OpenMpt.Ext;

//namespace TanksEXP.Sound
//{
//    public class ModuleSong : IDisposable
//    {
//        Module module;
//        ModuleExt ext;
//        Interactive interactive;
//        //private VorbisReader reader;
//        private DynamicSoundEffectInstance effect;

//        private Thread thread;
//        //private EventWaitHandle threadRunHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
//        //private EventWaitHandle needBufferHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
//        private ManualResetEvent threadRunEvent = new ManualResetEvent(false);
//        private ManualResetEvent needBufferEvent = new ManualResetEvent(false);
//        private byte[] buffer;
//        private short[] modBuffer;

//        int sampleRate;
//        int buffers = 2;

//        public SoundState State
//        {
//            get { return effect.State; }
//        }

//        public float Volume
//        {
//            get { return effect.Volume; }
//            set { effect.Volume = MathHelper.Clamp(value, 0, 1); }
//        }

//        public float Pitch
//        {
//            get { return effect.Pitch; }
//            set { effect.Pitch = MathHelper.Clamp(value, -1, 1); }
//        }

//        bool looped = true;
//        public bool IsLooped
//        {
//            get
//            {
//                return looped;
//            }
//            set
//            {
//                looped = value;
//                if (value)
//                {
//                    module.SetRepeatCount(-1);
//                }
//                else
//                {
//                    module.SetRepeatCount(0);
//                }
//            }
//        }

//        /// <summary>
//        /// Opens a tracker module using libopenmpt as a DynamicSoundEffect stream.
//        /// </summary>
//        /// <param name="moduleFile">The module file.</param>
//        /// <param name="sampleRate">Sample rate to play the file back with.</param>
//        /// <param name="bufferTimeMillis">The length of time the buffer should be made for the sample rate. Lower times will make changes more responsive.</param>
//        public ModuleSong(string moduleFile, int sampleRate = 48000, int bufferTimeMillis = 500)
//        {
//            ext = new ModuleExt(moduleFile);
//            module = ext.GetModule();
//            interactive = ext.GetInteractive();
//            module.SetRepeatCount(-1);
//            //reader = new VorbisReader(TitleContainer.OpenStream(oggFile), true);
//            effect = new DynamicSoundEffectInstance(sampleRate, AudioChannels.Stereo);
//            this.sampleRate = sampleRate;
//            buffer = new byte[effect.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(bufferTimeMillis))];
//            modBuffer = new short[buffer.Length / 2];

//            // when a buffer is needed, set our handle so the helper thread will read in more data
//            effect.BufferNeeded += (s, e) => needBufferEvent.Set();
//        }

//        ~ModuleSong()
//        {
//            Dispose(false);
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        protected void Dispose(bool isDisposing)
//        {
//            threadRunEvent.Set();
//            effect.Dispose();
            
//        }

//        public void Play()
//        {
//            Stop();

//            lock (effect)
//            {
//                effect.Play();
//            }

//            StartThread();
//        }

//        public void Pause()
//        {
//            lock (effect)
//            {
//                effect.Pause();
//            }
//        }

//        public void Resume()
//        {
//            lock (effect)
//            {
//                effect.Resume();
//            }
//        }

//        public void Stop()
//        {
//            lock (effect)
//            {
//                if (!effect.IsDisposed)
//                {
//                    effect.Stop();
//                }
//            }

//            //reader.DecodedTime = TimeSpan.Zero;
//            module.SetPositionOrderRow(0, 0);

//            if (thread != null)
//            {
//                // set the handle to stop our thread
//                threadRunEvent.Set();
//                thread = null;
//            }
//        }

//        private void StartThread()
//        {
//            if (thread == null)
//            {
//                thread = new Thread(StreamThread);
//                thread.Start();
//            }
//        }

//        private void StreamThread()
//        {
//            while (!effect.IsDisposed)
//            {
//                // sleep until we need a buffer
//                while (!effect.IsDisposed && !threadRunEvent.WaitOne(0) && !needBufferEvent.WaitOne(0))
//                {
//                    Thread.Sleep(50);
//                }

//                // if the thread is waiting to exit, leave
//                if (threadRunEvent.WaitOne(0))
//                {
//                    break;
//                }

//                lock (effect)
//                {
//                    // ensure the effect isn't disposed
//                    if (effect.IsDisposed) { break; }
//                }

//                for (int i = 0; i < buffers; i++)
//                {
//                    // read the next chunk of data
//                    int samplesRead = (int)module.ReadInterleavedStereo(sampleRate, modBuffer.Length / 2, modBuffer);//reader.ReadSamples(nvBuffer, 0, nvBuffer.Length);

//                    // out of data and looping? reset the reader and read again
//                    if (samplesRead == 0 && looped)
//                    {
//                        //reader.DecodedTime = TimeSpan.Zero;
//                        //samplesRead = reader.ReadSamples(nvBuffer, 0, nvBuffer.Length);
//                        module.SetPositionOrderRow(0, 0);
//                        samplesRead = (int)module.ReadInterleavedStereo(sampleRate, modBuffer.Length / 2, modBuffer);//reader.ReadSamples(nvBuffer, 0, nvBuffer.Length);
//                    }

//                    if (samplesRead > 0)
//                    {
//                        Buffer.BlockCopy(modBuffer, 0, buffer, 0, modBuffer.Length * sizeof(short));

//                        // submit our buffers
//                        lock (effect)
//                        {
//                            // ensure the effect isn't disposed
//                            if (effect.IsDisposed) { break; }

//                            effect.SubmitBuffer(buffer);
//                        }
//                    }
//                }

                

//                // reset our handle
//                needBufferEvent.Reset();
//            }
//        }

//        public void SetChannelVolume(float volume, int channel)
//        {
//            interactive.SetChannelVolume(channel, volume);
//        }

//        public void SetChannelVolumes(float volume, params int[] channels)
//        {
//            for (int i = 0; i < channels.Length; i++)
//            {
//                interactive.SetChannelVolume(channels[i], volume);
//            }
//        }

//        public void SetChannelMute(bool muted, int channel)
//        {
//            interactive.SetChannelMuteStatus(channel, muted);
//        }

//        public void SetChannelMutes(bool muted, params int[] channels)
//        {
//            for (int i = 0; i < channels.Length; i++)
//            {
//                interactive.SetChannelMuteStatus(channels[i], muted);
//            }
//        }

//        public void SelectSubsong(int pattern)
//        {
//            module.SelectSubsong(pattern);
            
//        }

//        public void SetPositionOrderRow(int order, int row)
//        {
//            module.SetPositionOrderRow(order, row);
            
//        }
//    }
//}
