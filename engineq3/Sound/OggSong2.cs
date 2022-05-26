//// ORIGINALLY WRITTEN BY NICK GRAVELYN
//// MODIFIED TO NOT USE THREADING
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using NVorbis;
//using Microsoft.Xna.Framework.Audio;
//using System.Threading;
//using Microsoft.Xna.Framework;

//namespace XboxTest.Sound
//{
//    class OggSong : IDisposable
//    {
//        private VorbisReader reader;
//        private DynamicSoundEffectInstance effect;

//        private byte[] buffer;
//        private float[] nvBuffer;

//        public SoundState State
//        {
//            get { return effect.State; }
//        }

//        public float Volume
//        {
//            get { return effect.Volume; }
//            set { effect.Volume = MathHelper.Clamp(value, 0, 1); }
//        }

//        public bool IsLooped { get; set; }

//        public OggSong(string oggFile)
//        {
//            reader = new VorbisReader(TitleContainer.OpenStream(oggFile), true);
//            effect = new DynamicSoundEffectInstance(reader.SampleRate, (AudioChannels)reader.Channels);
//            buffer = new byte[effect.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(500))];
//            nvBuffer = new float[buffer.Length / 2];

//            effect.BufferNeeded += new EventHandler<EventArgs>(FillBuffer);
//        }

//        void FillBuffer(object sender, EventArgs e)
//        {
//            // read the next chunk of data
//            int samplesRead = reader.ReadSamples(nvBuffer, 0, nvBuffer.Length);

//            // out of data and looping? reset the reader and read again
//            if (samplesRead == 0 && IsLooped)
//            {
//                reader.DecodedTime = TimeSpan.Zero;
//                samplesRead = reader.ReadSamples(nvBuffer, 0, nvBuffer.Length);
//            }

//            if (samplesRead > 0)
//            {
//                Submit(nvBuffer, samplesRead);
//            }
//        }

//        void Submit(float[] fBuffer, int samples)
//        {
//            // fr bruh
//            for (int i = 0; i < samples; i++)
//            {
//                short sValue = (short)Math.Max(Math.Min(short.MaxValue * fBuffer[i], short.MaxValue), short.MinValue);
//                buffer[i * 2] = (byte)(sValue & 0xff);
//                buffer[i * 2 + 1] = (byte)((sValue >> 8) & 0xff);
//            }

//            // ensure the effect isn't disposed
//            if (effect.IsDisposed) { return; }

//            effect.SubmitBuffer(buffer, 0, samples);
//            effect.SubmitBuffer(buffer, samples, samples);
//        }

//        ~OggSong()
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
//            //threadRunEvent.Set();
//            reader.Dispose();
//            effect.Dispose();
//        }

//        public void Play()
//        {
//            effect.Play();
//        }

//        public void Pause()
//        {
//            effect.Pause();
//        }

//        public void Resume()
//        {
//            effect.Resume();
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

//            reader.DecodedTime = TimeSpan.Zero;
//        }
//    }
//}
