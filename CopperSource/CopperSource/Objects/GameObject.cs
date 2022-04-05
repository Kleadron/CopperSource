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
        public string tag; // any data can go here
        public Vector3 position;
        public Quaternion rotation;

        // key values used for game saves?
        // if inherited, add custom keyvalues to this
        public virtual List<string> GetSavableKeys()
        {
            List<string> keys = new List<string>();
            keys.Add("name");
            keys.Add("tag");
            keys.Add("position");
            keys.Add("rotation");
            //keys.Add("origin");
            return keys;
        }

        public virtual void SetKeyValue(string key, string value) 
        {
            switch (key)
            {
                case "name":
                    name = value;
                    break;
                case "tag":
                    tag = value;
                    break;
                case "position":
                case "origin":
                    position = DataHelper.ValueToVector3(value);
                    break;
                case "angles":
                    Vector3 rotationAngles = DataHelper.ValueToVector3(value);
                    rotation = Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
                    break;
                case "rotation":
                    rotation = DataHelper.ValueToQuaternion(value);
                    break;
            }

            // call base.SetKeyValue if this is inherited
        }

        public virtual string GetKeyValue(string key)
        {
            switch (key)
            {
                case "name":
                    return name;
                case "tag":
                    return tag;
                case "position":
                case "origin":
                    return DataHelper.Vector3ToValue(position);
                case "rotation":
                    return DataHelper.QuaternionToValue(rotation);
                case "angles":
                    return DataHelper.Vector3ToValue(DataHelper.QuaternionToEulerAngles(rotation));
            }

            // return base.GetKeyValue if this is inherited
            return null;
        }

        public virtual void Update(float delta, float total)
        {

        }

        public virtual void Draw(float delta, float total)
        {

        }
    }
}
