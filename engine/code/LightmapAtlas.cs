using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace CopperSource
{
    public class LightmapAtlas
    {
        // texture size is PAGE_SIZExPAGE_SIZE
        public const int PAGE_SIZE = 4096;

        GraphicsDevice device;
        byte[] texData;
        Texture2D texture;

        public LightmapAtlas(GraphicsDevice device, List<LMTex> textures, byte[] lightmapData)
        {
            this.device = device;
            textures.Sort(SortFuncBySize);

            // The lightmap data is RGB!
            // The texture's SurfaceFormat is RGBA!
            texData = new byte[PAGE_SIZE * PAGE_SIZE * 4];
        }

        // first height then width
        int SortFuncBySize(LMTex left, LMTex right)
        {
            if (left.height > right.height)
                return -1;
            if (left.height < right.height)
                return 1;
            return 0;
        }

        //int CreateLightmapTexture(byte[] lightmapData, int offset, int width, int height)
        //{
        //    //Console.WriteLine(width + "x" + height);

        //    int texIndex = lightmapList.Count;
        //    Texture2D tex = new Texture2D(GraphicsDevice, width, height, false, SurfaceFormat.Color);
        //    lightmapList.Add(tex);

        //    Color[] colors = new Color[width * height];
        //    byte[] lmData = lightmapData;

        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            int dataIndex = (x + (y * width)) * 3;
        //            int colorIndex = (x + (y * width));
        //            Color c = Color.White;
        //            c.R = lmData[offset + dataIndex];
        //            c.G = lmData[offset + dataIndex + 1];
        //            c.B = lmData[offset + dataIndex + 2];

        //            colors[colorIndex] = c;
        //        }
        //    }

        //    tex.SetData(colors);

        //    return texIndex;
        //}
    }

    public class LMTex
    {
        public int offset;
        public int width;
        public int height;
    }
}
