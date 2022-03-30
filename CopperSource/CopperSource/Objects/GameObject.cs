using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource.Objects
{
    public abstract class GameObject
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;

        public void Deserialize(Dictionary<string, string> data)
        {
            position = DataHelper.ValueToVector3(data["pos"]);
        }

        public void Serialize(Dictionary<string, string> data)
        {

        }

        public virtual void Update(float delta, float total)
        {

        }

        public virtual void Draw(float delta, float total)
        {

        }
    }
}
