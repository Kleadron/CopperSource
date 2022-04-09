using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;

namespace CopperSource
{
    public class WadFile
    {
        const string HEADER_MAGIC = "WAD3";
        const int MAXTEXTURENAME = 16;
        const int MIPLEVELS = 4;

        struct Header
        {
            public string magic;
            public int numFiles;
            public int directoryOffset;
        }

        public struct DirectoryEntry
        {
            public int fileOffset;
            public int size;
            public int uncompressedSize;
            public byte type;
            public bool compressed;
            public short unused;
            public string name;
        }

        // MipTexture moved to it's own file file

        Header header;
        public DirectoryEntry[] entries;
        //MipTexture[] textures;

        BinaryReader reader;

        public WadFile(string path)
        {
            Console.WriteLine("Loading WAD " + path);

            Stream fs = TitleContainer.OpenStream(path);
            reader = new BinaryReader(fs);

            Console.WriteLine("Reading Header");
            ReadHeader(reader);
            Console.WriteLine("Reading Directory");
            ReadDirectory(reader);
            //Console.WriteLine("Reading Textures");
            //ReadTextures(reader);

            //reader.Close();
        }

        public void Close()
        {
            reader.Close();
        }

        void ReadHeader(BinaryReader reader)
        {
            header.magic = ASCIIEncoding.ASCII.GetString(reader.ReadBytes(4));
            if (header.magic != HEADER_MAGIC)
            {
                throw new Exception("Invalid WAD3 File!");
            }

            header.numFiles = reader.ReadInt32();
            header.directoryOffset = reader.ReadInt32();
        }

        void ReadDirectory(BinaryReader reader)
        {
            reader.BaseStream.Position = header.directoryOffset;
            entries = new DirectoryEntry[header.numFiles];

            for (int i = 0; i < entries.Length; i++)
            {
                entries[i].fileOffset = reader.ReadInt32();
                entries[i].size = reader.ReadInt32();
                entries[i].uncompressedSize = reader.ReadInt32();

                entries[i].type = reader.ReadByte();
                entries[i].compressed = reader.ReadBoolean();

                entries[i].unused = reader.ReadInt16();

                byte[] name = reader.ReadBytes(MAXTEXTURENAME);
                string namestring = ASCIIEncoding.ASCII.GetString(name);
                namestring = namestring.Substring(0, namestring.IndexOf((char)0));
                entries[i].name = namestring;
            }
        }

        public bool HasFile(string name)
        {
            return GetFileEntry(name) != -1;
        }

        public int GetFileEntry(string name)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].name == name)
                    return i;
            }

            return -1;
        }

        public bool TryReadTexture(string name, out MipTexture texture)
        {
            texture = new MipTexture();

            int entryIndex = GetFileEntry(name);
            if (entryIndex != -1)
            {
                texture = ReadTexture(reader, entries[entryIndex].fileOffset);

                return true;
            }

            return false;
        }

        MipTexture ReadTexture(BinaryReader reader, int offset)
        {
            MipTexture miptex = new MipTexture();

            int baseMiptexPosition = offset;
            reader.BaseStream.Position = baseMiptexPosition;
            //textures[i].name = "";

            byte[] name = reader.ReadBytes(MAXTEXTURENAME);
            string namestring = ASCIIEncoding.ASCII.GetString(name);

            namestring = namestring.Substring(0, namestring.IndexOf((char)0));

            miptex.name = namestring;

            Console.WriteLine(namestring);

            miptex.width = reader.ReadInt32();
            miptex.height = reader.ReadInt32();

            uint mip0 = reader.ReadUInt32();
            uint mip1 = reader.ReadUInt32();
            uint mip2 = reader.ReadUInt32();
            uint mip3 = reader.ReadUInt32();

            if (miptex.width > 4096 || miptex.height > 4096)
            {
                Console.WriteLine("Texture size too big! " + namestring + " " + miptex.width + "x" + miptex.height);
                return miptex;
            }

            if (mip0 != 0)
            {
                reader.BaseStream.Position = baseMiptexPosition + mip0;
                miptex.mip0data = ReadTextureData(reader, miptex.width, miptex.height);
            }
            if (mip1 != 0)
            {
                reader.BaseStream.Position = baseMiptexPosition + mip1;
                miptex.mip1data = ReadTextureData(reader, miptex.width / 2, miptex.height / 2);
            }
            if (mip2 != 0)
            {
                reader.BaseStream.Position = baseMiptexPosition + mip2;
                miptex.mip2data = ReadTextureData(reader, miptex.width / 4, miptex.height / 4);
            }
            if (mip3 != 0)
            {
                reader.BaseStream.Position = baseMiptexPosition + mip3;
                miptex.mip3data = ReadTextureData(reader, miptex.width / 8, miptex.height / 8);
            }

            if (mip0 != 0 || mip1 != 0 || mip2 != 0 || mip3 != 0)
            {
                //reader.BaseStream.Position = header.lumps[LUMP_TEXTURES].offset + mip3 + ((textures[i].width / 8) * (textures[i].height / 8));
                reader.BaseStream.Position += 2;
                miptex.colorPalette = ReadColorPalette(reader);
            }

            return miptex;
        }

        // set stream position before reading!
        byte[] ReadTextureData(BinaryReader reader, int width, int height)
        {
            byte[] colors = reader.ReadBytes(width * height);
            return colors;
        }

        Color[] ReadColorPalette(BinaryReader reader)
        {
            Color[] palette = new Color[256];

            for (int i = 0; i < palette.Length; i++)
            {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();

                palette[i] = new Color(r, g, b);
            }

            // last index is always transparent?
            //if (palette[255].R == 0 && palette[255].G == 0 && palette[255].B == 255)
            //    palette[255] = Color.Transparent;

            return palette;
        }
    }
}
