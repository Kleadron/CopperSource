using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource
{
    public class BspModel
    {
        public Node rootNode;
        public BoundingBox bb;
        public Vector3 center;
        public Vector3 rotationalOrigin;
        public int numLeaves;
        public int firstFace;
        public int numFaces;
    }
}
