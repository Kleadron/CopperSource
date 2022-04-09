using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource
{
    public struct MipTexture
    {
        public string name;
        public int width, height;
        public byte[] mip0data;
        public byte[] mip1data;
        public byte[] mip2data;
        public byte[] mip3data;
        public Color[] colorPalette;
    }
}
