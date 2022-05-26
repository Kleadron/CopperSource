//#define TIMER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using System.Diagnostics;

namespace KSoft.Client.Common
{
    // Loads an OBJ file
    public class ModelData
    {
        #region Variables
        public List<Vector3> positions = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
        //public List<Vector3> colors = new List<Vector3>();

        public string path;

        public class Vertex
        {
            // all related indices to this vertex
            public int
                position = 0,
                normal = 0,
                uv = 0;
                //color = 0;
        }

        public class Face
        {
            public string material;

            // all related vertices to this face
            public int firstVertex = -1;
            public int totalVertices = 0;
        }

        // Do not use vertices directly, use index to get the vertex
        public List<Vertex> vertices = new List<Vertex>();
        public List<int> indices = new List<int>();
        public List<Face> faces = new List<Face>();

        public class Object
        {
            public string name;

            // faces covering the entirety of the groups
            // for creating a graphics model this is probably the only thing you'll need
            public int firstFace = -1;
            public int totalFaces = 0;

            public int firstGroup = -1;
            public int totalGroups = 0;
        }

        public class Group
        {
            public string name;

            // all of the faces within this group
            public int firstFace = -1;
            public int totalFaces = 0;
        }

        public List<Object> objects = new List<Object>();
        public List<Group> groups = new List<Group>();

        public Dictionary<string, Object> nameToObject = new Dictionary<string, Object>();
        public Dictionary<string, Group> nameToGroup = new Dictionary<string, Group>();

        string currentObjectName;
        string currentGroupName;
        string currentMaterialName;

        Object currentObject;
        Group currentGroup;

        public class Material
        {
            public string name;
            public Vector3 diffuseColor = Vector3.One;
            public string diffuseMap = null;
            public Vector3 specularColor = Vector3.Zero;
            public float specularPower = 0f;
            public Vector3 emissiveColor = Vector3.Zero;
        }
        public List<Material> materials = new List<Material>();
        public Dictionary<string, Material> nameToMaterial = new Dictionary<string, Material>();

        bool loaded = false;
        bool fast;
        int lineNumber = 0;
        #endregion Variables

        public ModelData(string path, bool fast = true)
        {
#if TIMER
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            this.fast = fast;
            this.path = path;
            LoadObjFile(path);
#if TIMER
            stopwatch.Stop();
            Console.WriteLine(path + ": " + stopwatch.ElapsedMilliseconds + "ms");
#endif
        }

        #region Load
        void LoadObjFile(string path)
        {
            if (loaded)
            {
                throw new Exception("ModelData has already been loaded, do not attempt twice!");
            }

            if (!File.Exists(path))
            {
                throw new Exception("Wavefront OBJ file at \"" + path + "\" could not be found.");
            }

            // set up variables (indices start at 1 in the file, just pad the beginning)
            positions.Add(Vector3.Zero);
            normals.Add(Vector3.Zero);
            uvs.Add(Vector2.Zero);
            //colors.Add(Vector3.One);

            // begin reading
            string[] lines = File.ReadAllLines(path);

            for (int i = 0; i < lines.Length; i++)
            {
                lineNumber = i;
                ParseObjLine(lines[i]);
            }

            loaded = true;
        }

        void LoadMtlFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("MTL file at \"" + path + "\" could not be found.");
            }

            string[] lines = File.ReadAllLines(path);

