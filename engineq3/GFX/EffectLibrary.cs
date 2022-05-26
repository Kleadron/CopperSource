using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using KSoft.Client.Common;

namespace KSoft.Client.GFX
{
    public class EffectLibrary
    {
        public Dictionary<string, BasicEffect> effects = new Dictionary<string, BasicEffect>();

        public EffectLibrary(ModelData modelData, GraphicsDevice device)
        {
            foreach (ModelData.Material mat in modelData.materials)
            {
                BasicEffect effect = new BasicEffect(device);
            }
        }
    }
}
