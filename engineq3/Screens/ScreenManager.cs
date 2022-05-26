using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace KSoft.Client.Screens
{
    public class ScreenManager
    {
        List<GameScreen> screens;
        Queue<GameScreen> qAdd;
        Queue<GameScreen> qRemove;

        public ScreenManager()
        {
            screens = new List<GameScreen>();
            qAdd = new Queue<GameScreen>();
            qRemove = new Queue<GameScreen>();
        }

        public bool IsTopmost(GameScreen screen)
        {
            if (screens.Count > 0)
            {
                return screens[screens.Count - 1] == screen;
            }
            return false;
        }

        public GameScreen Topmost
        {
            get
            {
                if (screens.Count > 0)
                {
                    return screens[screens.Count - 1];
                }
                return null;
            }
        }

        public void Reset()
        {
            foreach (GameScreen screen in screens)
            {
                qRemove.Enqueue(screen);
            }

            while (qRemove.Count > 0)
            {
                GameScreen screen = qRemove.Dequeue();
                screens.Remove(screen);
                screen.Unload();

                //if (screens.Count > 0)
                //    Topmost.Shown();
            }
        }

        public void Remove(GameScreen screen)
        {
            qRemove.Enqueue(screen);
        }

        public void Add(GameScreen screen)
        {
            qAdd.Enqueue(screen);
        }

        public void Update(float delta, float total)
        {
            while (qRemove.Count > 0)
            {
                GameScreen screen = qRemove.Dequeue();
                screens.Remove(screen);
                screen.Unload();

                if (screens.Count > 0)
                    Topmost.Shown();
            }

            while (qAdd.Count > 0)
            {
                if (screens.Count > 0)
                    Topmost.Covered();

                GameScreen screen = qAdd.Dequeue();
                screens.Add(screen);
                screen.screens = this;
                screen.Load();
            }

            foreach (GameScreen screen in screens)
            {
                screen.Update(delta, total);
            }
        }

        public void Draw(float delta, float total)
        {
            foreach (GameScreen screen in screens)
            {
                screen.Draw(delta, total);
            }
        }
    }
}
