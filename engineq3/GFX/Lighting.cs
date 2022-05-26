using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KSoft.Client.GFX
{
    public static class Lighting
    {
        public static Vector3 LightSourceDirection;
        public static Vector3 LightDirection;
        public static Vector3 ShadowColor;

        static Lighting()
        {
            Vector3 lightSource = new Vector3(-0.2f, 0.9f, -0.1f);
            lightSource.Normalize();

            LightSourceDirection = lightSource;
            LightDirection = -LightSourceDirection;

            ShadowColor = Vector3.One * 0.5f;
        }
    }
}
