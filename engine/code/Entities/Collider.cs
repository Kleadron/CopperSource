using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CopperSource.code.Entities
{
    class Collider : Component
    {
        // implementation of a Collider component

        public Collider()
        {
            ColliderSystem.Register(this);

        }

        //int GetAmount()
        //{
        //   return ColliderSystem.components.Count();
        //}

    }
}
