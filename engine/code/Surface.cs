using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CopperSource
{
    public enum SurfaceRenderType
    {
        Solid,
        DontDraw
    }

    public class Surface
    {
        public int id;
        public int textureID;
        public int lightmapID;
        public Plane plane;
        public SurfaceRenderType type;
        public int start, baseVertex, numVerts, triCount;
        public int indicesStart, indicesLength;
    }
}
