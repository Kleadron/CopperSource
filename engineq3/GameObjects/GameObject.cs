using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using KSoft.Client.Common;
using KSoft.Client.GFX;
using KSoft.Client.World;
using System.IO;
//using KSoft.Client.Networking;

namespace KSoft.Client.GameObjects
{
    //[Flags]
    //public enum GameObjectFlags
    //{
    //    None = 0,
    //    ShouldUpdate = 1,
    //    ShouldDraw = 2,
    //    HasPhysicsMesh = 4,
    //}

    public abstract class GameObject
    {
        protected AssetManager Assets { get { return Engine.I.assets; } }
        protected ContentManager Content { get { return Engine.I.Content; } }
        protected GraphicsDevice GraphicsDevice { get { return Engine.I.GraphicsDevice; } }
        protected GameWorld World { get { return Engine.I.world; } }

        public string name;
        public object tag;
        //public GameObjectFlags flags;
        public Vector3 position;

        public int networkID;

        public virtual void Load() { }
        public virtual void Unload() 
        {
            //BeginCommand(StateCommand.GameObjectUnload);
        }

        public virtual void Update(float delta, float total) { }
        public virtual void Draw(Camera camera, float delta, float total) { }
        public virtual void DrawDebugOverlay(Camera camera, float delta, float total) { }

        //#region Networking
        //protected NetworkManager Network { get { return Engine.I.network; } }
        //protected bool IsNetGame { get { return false; } }
        //protected bool IsHost { get { return false; } }

        //public virtual void WriteStateSync(BinaryWriter writer)
        //{

        //}

        //public virtual void ReadStateSync(BinaryReader reader)
        //{

        //}

        //public virtual void ReadCommand(StateCommand type, BinaryReader reader) 
        //{
        //    if (type == StateCommand.GameObjectPosition)
        //    {
        //        position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        //    }
        //}

        //protected BinaryWriter GetNetWriter()
        //{
        //    return null;
        //}

        //protected void StartCommand(StateCommand type)
        //{

        //}

        //protected void SendCommand(MessageTarget who)
        //{

        //}
        //#endregion
    }
}
