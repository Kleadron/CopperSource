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
        public byte[][] mipData;
        public Color[] colorPalette;
    }
}
