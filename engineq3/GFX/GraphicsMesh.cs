using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace KSoft.Client.GFX
{
    // named KModel because Model is already taken by XNA
    public class GraphicsMesh : IDisposable
    {
        GraphicsDevice device;
        public VertexBuffer vb;
        public IndexBuffer ib;

        public FaceGroup[] faceGroups;
        public BasicEffect[] effects;
        public int renderGroups;

        BasicEffect shadowEffect;

        // culling information
        //public BoundingBox bb;
        //public BoundingSphere bs;

        //public BoundingBox GetTransformedBoundingBox(Matrix transform)
        //{
            
        //    return default(BoundingBox);
        //}

        //public BoundingSphere GetTransformedBoundingSphere(Matrix transform)
        //{
        //    return bs.Transform(transform);
        //}

        public GraphicsMesh(GraphicsDevice device)
        {
            this.device = device;
            //effect.DiffuseColor = Color.Indigo.ToVector3();
            shadowEffect = new BasicEffect(device);
            shadowEffect.DiffuseColor = Lighting.ShadowColor;
        }

        public void Draw(Matrix world, Camera camera)
        {
            Draw(world, camera.view, camera.proj);
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            if (vb == null || ib == null)
                return;

            device.SetVertexBuffer(vb);
            device.Indices = ib;

            for (int i = 0; i < renderGroups; i++)
            {
                FaceGroup group = faceGroups[i];
                BasicEffect effect = effects[i];

                effect.World = world;
                effect.View = view;
                effect.Projection = projection;

                effect.CurrentTechnique.Passes[0].Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, group.firstVertex, 0, group.totalVertices, group.firstIndex, group.totalPrimitives);
            }
            
        }

        // these functions draw the mesh with a pure black color

        public void DrawShadow(Matrix world, Camera camera)
        {
            DrawShadow(world, camera.view, camera.proj);
        }

        public void DrawShadow(Matrix world, Matrix view, Matrix projection)
        {
            if (vb == null || ib == null)
                return;

            shadowEffect.World = world;
            shadowEffect.View = view;
            shadowEffect.Projection = projection;

            Engine.I.Set3DShadowState();

            for (int i = 0; i < renderGroups; i++)
            {
                FaceGroup group = faceGroups[i];

                device.SetVertexBuffer(vb);
                device.Indices = ib;

                shadowEffect.CurrentTechnique.Passes[0].Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, group.firstVertex, 0, group.totalVertices, group.firstIndex, group.totalPrimitives);
            }

            Engine.I.Set3DState();

        }

        public void Dispose()
        {
            if (vb != null)
                vb.Dispose();
            if (ib != null)
                ib.Dispose();

            shadowEffect.Dispose();
        }
    }
}
