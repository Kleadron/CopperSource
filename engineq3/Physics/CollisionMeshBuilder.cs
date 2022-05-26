using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using KSoft.Client.Common;

namespace KSoft.Client.Physics
{
    class CollisionMeshBuilder
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<int> indices = new List<int>();

        ModelData data;
        Matrix transform;

        public CollisionMeshBuilder()
        {

        }

        public void Reset()
        {
            vertices.Clear();
            indices.Clear();
        }

        public void SetDataSource(ModelData data)
        {
            this.data = data;
        }

        public void SetAddTransform(Matrix transform)
        {
            this.transform = transform;
        }

        void AddFaceTransformed(ModelData.Face face)
        {
            // get a group or create a new one
            int firstFaceVertex = vertices.Count;
            int firstFaceIndex = indices.Count;

            // create vertices
            for (int j = 0; j < face.totalVertices; j++)
            {
                ModelData.Vertex vertex = data.vertices[data.indices[face.firstVertex + j]];

                Vector3 v = data.positions[vertex.position];
                Vector3.Transform(ref v, ref transform, out v);

                //vArray[vIndex++] = v;
                vertices.Add(v);
            }

            // triangulate indices
            for (int k = 2; k < face.totalVertices; k++)
            {
                indices.Add(firstFaceVertex);
                indices.Add(firstFaceVertex + k - 1);
                indices.Add(firstFaceVertex + k);
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

        public TriangleMesh Build()
        {
            Vector3[] vertexArray = vertices.ToArray();
            int[] indexArray = indices.ToArray();

            Reset();

            TriangleMesh model = new TriangleMesh(vertexArray, indexArray);

            return model;
        }
    }
}
