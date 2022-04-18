using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CopperSource.code.Entities
{
    public class Component
    {
        public Entity entity;

        public virtual void Update(GameTime gameTime) { }
    }
}
