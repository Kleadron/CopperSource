using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource.Entities
{
    public class BrushEntity : Entity
    {
        public BrushEntity(Engine engine) : base(engine)
        {
            // Example of adding a component to a entity

           /* Collider collider = new Collider();
            AddComponent(collider);*/
        }

        int modelIndex;
        public BspModel model;
        BoundingBox bb;
        RenderMode renderMode;
        Color renderColor = Color.White;
        float renderFXAmount;

       


        public override void SetKeyValue(string key, string value)
        {
            if (key == "model")
            {
                modelIndex = int.Parse(value.Substring(1));
            }

            if (key == "rendermode")
            {
                renderMode = (RenderMode)int.Parse(value);
                if (renderMode == RenderMode.Texture || renderMode == RenderMode.Color)
                {
                    renderMode = RenderMode.Dither_EXT;
                }
            }

            if (key == "rendercolor")
            {
                try
                {
                    Vector3 v = DataHelper.ValueToVector3(value);
                    v /= 256;
                    if (v.Length() > 0)
                    {
                        renderColor = new Color(v.X, v.Y, v.Z);
                    }
                }
                catch (Exception)
                {

                }
            }

            if (key == "renderamt")
            {
                renderFXAmount = float.Parse(value) / 255f;
            }

            base.SetKeyValue(key, value);
        }

        public override void Initialize()
        {
            model = engine.models[modelIndex];
            originOffset = model.center;
            bb = new BoundingBox(model.bb.Min + position, model.bb.Max + position);
            //renderColor.A = renderFXAmount;
        }

        public override void Draw(float delta, float total)
        {
            //if (IsOriginVisible || !IsOriginInsideWorld)
            if (engine.BBIsVisible(ref bb))
            {
                //game.SetModelTransform(Matrix.CreateTranslation(position));
                //game.RecursiveTreeDraw(model.rootNode, game.TransformedVisPosition);
                //game.DrawBspModel(model, Matrix.CreateTranslation(position));
                if (position != Vector3.Zero || renderMode == RenderMode.Dither_EXT || renderMode == RenderMode.Additive)
                {
                    Color color = renderColor;
                    if (renderMode == RenderMode.Additive)
                    {
                        color *= renderFXAmount;
                    }
                    engine.QueueDynamicBspModel(model, Matrix.CreateTranslation(position), renderMode, color);
                }
                else
                {
                    engine.QueueStaticBspModel(model);
                }
            }
        }
    }
}
