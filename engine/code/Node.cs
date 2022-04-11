using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource
{
    public class Node
    {
        public int id;
        public int modelID;
        public Node parentNode;
        public Node frontNode, backNode;
        public Leaf frontLeaf, backLeaf;
        public Plane plane;
        public BoundingBox bb;
        public int firstFace, nFaces;
    }
}
