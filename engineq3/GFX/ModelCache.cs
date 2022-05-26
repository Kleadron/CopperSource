using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSoft.Client.GFX
{
    // meant to store pre-built ModelBuilder models
    public static class ModelCache
    {
        static Dictionary<string, GraphicsMesh> models = new Dictionary<string, GraphicsMesh>();

        public static void Put(string name, GraphicsMesh model)
        {
            models[name] = model;
        }

        public static GraphicsMesh Get(string name)
        {
            return models[name];
        }

        public static void Clear()
        {
            models.Clear();
        }
    }
}
