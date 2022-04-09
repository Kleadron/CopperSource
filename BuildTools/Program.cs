using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;

namespace BuildUtils
{
    class Program
    {
        public static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                Work(args[i]);
            }
        }

        static void Work(string filename)
        {
            EffectProcessor processor = new EffectProcessor();
            EffectContent content = new EffectContent();
            content.Identity = new ContentIdentity(filename);
            content.EffectCode = File.ReadAllText(filename);
            MyProcessorContext context = new MyProcessorContext();
            CompiledEffectContent compiled = processor.Process(content, context);
            byte[] effectCode = compiled.GetEffectCode();

            string newFilename = Path.GetFileNameWithoutExtension(filename) + ".xfxb";
            File.WriteAllBytes(newFilename, effectCode);
        }
    }
}
