using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using KSoft.Client.GameObjects.Characters;
using KSoft.Client.GFX;
using KSoft.Client.World;
using KSoft.Client.Physics;

namespace KSoft.Client.GameObjects
{
    public class Projectile
    {
        //ObjectManager objects;
        GameWorld world;

        public bool active;
        Character owner;
        Vector3 position;
        Vector3 direction;
        float gravity;
        float speed;
        int damage;
        int bounces;
        Color color;

        int bounceCount = 0;

        public void Launch(Character owner, Vector3 position, Vector3 direction, float gravity, float speed, int damage, int bounces, Color color)
        {
            active = true;
            this.owner = owner;
            this.position = position;
            this.direction = direction;
            this.gravity = gravity;
            this.speed = speed;
            this.damage = damage;
            this.bounces = bounces;
            this.color = color;

            bounceCount = 0;

            world = Engine.I.world;
        }

        Character CheckCharacterCollision(float maxDistance)
        {
            // character collision
            Ray ray = new Ray(position, direction);
            foreach (Character character in Character.all)
            {
                if (character == owner && bounceCount == 0)
                {
                    continue;
                }

                BoundingBox charBB = character.GetWorldBB();
                float? dist = ray.Intersects(charBB);

                if (dist != null && dist <= maxDistance)
                {
                    return character;
                }
            }

            return null;
        }

        public void Update(float delta, float total)
        {
            if (!active)
                return;

            // world physics
            float lengthLeft = speed * delta;
            RayResult hit;
            while (world.Raycast(new Ray(position, direction), out hit))
            {
                float charDistCheck = lengthLeft;
                if (hit.distance < charDistCheck)
                    charDistCheck = hit.distance;

                // check if collided with a character
                Character hitChar = CheckCharacterCollision(charDistCheck);
                if (hitChar != null)
                {
                    hitChar.Hurt(damage);
                    active = false;
                    break;
                }

                // hit something
                if (hit.distance <= lengthLeft)
                {
                    // bounce
                    position += direction * (hit.distance - 0.01f);
                    direction = Vector3.Reflect(direction, hit.surfaceNormal);
                    lengthLeft -= hit.distance - 0.01f;
                    bounceCount++;
                    if (bounceCount > bounces)
                    {
                        active = false;
                        break;
                    }
                    // continue to next iteration;
                }
                else
                {
                    position += direction * lengthLeft;
                    break;
                }

                
            }
        }

        public void Draw(Camera camera, float delta, float total)
        {
            if (!active)
                return;

            DebugDraw.DrawLine(camera, position, position + direction, color, color);
        }
    }
}
