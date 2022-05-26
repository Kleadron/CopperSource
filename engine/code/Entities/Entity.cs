using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource.Entities
{
    public class Entity
    {
        // identity
        public int id;
        public string name;
        public string tag; // any data can go here
        public string classname;
        public bool active = true;

        // transform
        //public Vector3 position;
        //public Quaternion rotation;
        //public Vector3 originOffset;

        // internal
        //protected Engine engine;

        // components
        public List<Component> components = new List<Component>();

        public void AddComponent(Component component)
        {
            components.Add(component);
            component.entity = this;
            component.Added();
        }

        public T AddComponent<T>() where T : Component
        {
            T component = Activator.CreateInstance<T>();
            AddComponent(component);
            return component;
        }

        public T GetComponent<T>() where T : Component
        {
            foreach (Component component in components)
            {
                if (component is T)
                {
                    return (T)component;
                }
            }
            return null;
        }

        public void RemoveComponent(Component c)
        {
            components.Remove(c);
            c.Removed();
        }

        public void RemoveComponent<T>() where T : Component
        {
            Component toRemove = null;
            foreach (Component component in components)
            {
                if (component.GetType().Equals(typeof(T)))
                {
                    toRemove = (Component)component;
                    break;
                }
            }

            if (toRemove != null)
                RemoveComponent(toRemove);
        }

        public void Initialize()
        {
            foreach (Component c in components)
            {
                c.Initialize();
            }
        }

        public void LoadKeyValues(Dictionary<string, string> keyValues)
        {
            foreach (Component c in components)
            {
                c.LoadKeyValues(keyValues);
            }
        }

        //public Vector3 WorldOrigin
        //{
        //    get
        //    {
        //        return position + originOffset;
        //    }
        //}

        //public bool IsOriginVisible
        //{
        //    get
        //    {
        //        return engine.PointIsVisible(WorldOrigin);
        //    }
        //}

        //public bool IsOriginInsideWorld
        //{
        //    get
        //    {
        //        return engine.GetLeafFromPosition(WorldOrigin).id != 0;
        //    }
        //}

        //public Entity(Engine engine)
        //{
        //    this.engine = engine;
        //}

        // key values used for game saves?
        // if inherited, add custom keyvalues to this
        //public virtual List<string> GetSavableKeys()
        //{
        //    List<string> keys = new List<string>();
        //    keys.Add("name");
        //    keys.Add("tag");
        //    keys.Add("position");
        //    keys.Add("rotation");
        //    //keys.Add("origin");
        //    return keys;
        //}

        //public virtual void SetKeyValue(string key, string value) 
        //{
        //    switch (key)
        //    {
        //        case "name":
        //            name = value;
        //            break;
        //        case "classname":
        //            classname = value;
        //            break;
        //        case "tag":
        //            tag = value;
        //            break;
        //        case "position":
        //        case "origin":
        //            position = DataHelper.ValueToVector3(value);
        //            break;
        //        case "angles":
        //            Vector3 rotationAngles = DataHelper.ValueToVector3(value);
        //            rotation = Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        //            break;
        //        case "rotation":
        //            rotation = DataHelper.ValueToQuaternion(value);
        //            break;
        //    }

        //    // call base.SetKeyValue if this is inherited
        //}

        //public virtual string GetKeyValue(string key)
        //{
        //    switch (key)
        //    {
        //        case "name":
        //            return name;
        //        case "classname":
        //            return classname;
        //        case "tag":
        //            return tag;
        //        case "position":
        //        case "origin":
        //            return DataHelper.Vector3ToValue(position);
        //        case "rotation":
        //            return DataHelper.QuaternionToValue(rotation);
        //        case "angles":
        //            return DataHelper.Vector3ToValue(DataHelper.QuaternionToEulerAngles(rotation));
        //    }

        //    // return base.GetKeyValue if this is inherited
        //    return null;
        //}

        //public virtual void Initialize()
        //{

        //}

        //public virtual void Update(float delta, float total)
        //{

        //}

        //public virtual void Draw(float delta, float total)
        //{

        //}
    }
}
