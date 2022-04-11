#region Using Statements
using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace CopperSource
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldVertex : IVertexType
    {
        #region Private Properties

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }

        #endregion

        #region Public Variables

        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector2 LightmapCoordinate;
        //public Vector3 Binormal;
        //public Vector3 Tangent;

        #endregion

        #region Public Static Variables

        public static readonly VertexDeclaration VertexDeclaration;

        #endregion

        #region Private Static Constructor

        static WorldVertex()
        {
            VertexDeclaration = new VertexDeclaration(
                new VertexElement[]
				{
					new VertexElement(
						0,
						VertexElementFormat.Vector3,
						VertexElementUsage.Position,
						0
					),
					new VertexElement(
						12,
						VertexElementFormat.Vector3,
						VertexElementUsage.Normal,
						0
					),
					new VertexElement(
						24,
						VertexElementFormat.Vector2,
						VertexElementUsage.TextureCoordinate,
						0
					),
                    new VertexElement(
						32,
						VertexElementFormat.Vector2,
						VertexElementUsage.TextureCoordinate,
						1
					),
                    //new VertexElement(
                    //    40,
                    //    VertexElementFormat.Vector3,
                    //    VertexElementUsage.Binormal,
                    //    0
                    //),
                    //new VertexElement(
                    //    52,
                    //    VertexElementFormat.Vector3,
                    //    VertexElementUsage.Tangent,
                    //    0
                    //),
				}
            );
        }

        #endregion

        #region Public Constructor

        public WorldVertex(
            Vector3 position,
            Vector3 normal,
            Vector2 textureCoordinate1,
            Vector2 textureCoordinate2
        )
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate1;
            LightmapCoordinate = textureCoordinate2;
            //Binormal = Vector3.Zero;
            //Tangent = Vector3.Zero;
        }

        #endregion

        #region Public Static Operators and Override Methods

        public override int GetHashCode()
        {
            // TODO: Fix GetHashCode
            return 0;
        }

        public override string ToString()
        {
            return (
                "{{Position:" + Position.ToString() +
                " Normal:" + Normal.ToString() +
                " TextureCoordinate1:" + TextureCoordinate.ToString() +
                " TextureCoordinate2:" + LightmapCoordinate.ToString() +
                "}}"
            );
        }

        public static bool operator ==(WorldVertex left, WorldVertex right)
        {
            return ((left.Position == right.Position) &&
                    (left.Normal == right.Normal) &&
                    (left.TextureCoordinate == right.TextureCoordinate) &&
                    (left.LightmapCoordinate == right.LightmapCoordinate));
        }

        public static bool operator !=(WorldVertex left, WorldVertex right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            return (this == ((WorldVertex)obj));
        }

        #endregion
    }
}
