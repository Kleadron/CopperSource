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
    public struct VertexPositionNormalDualTexture : IVertexType
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
        public Vector2 TextureCoordinate1;
        public Vector2 TextureCoordinate2;

        #endregion

        #region Public Static Variables

        public static readonly VertexDeclaration VertexDeclaration;

        #endregion

        #region Private Static Constructor

        static VertexPositionNormalDualTexture()
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
					)
				}
            );
        }

        #endregion

        #region Public Constructor

        public VertexPositionNormalDualTexture(
            Vector3 position,
            Vector3 normal,
            Vector2 textureCoordinate1,
            Vector2 textureCoordinate2
        )
        {
            Position = position;
            Normal = normal;
            TextureCoordinate1 = textureCoordinate1;
            TextureCoordinate2 = textureCoordinate2;
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
                " TextureCoordinate1:" + TextureCoordinate1.ToString() +
                " TextureCoordinate2:" + TextureCoordinate2.ToString() +
                "}}"
            );
        }

        public static bool operator ==(VertexPositionNormalDualTexture left, VertexPositionNormalDualTexture right)
        {
            return ((left.Position == right.Position) &&
                    (left.Normal == right.Normal) &&
                    (left.TextureCoordinate1 == right.TextureCoordinate1) &&
                    (left.TextureCoordinate2 == right.TextureCoordinate2));
        }

        public static bool operator !=(VertexPositionNormalDualTexture left, VertexPositionNormalDualTexture right)
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
            return (this == ((VertexPositionNormalDualTexture)obj));
        }

        #endregion
    }
}
