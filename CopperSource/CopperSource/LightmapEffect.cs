#region File Description
//-----------------------------------------------------------------------------
// DualTextureEffect.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace CopperSource
{
    /// <summary>
    /// Built-in effect that supports two-layer multitexturing.
    /// </summary>
    public class LightmapEffect : Effect, IEffectMatrices, IEffectFog
    {
        #region Effect Parameters

        EffectParameter texDiffuseParam;
        EffectParameter texLightmapParam;
        EffectParameter texDetailParam;
        EffectParameter diffuseColorParam;
        EffectParameter fogColorParam;
        EffectParameter fogVectorParam;

        EffectParameter detailScaleParam;

        EffectParameter worldViewProjParam;
        EffectParameter shaderIndexParam;

        #endregion

        #region Fields

        bool fogEnabled;
        bool vertexColorEnabled;
        bool detailTextureEnabled;

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.Identity;
        Matrix projection = Matrix.Identity;

        Matrix worldView;

        Vector3 diffuseColor = Vector3.One;

        float alpha = 1;

        float fogStart = 0;
        float fogEnd = 1;

        Vector2 detailScale = Vector2.One;

        EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;

        #endregion

        #region Public Properties


        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        public Matrix World
        {
            get { return world; }

            set
            {
                world = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        public Matrix View
        {
            get { return view; }

            set
            {
                view = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }

            set
            {
                projection = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj;
            }
        }


        /// <summary>
        /// Gets or sets the material diffuse color (range 0 to 1).
        /// </summary>
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }

            set
            {
                diffuseColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the material alpha.
        /// </summary>
        public float Alpha
        {
            get { return alpha; }

            set
            {
                alpha = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the fog enable flag.
        /// </summary>
        public bool FogEnabled
        {
            get { return fogEnabled; }

            set
            {
                if (fogEnabled != value)
                {
                    fogEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex | EffectDirtyFlags.FogEnable;
                }
            }
        }


        /// <summary>
        /// Gets or sets the fog start distance.
        /// </summary>
        public float FogStart
        {
            get { return fogStart; }

            set
            {
                fogStart = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the fog end distance.
        /// </summary>
        public float FogEnd
        {
            get { return fogEnd; }

            set
            {
                fogEnd = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the fog color.
        /// </summary>
        public Vector3 FogColor
        {
            get { return fogColorParam.GetValueVector3(); }
            set { fogColorParam.SetValue(value); }
        }

        /// <summary>
        /// Gets or sets the fog color.
        /// </summary>
        public Vector2 DetailScale
        {
            get { return detailScaleParam.GetValueVector2(); }
            set { detailScaleParam.SetValue(value); }
        }

        /// <summary>
        /// Gets or sets the current base texture.
        /// </summary>
        public Texture2D DiffuseTexture
        {
            get { return texDiffuseParam.GetValueTexture2D(); }
            set { texDiffuseParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the current lightmap texture.
        /// </summary>
        public Texture2D LightmapTexture
        {
            get { return texLightmapParam.GetValueTexture2D(); }
            set { texLightmapParam.SetValue(value); }
        }

        /// <summary>
        /// Gets or sets the current overlay texture.
        /// </summary>
        public Texture2D DetailTexture
        {
            get { return texDetailParam.GetValueTexture2D(); }
            set { texDetailParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets whether vertex color is enabled.
        /// </summary>
        public bool VertexColorEnabled
        {
            get { return vertexColorEnabled; }

            set
            {
                if (vertexColorEnabled != value)
                {
                    vertexColorEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the detail texture is enabled.
        /// </summary>
        public bool DetailTextureEnabled
        {
            get { return detailTextureEnabled; }

            set
            {
                if (detailTextureEnabled != value)
                {
                    detailTextureEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }

        #endregion

        #region Methods


        /// <summary>
        /// Creates a new DualTextureEffect with default parameter settings.
        /// </summary>
        //public DualTextureEffect(GraphicsDevice device)
        //    : base(device, Resources.DualTextureEffect)
        //{
        //    CacheEffectParameters();
        //}
        public LightmapEffect(Effect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters();
        }

        /// <summary>
        /// Creates a new DualTextureEffect by cloning parameter settings from an existing instance.
        /// </summary>
        public LightmapEffect(LightmapEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters();

            fogEnabled = cloneSource.fogEnabled;
            vertexColorEnabled = cloneSource.vertexColorEnabled;
            detailTextureEnabled = cloneSource.detailTextureEnabled;

            world = cloneSource.world;
            view = cloneSource.view;
            projection = cloneSource.projection;

            diffuseColor = cloneSource.diffuseColor;

            alpha = cloneSource.alpha;

            fogStart = cloneSource.fogStart;
            fogEnd = cloneSource.fogEnd;

            detailScale = cloneSource.detailScale;
        }


        /// <summary>
        /// Creates a clone of the current DualTextureEffect instance.
        /// </summary>
        public override Effect Clone()
        {
            return new LightmapEffect(this);
        }


        /// <summary>
        /// Looks up shortcut references to our effect parameters.
        /// </summary>
        void CacheEffectParameters()
        {
            texDiffuseParam = Parameters["TexDiffuse"];
            texLightmapParam = Parameters["TexLightmap"];
            texDetailParam = Parameters["TexDetail"];
            diffuseColorParam = Parameters["DiffuseColor"];
            fogColorParam = Parameters["FogColor"];
            fogVectorParam = Parameters["FogVector"];

            detailScaleParam = Parameters["DetailScale"];

            worldViewProjParam = Parameters["WorldViewProj"];
            shaderIndexParam = Parameters["ShaderIndex"];
        }


        /// <summary>
        /// Lazily computes derived parameter values immediately before applying the effect.
        /// </summary>
        protected override void OnApply()
        {
            // Recompute the world+view+projection matrix or fog vector?
            dirtyFlags = EffectHelpers.SetWorldViewProjAndFog(dirtyFlags, ref world, ref view, ref projection, ref worldView, fogEnabled, fogStart, fogEnd, worldViewProjParam, fogVectorParam);

            // Recompute the diffuse/alpha material color parameter?
            if ((dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
            {
                diffuseColorParam.SetValue(new Vector4(diffuseColor * alpha, alpha));

                dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
            }

            // Recompute the shader index?
            if ((dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
            {
                int shaderIndex = 0;

                if (!fogEnabled)
                    shaderIndex += 1;

                if (vertexColorEnabled)
                    shaderIndex += 2;

                if (detailTextureEnabled)
                    shaderIndex += 4;

                shaderIndexParam.SetValue(shaderIndex);

                dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;
            }
        }


        #endregion
    }
}