using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CopperSource.Entities
{
    public class Component
    {
        // the entity that owns this component, or the "parent"
        public Entity entity;

        // called when the entity is loading it's data
        public virtual void LoadKeyValues(Dictionary<string, string> keyValues) { }

        // called when the entity is intialized
        public virtual void Initialize() { }

        // called when the component's system is updated
        public virtual void Update(GameTime gameTime) { }

        // called after the component is added to it's entity
        public virtual void Added() { }

        // called after the component is removed from it's entity
        public virtual void Removed() { }
    }
}
