using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CopperSource.Entities
{
    class BaseSystem<T> where T : Component
    {

        public static List<T> components = new List<T>();

        public static void Register(T component)
        {
            components.Add(component);
        }

        public static void Update(GameTime gameTime)
        {
            foreach (T component in components)
            {
                component.Update(gameTime);
            }
        }

    }


    
    
    class ColliderSystem : BaseSystem<Collider> { }
}
