using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource.Objects
{
    public class BrushEntity : Entity
    {
        public BrushEntity(Game1 game) : base(game)
        {

        }

        public BspModel model;

        public override void SetKeyValue(string key, string value)
        {
            if (key == "model")
            {
                int modelIndex = int.Parse(value.Substring(1));
                model = game.models[modelIndex];
                originOffset = model.center;
                //Console.WriteLine(model.center);
            }

            base.SetKeyValue(key, value);
        }

        public override void Draw(float delta, float total)
        {
            if (IsOriginVisible || !IsOriginInsideWorld)
            {
                //game.SetModelTransform(Matrix.CreateTranslation(position));
                //game.RecursiveTreeDraw(model.rootNode, game.TransformedVisPosition);
                //game.DrawBspModel(model, Matrix.CreateTranslation(position));
                if (position != Vector3.Zero)
                {
                    game.QueueDynamicBspModel(model, Matrix.CreateTranslation(position));
                }
                else
                {
                    game.QueueStaticBspModel(model);
                }
            }
        }
    }
}
