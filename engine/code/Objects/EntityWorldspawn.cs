﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CopperSource.Objects
{
    [EntityClassname("worldspawn")]
    public class EntityWorldspawn : Entity
    {
        public string[] wads;

        public EntityWorldspawn(Game1 game) : base(game)
        {

        }

        public override void SetKeyValue(string key, string value)
        {
            if (key == "wad")
            {
                wads = value.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            }

            base.SetKeyValue(key, value);
        }
    }
}