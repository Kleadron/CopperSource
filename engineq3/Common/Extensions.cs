using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace KSoft.Client.Common
{
    public static class Extensions
    {
        public static void SelectiveUnload(this ContentManager cm, object asset)
        {
            if (cm is ExtendedContentManager)
            {
                ((ExtendedContentManager)cm).SelectiveUnload(asset);
            }
            else
            {
                throw new Exception("Not an ExtendedContentManager");
            }
        }

        public static void SelectiveUnload(this ContentManager cm, string assetName)
        {
            if (cm is ExtendedContentManager)
            {
                ((ExtendedContentManager)cm).SelectiveUnload(assetName);
            }
            else
            {
                throw new Exception("Not an ExtendedContentManager");
            }
        }
    }
}
