using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource.Entities.Components
{
    // seems overkill, this is a common trait all entities should have?
    // although entities exist without positions too...
    public class Transform : Component
    {
        public Vector3 position;
        public Quaternion rotation;

        public override void LoadKeyValues(Dictionary<string, string> keyValues)
        {
            
        }
    }
}
