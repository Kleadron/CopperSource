using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource
{
    public static class DataHelper
    {
        public static Vector3 QuaternionToEulerAngles(Quaternion rotation)
        {
            double q0 = rotation.W;
            double q1 = rotation.Y;
            double q2 = rotation.X;
            double q3 = rotation.Z;

            Vector3 radAngles = new Vector3();
            radAngles.Y = (float)Math.Atan2(2 * (q0 * q1 + q2 * q3), 1 - 2 * (Math.Pow(q1, 2) + Math.Pow(q2, 2)));
            radAngles.X = (float)Math.Asin(2 * (q0 * q2 - q3 * q1));
            radAngles.Z = (float)Math.Atan2(2 * (q0 * q3 + q1 * q2), 1 - 2 * (Math.Pow(q2, 2) + Math.Pow(q3, 2)));

            return radAngles;
        }

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

        public static string Vector3ToValue(Vector3 v)
        {
            string value = v.X.ToString() + " " + v.Y.ToString() + " " + v.Z.ToString();
            return value;
        }

        public static string Vector4ToValue(Vector4 v)
        {
            string value = v.X.ToString() + " " + v.Y.ToString() + " " + v.Z.ToString() + " " + v.W.ToString();
            return value;
        }

        public static string QuaternionToValue(Quaternion v)
        {
            string value = v.X.ToString() + " " + v.Y.ToString() + " " + v.Z.ToString() + " " + v.W.ToString();
            return value;
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