            for (int i = 0; i < lines.Length; i++)
            {
                ParseMtlLine(lines[i]);
            }
        }

        void ParseMtlLine(string line)
        {
            line = line.Trim();

            if (line.Length == 0)
                return;
            if (line[0] == '#')
                return;

            string[] split = line.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length == 0)
                return;

            string command = split[0];

            switch (command)
            {
                case "newmtl":
                    {
                        Material mat = new Material();
                        mat.name = split[1];
                        materials.Add(mat);
                        nameToMaterial.Add(mat.name, mat);
                        currentMaterialName = mat.name;
                        break;
                    }

                case "Kd":
                    {
                        Vector3 color = new Vector3(
                            float.Parse(split[1]),
                            float.Parse(split[2]),
                            float.Parse(split[3]));

                        nameToMaterial[currentMaterialName].diffuseColor = color;
                        break;
                    }

                case "Ks":
                    {
                        Vector3 color = new Vector3(
                            float.Parse(split[1]),
                            float.Parse(split[2]),
                            float.Parse(split[3]));

                        nameToMaterial[currentMaterialName].specularColor = color;
                        break;
                    }

                case "Ns":
                    {
                        nameToMaterial[currentMaterialName].specularPower = float.Parse(split[1]);
                        break;
                    }

                case "Ke":
                    {
                        Vector3 color = new Vector3(
                            float.Parse(split[1]),
                            float.Parse(split[2]),
                            float.Parse(split[3]));

                        nameToMaterial[currentMaterialName].emissiveColor = color;
                        break;
                    }

                case "map_Kd":
                    {
                        // if paths are supposed to be relative to the MTL I'm gonna have to fix some things
                        string texName = Path.GetDirectoryName(path) + "/" + split[1];

                        nameToMaterial[currentMaterialName].diffuseMap = texName;
                        break;
                    }

                default:
                    {
                        //Console.WriteLine("MTL???: " + command);
                        break;
                    }
            }
        }

        char[] seperators = new char[] {' ', '\n', '\r', '\t'};

        // deduplicate vertex
        void AddVertex(int posI, int uvI, int normalI)
        {
            int vertexIndex = vertices.Count;

            if (!fast)
            {
                // search backwards for existing vertex
                for (int i = vertexIndex - 1; i >= 0; i--)
                {
                    Vertex v = vertices[i];
                    if (posI == v.position &&
                        uvI == v.uv &&
                        normalI == v.normal)
                    {
                        indices.Add(i);
                        return;
                    }
                }
            }

            Vertex v2 = new Vertex();

            v2.position = posI;
            v2.uv = uvI;
            v2.normal = normalI;

            vertices.Add(v2);
            indices.Add(vertexIndex);
        }

        void ParseObjLine(string line)
        {
            line = line.Trim();

            if (line.Length == 0)
                return;
            if (line[0] == '#')
                return;

            string[] split = line.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length == 0)
                return;

            string command = split[0];

            switch (command)
            {
                case "v":
                    {
                        Vector3 position = new Vector3(
                            float.Parse(split[1]),
                            float.Parse(split[2]),
                            float.Parse(split[3]));

                        positions.Add(position);
                        break;
                    }

                case "vt":
                    {
                        // flip Y for DirectX?
                        Vector2 uv = new Vector2(
                            float.Parse(split[1]),
                            -float.Parse(split[2]));

                        uvs.Add(uv);
                        break;
                    }

                case "vn":
                    {
                        Vector3 normal = new Vector3(
                            float.Parse(split[1]),
                            float.Parse(split[2]),
                            float.Parse(split[3]));

                        normals.Add(normal);
                        break;
                    }

                case "o":
                    {
                        Object obj = new Object();
                        obj.name = split[1];
                        obj.firstFace = faces.Count;
                        obj.totalFaces = 0;
                        obj.firstGroup = groups.Count;
                        obj.totalGroups = 0;

                        currentObject = obj;
                        currentObjectName = obj.name;
                        nameToObject.Add(obj.name, obj);

                        break;
                    }

                case "g":
                    {
                        Group group = new Group();
                        if (split.Length > 1)
                            group.name = split[1];
                        else
                            group.name = "UNNAMED_" + lineNumber;
                        group.firstFace = faces.Count;
                        group.totalFaces = 0;

                        currentGroup = group;
                        currentGroupName = group.name;
                        nameToGroup.Add(group.name, group);

                        if (currentObject != null)
                            currentObject.totalGroups++;

                        break;
                    }

                case "mtllib":
                    {
                        string mtlPath = Path.GetDirectoryName(path) + "/" + split[1];
                        LoadMtlFile(mtlPath);
                        break;
                    }

                case "usemtl":
                    {
                        currentMaterialName = split[1];
                        break;
                    }

                case "f":
                    {
                        Face face = new Face();
                        face.material = currentMaterialName;

                        int vertexCount = split.Length - 1;
                        if (vertexCount < 3)
                        {
                            throw new Exception("Faces cannot be declared with less than 3 points!");
                        }

                        face.firstVertex = indices.Count;
                        face.totalVertices = vertexCount;

                        for (int i = 0; i < vertexCount; i++)
                        {
                            int posI = 0;
                            int uvI = 0;
                            int normalI = 0;

                            // position/uv/normal
                            string[] vSplit = split[i+1].Split('/');

                            // parse position (should always exist)
                            posI = int.Parse(vSplit[0]);

                            // parse UV (optional)
                            if (vSplit.Length > 1 && vSplit[1] != "")
                            {
                                uvI = int.Parse(vSplit[1]);
                            }

                            // parse normal (optional)
                            if (vSplit.Length > 2 && vSplit[2] != "")
                            {
                                normalI = int.Parse(vSplit[2]);
                            }

                            //vertices.Add(v);
                            AddVertex(posI, uvI, normalI);
                        }

                        faces.Add(face);

                        if (currentObject != null)
                            currentObject.totalFaces++;
                        if (currentGroup != null)
                            currentGroup.totalFaces++;

                        break;
                    }

                default:
                    {
                        //Console.WriteLine("OBJ???: " + command);
                        break;
                    }
            }
        }
        #endregion Load

        #region Save

        // F or G?
        string floatSaveFormat = "G";

        public void SaveToObj(string path)
        {
            FileStream stream = File.Open(path, FileMode.Create);
            StreamWriter writer = new StreamWriter(stream);

            for (int i = 1; i < positions.Count; i++)
            {
                Vector3 pos = positions[i];

                string s = "v ";
                s += pos.X.ToString(floatSaveFormat) + " ";
                s += pos.Y.ToString(floatSaveFormat) + " ";
                s += pos.Z.ToString(floatSaveFormat);

                writer.WriteLine(s);
            }

            for (int i = 1; i < uvs.Count; i++)
            {
                Vector2 pos = uvs[i];

                string s = "vt ";
                s += pos.X.ToString(floatSaveFormat) + " ";
                s += pos.Y.ToString(floatSaveFormat);

                writer.WriteLine(s);
            }

            for (int i = 1; i < normals.Count; i++)
            {
                Vector3 pos = normals[i];

                string s = "vn ";
                s += pos.X.ToString(floatSaveFormat) + " ";
                s += pos.Y.ToString(floatSaveFormat) + " ";
                s += pos.Z.ToString(floatSaveFormat);

                writer.WriteLine(s);
            }

            for (int i = 0; i < faces.Count; i++)
            {
                Face face = faces[i];

                string s = "f ";
                for (int j = 0; j < face.totalVertices; j++)
                {
                    Vertex v = vertices[indices[face.firstVertex + j]];

                    s += v.position.ToString();
                    s += "/";
                    if (v.uv != 0)
                        s += v.uv.ToString();
                    s += "/";
                    if (v.normal != 0)
                        s += v.normal.ToString();

                    if (j < face.totalVertices - 1)
                        s += " ";
                }

                writer.WriteLine(s);
            }

            writer.Close();
            stream.Close();
        }

        //int binVersion = 0;
        //const int VFLAG_HAS_UV = 1;
        //const int VFLAG_HAS_NORMAL = 2;

        //public void SaveToBin(string path)
        //{
        //    FileStream stream = File.Open(path, FileMode.Create);
        //    BinaryWriter writer = new BinaryWriter(stream);

        //    writer.Write(binVersion);

        //    for (int i = 1; i < positions.Count; i++)
        //    {
        //        Vector3 pos = positions[i];

        //        writer.Write(pos.X);
        //        writer.Write(pos.Y);
        //        writer.Write(pos.Z);
        //    }

        //    for (int i = 1; i < uvs.Count; i++)
        //    {
        //        Vector2 uv = uvs[i];

        //        writer.Write(uv.X);
        //        writer.Write(uv.Y);
        //    }

        //    for (int i = 1; i < normals.Count; i++)
        //    {
        //        Vector3 normal = normals[i];

        //        writer.Write(normal.X);
        //        writer.Write(normal.Y);
        //        writer.Write(normal.Z);
        //    }

        //    for (int i = 0; i < faces.Count; i++)
        //    {
        //        Face face = faces[i];

        //        writer.Write((byte)0);

        //        for (int j = 0; j < face.totalVertices; j++)
        //        {
        //            Vertex v = vertices[indices[face.firstVertex + j]];

        //            int flags = 0;
        //            if (v.uv != 0)
        //                flags |= VFLAG_HAS_UV;
        //            if (v.normal != 0)
        //                flags |= VFLAG_HAS_NORMAL;

        //            //writer.Write((byte)flags);

        //            writer.Write(v.position);
        //            if (v.uv != 0)
        //                writer.Write(v.uv);
        //            if (v.normal != 0)
        //                writer.Write(v.normal);
        //        }
        //    }

        //    writer.Close();
        //    stream.Close();
        //}
        #endregion Save
    }
}
