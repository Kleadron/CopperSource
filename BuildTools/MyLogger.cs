using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace BuildUtils
{
    class MyLogger : ContentBuildLogger
    {
        public override void LogMessage(string message, params object[] messageArgs) { Console.WriteLine(message); }
        public override void LogImportantMessage(string message, params object[] messageArgs) { Console.WriteLine(message); }
        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs) { Console.WriteLine(message); }
    }
}
