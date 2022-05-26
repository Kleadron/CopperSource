using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CopperSource.Entities.Components;

namespace CopperSource.Entities
{
    public static class EntityBuilder
    {
        //Func<Dictionary<string, string>, Entity> e;

        //static Engine engine;

        //public static void Initialize(Engine e)
        //{
        //    engine = e;
        //}

        static void BuildWorldSpawn(Entity e)
        {
            e.AddComponent<WorldInfo>();
        }

        static void BuildPointEntity(Entity e)
        {
            e.AddComponent<Transform>();
        }

        static void BuildBrushEntity(Entity e)
        {
            e.AddComponent<Transform>();
            e.AddComponent<RenderFX>();
            e.AddComponent<BspModelRenderer>();
        }

        public static Entity BuildEntity(Dictionary<string, string> keyValues)
        {
            string classname = keyValues["classname"];
            Entity entity = new Entity();
            entity.classname = classname;
            
            // add components


            // initialization
            entity.LoadKeyValues(keyValues);
            entity.Initialize();


            return entity;
        }
    }
}
