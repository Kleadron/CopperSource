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
            if (IsOriginVisible)
            {
                game.SetModelTransform(Matrix.CreateTranslation(position));
                game.RecursiveTreeDraw(model.rootNode, game.TransformedVisPosition);
            }
        }
    }
}
