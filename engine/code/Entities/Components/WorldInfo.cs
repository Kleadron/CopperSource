using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CopperSource.Entities.Components
{
    public class WorldInfo : Component
    {
        public string[] wads;
        public string chapterTitle;
        public string message;

        public override void LoadKeyValues(Dictionary<string, string> keyValues)
        {
            if (keyValues.ContainsKey("wad"))
                wads = keyValues["wad"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            keyValues.TryGetValue("chaptertitle", out chapterTitle);
            keyValues.TryGetValue("message", out message);
        }
    }
}
