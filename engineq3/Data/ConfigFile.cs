using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KSoft.Client.Data
{
    public class ConfigFile
    {
        Dictionary<string, string> keyValues = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                if (keyValues.ContainsKey(key))
                    return keyValues[key];
                else
                    return null;
            }

            set
            {
                if (value == null)
                {
                    if (keyValues.ContainsKey(key))
                        keyValues.Remove(key);
                }
                else
                {
                    keyValues[key] = value;
                }
            }
        }

        public ConfigFile()
        {
            if (!Directory.Exists("Config"))
            {
                Directory.CreateDirectory("Config");
            }
        }

        char[] splitChars = new char[] { '=' };

        void LoadKeyValues(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                // blank line
                if (line.Length == 0)
                    continue;
                // comment
                if (line[0] == '#')
                    continue;

                string[] split = line.Split(splitChars, 2, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 2)
                {
                    keyValues[split[0]] = split[1];
                }
            }
        }

        string[] GetKeyValues()
        {
            string[] lines = new string[keyValues.Keys.Count];

            int line = 0;
            foreach(KeyValuePair<string, string> pair in keyValues) 
            {
                string lineString = pair.Key + "=" + pair.Value;
                lines[line++] = lineString;
            }

            return lines;
        }

        public bool Load(string name)
        {
            string filePath = "Config/" + name + ".cfg";

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                LoadKeyValues(lines);

                return true;
            }
            return false;
        }

        public bool Save(string name)
        {
            string filePath = "Config/" + name + ".cfg";

            string[] lines = GetKeyValues();
            File.WriteAllLines(filePath, lines);

            return true;
        }
    }
}
