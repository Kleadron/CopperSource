using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSoft.Client.GFX;
using KSoft.Client.GameObjects.Characters;
using Microsoft.Xna.Framework;

namespace KSoft.Client.GameObjects
{
    public class ObjectManager
    {
        List<GameObject> objects;
        Queue<GameObject> qRemove;
        Queue<GameObject> qAdd;

        const int MaxProjectiles = 500;
        Projectile[] projectiles;
        int nextProjectileIndex = 0;

        public ObjectManager()
        {
            objects = new List<GameObject>();
            qRemove = new Queue<GameObject>();
            qAdd = new Queue<GameObject>();

            projectiles = new Projectile[MaxProjectiles];
            for (int i = 0; i < MaxProjectiles; i++)
            {
                projectiles[i] = new Projectile();
            }
        }

        public void Remove(GameObject gameObject)
        {
            qRemove.Enqueue(gameObject);
        }

        public void Add(GameObject gameObject)
        {
            qAdd.Enqueue(gameObject);
        }

        Projectile GetFreeProjectile()
        {
            Projectile projectile = null;

            int startingIndex = nextProjectileIndex;

            //nextProjectileIndex %= MaxProjectiles;

            while (projectile == null)
            {
                Projectile candidate = projectiles[nextProjectileIndex];
                nextProjectileIndex++;
                nextProjectileIndex %= MaxProjectiles;

                if (!candidate.active)
                {
                    projectile = candidate;
                    projectile.active = true;
                    break;
                }

                // no free projectiles :(
                if (nextProjectileIndex == startingIndex)
                {
                    throw new Exception("Too many projectiles! Hit limit of " + MaxProjectiles);
                    break;
                }
            }

            return projectile;
        }



        public bool LaunchProjectile(Character owner, Vector3 position, Vector3 direction, float gravity, float speed, int damage, int bounces, Color color)
        {
            Projectile proj = GetFreeProjectile();

            if (proj != null)
            {
                proj.Launch(owner, position, direction, gravity, speed, damage, bounces, color);
                return true;
            }

            return false;
        }

        public void Update(float delta, float total)
        {
            while (qRemove.Count > 0)
            {
                GameObject gameObject = qRemove.Dequeue();
                objects.Remove(gameObject);
                gameObject.Unload();
            }

            while (qAdd.Count > 0)
            {
                GameObject gameObject = qAdd.Dequeue();
                objects.Add(gameObject);
                gameObject.Load();
            }

            foreach (GameObject gameObject in objects)
            {
                gameObject.Update(delta, total);
            }

            for (int i = 0; i < MaxProjectiles; i++)
            {
                projectiles[i].Update(delta, total);
            }
        }

        public void Draw(Camera camera, float delta, float total)
        {
            foreach (GameObject gameObject in objects)
            {
                gameObject.Draw(camera, delta, total);
            }

            for (int i = 0; i < MaxProjectiles; i++)
            {
                projectiles[i].Draw(camera, delta, total);
            }

            foreach (GameObject gameObject in objects)
            {
                gameObject.DrawDebugOverlay(camera, delta, total);
            }
        }
    }
}
