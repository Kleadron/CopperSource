using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CopperSource
{
    public static class Extensions
    {
        // corrects lighting direction
        public static void Hammerize(this DirectionalLight light)
        {
            Vector3 oldDirection = light.Direction;
            Vector3 newDirection = Vector3.Zero;
            newDirection.Z = oldDirection.Y;
            newDirection.Y = oldDirection.X;
            newDirection.X = -oldDirection.Z;
            light.Direction = newDirection;
        }
    }
}
