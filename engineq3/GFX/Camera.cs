using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KSoft.Client.GFX
{
    public class Camera
    {
        //public static Camera MAIN;
        public const float DEFAULT_FOV = 70f;

        public Matrix view;
        public Matrix proj;

        public float fov = DEFAULT_FOV; // in degrees
        public Vector3 position;
        public Vector3 lookDirection;

        public void Update()
        {

        }
    }
}
