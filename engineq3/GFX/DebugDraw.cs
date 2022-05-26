using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace KSoft.Client.GFX
{
    // SLOW IMMEDIATE MODE GRAPHICS API
    public static class DebugDraw
    {
        static GraphicsDevice device;
        static BasicEffect effect;

        static VertexPositionColor[] vBuffer = new VertexPositionColor[] 
        {
            new VertexPositionColor(Vector3.Zero, Color.White),
            new VertexPositionColor(Vector3.Zero, Color.White)
        };

        public static void Load(GraphicsDevice device)
        {
            DebugDraw.device = device;
            effect = new BasicEffect(device);
        }

        public static void DrawLine(Camera camera, Vector3 p1, Vector3 p2, Color c1, Color c2)
        {
            effect.World = Matrix.Identity;
            effect.View = camera.view;
            effect.Projection = camera.proj;
            vBuffer[0] = new VertexPositionColor(p1, c1);
            vBuffer[1] = new VertexPositionColor(p2, c2);
            effect.VertexColorEnabled = true;
            effect.CurrentTechnique.Passes[0].Apply();
            device.DrawUserPrimitives(PrimitiveType.LineList, vBuffer, 0, 1);
        }

        public static void Unload()
        {

        }
    }
}
