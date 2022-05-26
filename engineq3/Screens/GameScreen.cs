using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using KSoft.Client.Common;
using KSoft.Client.GFX;
using KSoft.Client.World;

namespace KSoft.Client.Screens
{
    public abstract class GameScreen
    {
        public ScreenManager screens;

        // shortcuts :D
        protected AssetManager Assets { get { return Engine.I.assets; } }
        protected ContentManager Content { get { return Engine.I.Content; } }
        protected GraphicsDevice GraphicsDevice { get { return Engine.I.GraphicsDevice; } }
        protected GameWorld World { get { return Engine.I.world; } }

        public bool IsTop { get { return screens.IsTopmost(this); } }
        //Action OnLoad, OnClose, OnShown, OnHidden;

        public virtual void Load() { }
        public virtual void Unload() { }
        public virtual void Shown() { }
        public virtual void Covered() { }

        public virtual void Update(float delta, float total) { }
        public virtual void Draw(float delta, float total) { }

        //protected void UpdateWorld(float delta, float total)
        //{
        //    Engine.I.world.Update(delta, total);
        //}

        //protected void DrawWorld(Camera camera, float delta, float total)
        //{
        //    Engine.I.world.Draw(camera, delta, total);
        //}
    }
}
