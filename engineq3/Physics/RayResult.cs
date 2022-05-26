using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KSoft.Client.Physics
{
    public struct RayResult
    {
        //public Vector3 hitPosition;
        public Vector3 surfaceNormal;
        public float distance;
    }
}
