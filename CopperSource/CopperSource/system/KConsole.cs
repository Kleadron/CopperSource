using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CopperSource
{
    public static class KConsole
    {
        static int screenWidth;
        static int screenHeight;

        // light pink seems to give a good readable contrast as nothing in the game will normally be pink
        static Color color = Color.LightPink;

        const int MAX_HISTORY = 10;

        static List<LogEntry> logEntries = new List<LogEntry>();
        //static List<LogEntry> removeList = new List<LogEntry>();

        public static Action<string[]> listeners;

        static StringBuilder inputBuffer = new StringBuilder(64);

        static bool active = false;
        public static bool Active 
        {
            get
            {
                return active;
            }
        }

        public static void SetResolution(int width, int height)
        {
            screenWidth = width;
            screenHeight = height;
        }

        public static void Log(string s)
        {
            LogEntry entry = new LogEntry();
            entry.text = s;
            entry.timeLeft = 10f;
            logEntries.Add(entry);
        }

        public static void Update()
        {
            if (Input.KeyPressed(Keys.OemTilde))
            {
                if (active)
                {
                    Input.UnbindInputBuffer();
                    inputBuffer.Clear();
                }
                else
                {
                    Input.BindInputBuffer(inputBuffer);
                }

                active = !active;
            }

            if (active && Input.KeyPressed(Keys.Enter))
            {
                Log("]" + inputBuffer.ToString());
                string[] split = inputBuffer.ToString().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 0)
                    listeners(split);
                Input.ClearInputBuffer();
            }
        }

        public static void Draw(SpriteBatch sb, SpriteFont font, float delta, float total)
        {
            int linePosition = screenHeight - (font.LineSpacing * 2);

            while (logEntries.Count > MAX_HISTORY)
            {
                logEntries.RemoveAt(0);
            }

            for (int i = logEntries.Count - 1; i >= 0; i--)
            {
                // do not draw non-viewable text
                if (linePosition > -font.LineSpacing)
                {
                    float colorMultiplier = MathHelper.Clamp(logEntries[i].timeLeft, 0, 1);
                    if (active)
                    {
                        colorMultiplier = 1f;
                    }
                    if (colorMultiplier > 0f)
                    {
                        sb.DrawString(font, logEntries[i].text, new Vector2(0, linePosition) + Vector2.One, Color.Black * colorMultiplier);
                        sb.DrawString(font, logEntries[i].text, new Vector2(0, linePosition), color * colorMultiplier);
                    }
                }
                linePosition -= font.LineSpacing;

                //if (!active)
                    logEntries[i].timeLeft -= delta;
                //if (logEntries[i].timeLeft <= 0f)
                //{
                //    removeList.Add(logEntries[i]);
                //}
            }

            //for (int i = 0; i < removeList.Count; i++)
            //{
            //    logEntries.Remove(removeList[i]);
            //}
            //removeList.Clear();

            if (!active)
                return;

            string beginning = "]";
            string cursor = "_";
            string prompt = beginning + inputBuffer.ToString();

            int caretPos = Input.CarotPosition + beginning.Length;
            float offset = font.MeasureString(prompt.Substring(0, caretPos)).X;

            sb.DrawString(font, prompt, new Vector2(0, screenHeight - font.LineSpacing) + Vector2.One, Color.Black);
            sb.DrawString(font, prompt, new Vector2(0, screenHeight - font.LineSpacing), color);

            if (Input.TimeSinceInputBufferModified % 0.5f < 0.25f)
            {
                sb.DrawString(font, cursor, new Vector2(offset, screenHeight - font.LineSpacing) + Vector2.One, Color.Black);
                sb.DrawString(font, cursor, new Vector2(offset, screenHeight - font.LineSpacing), color);
            }
        }

        static KConsole()
        {
            KConsole.listeners += Cmd_Version;
        }

        static void Cmd_Version(string[] args)
        {
            if (args[0] == "version")
            {
                KConsole.Log("Application Version: " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
                //KConsole.Log("Shared Version: " + System.Reflection.Assembly.GetAssembly(typeof(KConsole)).GetName().Version.ToString());
            }
        }

        class LogEntry
        {
            public string text;
            public float timeLeft;
        }
    }

}
