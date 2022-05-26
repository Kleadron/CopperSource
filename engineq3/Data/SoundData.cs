using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KSoft.Client.Data
{
    public class SoundData
    {
        public int sampleRate;
        public int channels;
        public byte[] data;

        public SoundData(string path)
        {
            FileStream fs = File.OpenRead(path);
            BinaryReader reader = new BinaryReader(fs);

            string fileHeader = new string(Encoding.ASCII.GetChars(reader.ReadBytes(4)));

            if (fileHeader != "RIFF")
            {
                throw new Exception("Wave file is not in RIFF format!");
            }

            // should add actual stuff to go here...

            reader.Close();
            fs.Close();
        }
    }
}
