using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource
{
    public class Leaf
    {
        public int id;
        public int modelID;
        public Node parentNode;
        public BoundingBox bb;
        public int firstMarkSurface, nMarkSurfaces;
        public int visCluster;
    }
}
