using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CopperSource
{
    [Flags]
    public enum MipTexPropertyFlags : byte
    {
        None = 0,               // No special properties
        Transparent = 1,        // {  : texture is transparent cutout
        Water = 2,              // !  : texture uses water effect
        Light = 4,              // ~  : texture emits light
        Animated = 8,           // +  : texture has animation
        AnimatedToggle = 16,    // +A : texture has toggled animation
        Randomized = 32,        // -  : texture has random tiling
    }

    // allows to extract the properties of a texture's name from a string
    public struct MipTextureProperties
    {
        public MipTexPropertyFlags flags;
        public byte animationIndex;
        public byte randomIndex;
        public string originalName;
        public string filteredName;

        public MipTextureProperties(string name)
        {
            flags = MipTexPropertyFlags.None;
            animationIndex = 0;
            randomIndex = 0;
            originalName = name;
            filteredName = "";

            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];

                // upon first letter found, create a filtered version of the name
                if (Char.IsLetter(c))
                {
                    filteredName = name.Substring(i);
                    break;
                }

                // figure out the properties
                if (c == '{')
                {
                    flags |= MipTexPropertyFlags.Transparent;
                }
                if (c == '!')
                {
                    flags |= MipTexPropertyFlags.Water;
                }
                if (c == '~')
                {
                    flags |= MipTexPropertyFlags.Light;
                }
                if (c == '+')
                {
                    flags |= MipTexPropertyFlags.Animated;
                    char n = name[++i]; // next character
                    if (n == 'A')
                    {
                        flags |= MipTexPropertyFlags.AnimatedToggle;
                    }
                    else
                    {
                        animationIndex = (byte)(n - '0');
                    }
                }
                if (c == '-')
                {
                    flags |= MipTexPropertyFlags.Randomized;
                    char n = name[++i]; // next character
                    randomIndex = (byte)(n - '0');
                }
            }
        }
    }
}
