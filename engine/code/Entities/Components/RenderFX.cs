using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource.Entities.Components
{
    // holds RenderFX info
    public class RenderFX : Component
    {
        public RenderMode mode = RenderMode.Normal;
        public Color color = Color.White;
        public float amount = 1f;
    }
}
