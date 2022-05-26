using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.IO;
using KSoft.Client.Common;

namespace KSoft.Client.GFX
{
    public class GraphicsMeshBuilder
    {
        // contains a group of polygons to go with a material
        class PolyGroup
        {
            public string material;

            public List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            public List<ushort> indices = new List<ushort>();
        }

        GraphicsDevice device;
        ContentManager content;

        Dictionary<string, BasicEffect> materialToEffect = new Dictionary<string,BasicEffect>();
        List<BasicEffect> effects = new List<BasicEffect>();
        Dictionary<string, PolyGroup> materialToGroup = new Dictionary<string,PolyGroup>();
        List<PolyGroup> polyGroups = new List<PolyGroup>();

        ModelData data;
        Matrix transform;

        public GraphicsMeshBuilder(GraphicsDevice device, ContentManager content)
        {
            this.device = device;
            this.content = content;
        }

        public void Reset()
        {
            materialToEffect.Clear();
            effects.Clear();
            materialToGroup.Clear();
            polyGroups.Clear();

            //vIndex = 0;
            //iIndex = 0;
        }

        public void SetDataSource(ModelData data)
        {
            this.data = data;
        }

        public void SetAddTransform(Matrix transform)
        {
            this.transform = transform;
        }

        PolyGroup GetMaterialGroup(string materialName)
        {
            PolyGroup group;
            if (!materialToGroup.TryGetValue(materialName, out group))
            {
                group = new PolyGroup();
                group.material = materialName;
                polyGroups.Add(group);
                materialToGroup.Add(materialName, group);

                ModelData.Material material = data.nameToMaterial[materialName];
                BasicEffect effect = new BasicEffect(device);

                effect.EnableDefaultLighting();
                effect.PreferPerPixelLighting = true;

                effect.DiffuseColor = material.diffuseColor;

                if (material.diffuseMap != null)
                {
                    string texturePath = material.diffuseMap;

                    if (texturePath.StartsWith(content.RootDirectory))
                    {
                        texturePath = texturePath.Substring(content.RootDirectory.Length + 1);
                        effect.TextureEnabled = true;
                    }

                    //texturePath = Path.GetDirectoryName(texturePath) + "/" + Path.GetFileNameWithoutExtension(texturePath);

                    // modified to use custom asset manager
                    effect.Texture = Engine.I.assets.Load<Texture2D>(texturePath);
                }

                effect.SpecularColor = material.specularColor;
                effect.SpecularPower = material.specularPower;
                effect.EmissiveColor = material.emissiveColor;

                effects.Add(effect);
                materialToEffect.Add(materialName, effect);
            }
            return group;
        }

        void AddFaceTransformed(ModelData.Face face)
        {
            // get a group or create a new one
            PolyGroup group = GetMaterialGroup(face.material);

            int firstFaceVertex = group.vertices.Count;
            int firstFaceIndex = group.indices.Count;

            // create vertices
            for (int j = 0; j < face.totalVertices; j++)
            {
                ModelData.Vertex vertex = data.vertices[data.indices[face.firstVertex + j]];

                VertexPositionNormalTexture v = new VertexPositionNormalTexture(
                    data.positions[vertex.position], data.normals[vertex.normal], data.uvs[vertex.uv]);

                //if (translated)
                {
                    Vector3.Transform(ref v.Position, ref transform, out v.Position);
                    Vector3.TransformNormal(ref v.Normal, ref transform, out v.Normal);
                    //v.Normal.Normalize();
                }

                //vArray[vIndex++] = v;
                group.vertices.Add(v);
            }

            // triangulate indices
            for (int k = 2; k < face.totalVertices; k++)
            {
                group.indices.Add((ushort)firstFaceVertex);
                group.indices.Add((ushort)(firstFaceVertex + k - 1));
                group.indices.Add((ushort)(firstFaceVertex + k));
            }
        }

        public void AddAll()
        {
            foreach (ModelData.Face face in data.faces)
            {
                AddFaceTransformed(face);
            }
        }

        public void AddObject(string objectName)
        {
            if (!data.nameToObject.ContainsKey(objectName))
            {
                throw new Exception("Object \"" + objectName + "\" does not exist in model data");
            }
            ModelData.Object obj = data.nameToObject[objectName];
            for (int i = obj.firstFace; i < obj.firstFace + obj.totalFaces; i++)
            {
                AddFaceTransformed(data.faces[i]);
            }
        }

        // probably slow af
        //BoundingSphere CalculateBoundingSphere()
        //{
        //    BoundingSphere sphere = new BoundingSphere();
        //    bool first = true;

        //    foreach (PolyGroup group in polyGroups)
        //    {
        //        foreach (VertexPositionNormalTexture vpnt in group.vertices)
        //        {
        //            Vector3 v = vpnt.Position;
        //            if (first)
        //            {
        //                sphere.Center = v;
        //                sphere.Radius = 0;
        //            }
        //            else
        //            {
        //                sphere = BoundingSphere.CreateMerged(sphere, new BoundingSphere(v, 0));
        //            }
        //        }
        //    }

        //    return sphere;
        //}

        public GraphicsMesh Build()
        {
            GraphicsMesh model = new GraphicsMesh(device);

            int vertexArraySize = 0;
            int indexArraySize = 0;

            //model.bs = CalculateBoundingSphere();

            foreach (PolyGroup group in polyGroups)
            {
                vertexArraySize += group.vertices.Count;
                indexArraySize += group.indices.Count;
            }

            VertexPositionNormalTexture[] vArray = new VertexPositionNormalTexture[vertexArraySize];
            ushort[] iArray = new ushort[indexArraySize];

            int vIndex = 0;
            int iIndex = 0;

            model.faceGroups = new FaceGroup[polyGroups.Count];
            model.effects = new BasicEffect[polyGroups.Count];
            int groupI = 0;

            foreach (PolyGroup group in polyGroups)
            {
                FaceGroup fg = new FaceGroup();

                fg.firstVertex = vIndex;
                fg.firstIndex = iIndex;

                group.vertices.CopyTo(vArray, vIndex);
                vIndex += group.vertices.Count;
                group.indices.CopyTo(iArray, iIndex);
                iIndex += group.indices.Count;

                fg.totalVertices = group.vertices.Count;
                fg.totalIndices = group.indices.Count;

                fg.totalPrimitives = fg.totalIndices / 3;

                model.faceGroups[groupI] = fg;
                model.effects[groupI] = materialToEffect[group.material];
                groupI++;
            }

            model.renderGroups = groupI;

            //Console.WriteLine("ModelBuilder: Built " + (iIndex / 3) + " triangles");


            if (vIndex > 0 && iIndex > 0)
            {
                model.vb = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, vIndex, BufferUsage.WriteOnly);
                model.ib = new IndexBuffer(device, IndexElementSize.SixteenBits, iIndex, BufferUsage.WriteOnly);

                model.vb.SetData(vArray, 0, vIndex);
                model.ib.SetData(iArray, 0, iIndex);
            }

            Reset();

            return model;
        }
    }
}
