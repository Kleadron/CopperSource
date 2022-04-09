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

        const byte TYPE_QPIC = 0x42;
        const byte TYPE_MIPTEX = 0x43;
        const byte TYPE_FONT = 0x46;

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

        public struct CharInfo
        {
            public short texOffset;
            public short charWidth;
        }

        public struct Font
        {
            public int width;
            public int height;
            public int rowCount;
            public int rowHeight;
            public CharInfo[] info;
            public byte[] data;
            public short colorCount;
            public Color[] colorPalette;
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
                entries[i].name = namestring.ToUpper();
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
                // not a mip texture
                if (entries[entryIndex].type != TYPE_MIPTEX)
                {
                    Console.WriteLine(name + " is not a texture!");
                    return false;
                }

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

            //Console.WriteLine(namestring);

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

        // shamelessly copied code (trollface)
        public bool TryReadFont(string name, out Font texture)
        {
            texture = new Font();

            int entryIndex = GetFileEntry(name);
            if (entryIndex != -1)
            {
                // not a font
                if (entries[entryIndex].type != TYPE_FONT)
                {
                    Console.WriteLine(name + " is not a font!");
                    return false;
                }

                texture = ReadFont(reader, entries[entryIndex].fileOffset);

                return true;
            }

            return false;
        }

        Font ReadFont(BinaryReader reader, int offset)
        {
            Font font = new Font();

            reader.BaseStream.Position = offset;

            font.width = reader.ReadInt32();
            font.height = reader.ReadInt32();
            font.rowCount = reader.ReadInt32();
            font.rowHeight = reader.ReadInt32();

            font.info = new CharInfo[256];

            for (int i = 0; i < font.info.Length; i++)
            {
                font.info[i].texOffset = reader.ReadInt16();
                font.info[i].charWidth = reader.ReadInt16();
            }

            font.data = ReadTextureData(reader, 256, font.height);

            font.colorCount = reader.ReadInt16();

            // hope that's right
            font.colorPalette = ReadColorPalette(reader, 256);

            return font;
        }

        // set stream position before reading!
        byte[] ReadTextureData(BinaryReader reader, int width, int height)
        {
            byte[] colors = reader.ReadBytes(width * height);
            return colors;
        }

        Color[] ReadColorPalette(BinaryReader reader, int colors = 256)
        {
            Color[] palette = new Color[colors];

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
