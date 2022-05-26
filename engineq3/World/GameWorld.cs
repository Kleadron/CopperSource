using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSoft.Client.GameObjects;
using KSoft.Client.GFX;
using Microsoft.Xna.Framework;
using KSoft.Client.Common;
using KSoft.Client.Physics;
using KSoft.Client.GameObjects.Characters;

namespace KSoft.Client.World
{
    public class GameWorld
    {
        Engine engine;
        ObjectManager objects;

        public GameWorld(Engine engine)
        {
            this.engine = engine;
            objects = new ObjectManager();
        }

        public void LoadAssets(AssetManager assets)
        {
            
        }

        public void Update(float delta, float total)
        {
            objects.Update(delta, total);
        }

        public bool Raycast(Ray ray, out RayResult hit)//, out Vector3 hitPosition)
        {
            hit = new RayResult();

            //if (cMesh.CheckRay2(ray, out hit))
            //{
            //    return true;
            //}

            return false;
        }

        public void Draw(Camera camera, float delta, float total)
        {
            

            objects.Draw(camera, delta, total);
        }

        public void AddObject(GameObject gameObject)
        {
            objects.Add(gameObject);
        }

        public void AddObject(GameObject gameObject, float x, float y, float z)
        {
            gameObject.position = new Vector3(x, y, z);
            objects.Add(gameObject);
        }

        public void RemoveObject(GameObject gameObject)
        {
            objects.Remove(gameObject);
        }

        public void LaunchProjectile(Character owner, Vector3 position, Vector3 direction, float gravity, float speed, int damage, int bounces, Color color)
        {
            objects.LaunchProjectile(owner, position, direction, gravity, speed, damage, bounces, color);
        }
    }
}
