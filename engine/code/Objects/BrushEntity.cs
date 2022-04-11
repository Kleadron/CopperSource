using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource.Objects
{
    public class BrushEntity : Entity
    {
        public BrushEntity(Engine engine) : base(engine)
        {

        }

        int modelIndex;
        public BspModel model;
        BoundingBox bb;

        public override void SetKeyValue(string key, string value)
        {
            if (key == "model")
            {
                modelIndex = int.Parse(value.Substring(1));
                //Console.WriteLine(model.center);
            }

            base.SetKeyValue(key, value);
        }

        public override void Initialize()
        {
            model = engine.models[modelIndex];
            originOffset = model.center;
            bb = new BoundingBox(model.bb.Min + position, model.bb.Max + position);
        }

        public override void Draw(float delta, float total)
        {
            //if (IsOriginVisible || !IsOriginInsideWorld)
            if (engine.BBIsVisible(ref bb))
            {
                //game.SetModelTransform(Matrix.CreateTranslation(position));
                //game.RecursiveTreeDraw(model.rootNode, game.TransformedVisPosition);
                //game.DrawBspModel(model, Matrix.CreateTranslation(position));
                if (position != Vector3.Zero)
                {
                    engine.QueueDynamicBspModel(model, Matrix.CreateTranslation(position));
                }
                else
                {
                    engine.QueueStaticBspModel(model);
                }
            }
        }
    }
}
