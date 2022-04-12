using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;

namespace CopperSource
{
    public class LightmapAtlas
    {
        // texture size is PAGE_SIZExPAGE_SIZE
        public int atlasSize = 256;
        public int sizeIncrement = 64;

        GraphicsDevice device;
        Color[] texData;

        public Texture2D texture;
        public LMTexUV[] uvs;

        public LightmapAtlas(GraphicsDevice device, List<LMTexEntry> textures, byte[] lightmapData)
        {
            Console.WriteLine("Lightmap Atlas: Compiling " + textures.Count + " textures");

            this.device = device;
            textures.Sort(SortFuncBySize);

            //for (int i = 0; i < texData.Length; i++)
            //{
            //    texData[i] = Color.Magenta;
            //}

            uvs = new LMTexUV[textures.Count];

            int rowY = 0;
            int rowX = 0;

            int tallestTextureThisRow = 0;
            bool sizeFits = false;

            Console.WriteLine("Lightmap Atlas: Testing Size");

            while (!sizeFits)
            {
                sizeFits = true;

                rowX = 0;
                rowY = 0;
                tallestTextureThisRow = 0;

                for (int i = 0; i < textures.Count; i++)
                {
                    LMTexEntry tex = textures[i];

                    if (rowX + tex.width > atlasSize)
                    {
                        bool foundSuitableSize = false;

                        // find a smaller texture
                        //for (int j = i; j < textures.Count; j++)
                        //{
                        //    if (rowX + textures[j].width <= PAGE_WIDTH && textures[j].height == tex.height)
                        //    {
                        //        foundSuitableSize = true;
                        //        tex = textures[j];
                        //    }
                        //}

                        // advance the row
                        if (!foundSuitableSize)
                        {
                            rowX = 0;
                            rowY += tallestTextureThisRow;
                            tallestTextureThisRow = 0;

                            if (rowY + tex.height > atlasSize)
                            {
                                //throw new Exception("Too many textures to build Lightmap atlas!");
                                int nextSize = atlasSize + sizeIncrement;

                                // untested on FNA but should also be allowed
#if XNA
                                if (nextSize > 4096)
                                {
                                    throw new Exception("Too many textures to build Lightmap atlas! Why is your map so big!");
                                }
#endif

                                Console.WriteLine("Lightmap Atlas: " + atlasSize + "x" + atlasSize + " is too small, trying " + nextSize + "x" + nextSize);
                                sizeFits = false;
                                atlasSize = nextSize;
                                break;
                            }
                        }
                    }

                    if (tallestTextureThisRow < tex.height)
                        tallestTextureThisRow = tex.height;

                    rowX += tex.width;
                }
            }

            // The lightmap data is RGB!
            // The texture's SurfaceFormat is RGBA/Color!
            texData = new Color[atlasSize * atlasSize];

            rowX = 0;
            rowY = 0;
            tallestTextureThisRow = 0;

            for (int i = 0; i < textures.Count; i++)
            {
                LMTexEntry tex = textures[i];

                if (rowX + tex.width > atlasSize)
                {
                    bool foundSuitableSize = false;

                    // find a smaller texture
                    //for (int j = i; j < textures.Count; j++)
                    //{
                    //    if (rowX + textures[j].width <= PAGE_WIDTH && textures[j].height == tex.height)
                    //    {
                    //        foundSuitableSize = true;
                    //        tex = textures[j];
                    //    }
                    //}

                    // advance the row
                    if (!foundSuitableSize)
                    {
                        rowX = 0;
                        rowY += tallestTextureThisRow;
                        tallestTextureThisRow = 0;

                        //if (rowY + tex.height > atlasSize)
                        //{
                        //    throw new Exception("Too many textures to build Lightmap atlas!");
                        //}
                    }
                }

                if (tallestTextureThisRow < tex.height)
                    tallestTextureThisRow = tex.height;

                CopyLightmapData(lightmapData, tex.offset, new Rectangle(rowX, rowY, tex.width, tex.height), tex.id);
                rowX += tex.width;
            }

            texture = new Texture2D(device, atlasSize, atlasSize, false, SurfaceFormat.Color);
            texture.SetData(texData);
            texData = null;

            Console.WriteLine("Lightmap Atlas: Done, " + atlasSize + "x" + atlasSize + " size atlas");
            KConsole.Log("Lightmap atlas is " + atlasSize + "x" + atlasSize);

            //texture.SaveAsPng(File.Open("lightmap.png", FileMode.Create), atlasSize, atlasSize);
            //texture = Texture2D.FromStream(device, File.Open("jpeg.png", FileMode.Open));
        }

        // first height then width
        int SortFuncBySize(LMTexEntry left, LMTexEntry right)
        {
            if (left.height > right.height)
                return -1;
            if (left.height < right.height)
                return 1;

            if (left.width > right.width)
                return -1;
            if (left.width < right.width)
                return 1;

            return 0;
        }

        // texture width and height derived from destination rect
        void CopyLightmapData(byte[] lightmapData, int offset, Rectangle destination, int id)
        {
            //Console.WriteLine(width + "x" + height);

            int width = destination.Width;
            int height = destination.Height;

            uvs[id].min.X = destination.Left + 0.5f;
            uvs[id].min.Y = destination.Top + 0.5f;
            uvs[id].max.X = destination.Right - 0.5f;
            uvs[id].max.Y = destination.Bottom - 0.5f;

            uvs[id].min /= atlasSize;
            uvs[id].max /= atlasSize;

            //Color[] colors = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pageX = destination.X + x;
                    int pageY = destination.Y + y;

                    int dataIndex = (x + (y * width)) * 3;
                    int colorIndex = (pageX + (pageY * atlasSize));
                    Color c = Color.White;
                    c.R = lightmapData[offset + dataIndex];
                    c.G = lightmapData[offset + dataIndex + 1];
                    c.B = lightmapData[offset + dataIndex + 2];

                    texData[colorIndex] = c;
                }
            }

            
        }
    }

    public class LMTexEntry
    {
        public int id;
        public int offset;
        public int width;
        public int height;
    }

    public struct LMTexUV
    {
        public Vector2 min, max;
    }
}
