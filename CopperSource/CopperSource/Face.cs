using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CopperSource
{
    public enum FaceType
    {
        Solid,
        DontDraw
    }

    public class Face
    {
        public int id;
        public int textureID;
        public int lightmapID;
        public Plane plane;
        public FaceType type;
        public int start, baseVertex, numVerts, triCount;
        public int indicesStart, indicesLength;
    }
}
