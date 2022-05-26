using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KSoft.Client.GameObjects.Characters
{
    // A controllable object with health and other properties
    public class Character : GameObject
    {
        public bool alive = true;
        public int maxHP = 1;
        public int HP = 1;

        public float boundingWidth = 1f;
        public float boundingHeight = 1f;

        public static List<Character> all = new List<Character>();

        public override void Load()
        {
            all.Add(this);
            base.Load();
        }

        public override void Unload()
        {
            all.Remove(this);
            base.Unload();
        }

        public BoundingBox GetBB()
        {
            return new BoundingBox(
                new Vector3(-boundingWidth/2f, 0f, -boundingWidth/2f),
                new Vector3(boundingWidth/2f, boundingHeight, boundingWidth / 2f));
        }

        public BoundingBox GetWorldBB()
        {
            BoundingBox bb = GetBB();
            return new BoundingBox(bb.Min + position, bb.Max + position);
        }

        // Axis-aligned bounding check
        public bool Intersects(Character character)
        {
            // X check
            bool intersectsX = 
                position.X - boundingWidth < character.position.X + character.boundingWidth &&
                position.X + boundingWidth > character.position.X - character.boundingWidth;

            // Z check
            bool intersectsZ =
                position.Z - boundingWidth < character.position.Z + character.boundingWidth &&
                position.Z + boundingWidth > character.position.Z - character.boundingWidth;

            // height check
            //?? may be incorrect
            bool intersectsY =
                position.Y - boundingHeight*2 < character.position.Y + character.boundingHeight*2 &&
                position.Y + boundingHeight*2 > character.position.Y - character.boundingHeight*2;

            return intersectsX && intersectsY && intersectsZ;
        }

        public virtual void Hurt(int damagePoints)
        {
            HP -= damagePoints;
            if (HP <= 0)
            {
                alive = false;
                Killed();
            }
        }

        public virtual void Heal(int damagePoints)
        {
            HP += damagePoints;
            if (HP > maxHP)
            {
                HP = maxHP;
            }
        }

        public virtual void Killed()
        {
            World.RemoveObject(this);
        }
    }
}
