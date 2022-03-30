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
            textures.Sort(SortFuncByHeight);

            // The lightmap data is RGB!
            // The texture's SurfaceFormat is RGBA!
            texData = new byte[PAGE_SIZE * PAGE_SIZE * 4];
        }

        // test this
        int SortFuncByHeight(LMTex left, LMTex right)
        {
            if (left.height > right.height)
                return -1;
            if (left.height < right.height)
                return 1;
            return 0;
        }
    }

    public class LMTex
    {
        public int offset;
        public int width;
        public int height;
    }
}
