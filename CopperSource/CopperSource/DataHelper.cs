using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource
{
    public static class DataHelper
    {
        public static Vector2 ValueToVector2(string value)
        {
            Vector2 v;
            string[] split = value.Split(' ');
            v.X = float.Parse(split[0]);
            v.Y = float.Parse(split[1]);
            return v;
        }

        public static Vector3 ValueToVector3(string value)
        {
            Vector3 v;
            string[] split = value.Split(' ');
            v.X = float.Parse(split[0]);
            v.Y = float.Parse(split[1]);
            v.Z = float.Parse(split[2]);
            return v;
        }

        public static Vector4 ValueToVector4(string value)
        {
            Vector4 v;
            string[] split = value.Split(' ');
            v.X = float.Parse(split[0]);
            v.Y = float.Parse(split[1]);
            v.Z = float.Parse(split[2]);
            v.W = float.Parse(split[3]);
            return v;
        }

        public static Quaternion ValueToQuaternion(string value)
        {
            Quaternion v;
            string[] split = value.Split(' ');
            v.X = float.Parse(split[0]);
            v.Y = float.Parse(split[1]);
            v.Z = float.Parse(split[2]);
            v.W = float.Parse(split[3]);
            return v;
        }
    }
}
