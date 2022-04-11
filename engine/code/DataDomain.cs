using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CopperSource
{
    // should encapsulate a path with game data files contained in it.
    // examples are "ksoft", "valve", "cstrike"
    public class DataDomain
    {
        string path;

        public DataDomain(string path)
        {
            this.path = path;
        }

        public string GetPath()
        {
            return path;
        }
    }
}
