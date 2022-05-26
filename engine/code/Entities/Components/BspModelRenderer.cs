using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CopperSource.Entities.Components
{
    public class BspModelRendererSystem : BaseSystem<BspModelRenderer> 
    {
        public static void Draw(GameTime gameTime)
        {
            foreach (BspModelRenderer component in components)
            {
                component.Draw(gameTime);
            }
        }
    }

    // A component that uses the transform of the entity to draw a BSP model from the map
    public class BspModelRenderer : Component
    {
        //public BspModel model;
        int modelID;
        //public BoundingBox bb;
        RenderFX renderFX;
        Transform transform;

        public override void Added()
        {
            BspModelRendererSystem.Register(this);
        }

        public override void Removed()
        {
            BspModelRendererSystem.Deregister(this);
        }

        public override void LoadKeyValues(Dictionary<string, string> keyValues)
        {
            
        }

        public override void Initialize()
        {
            renderFX = entity.GetComponent<RenderFX>();
            if (renderFX == null)
            {
                renderFX = new RenderFX();
                entity.AddComponent(renderFX);
            }
        }

        public void Draw(GameTime gameTime)
        {

        }
    }
}
