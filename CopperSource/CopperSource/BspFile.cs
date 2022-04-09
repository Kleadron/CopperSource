using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
//using BEPUphysics.Collidables;

namespace CopperSource
{
    public class BspFile
    {
        public const uint LUMP_ENTITIES = 0;
        public const uint LUMP_PLANES = 1;
        public const uint LUMP_TEXTURES = 2;
        public const uint LUMP_VERTICES = 3;
        public const uint LUMP_VISIBILITY = 4;
        public const uint LUMP_NODES = 5;
        public const uint LUMP_TEXTURE_INFO = 6;
        public const uint LUMP_FACES = 7;
        public const uint LUMP_LIGHTING = 8;
        public const uint LUMP_CLIP_NODES = 9;
        public const uint LUMP_LEAVES = 10;
        public const uint LUMP_MARKSURFACE = 11;
        public const uint LUMP_EDGES = 12;
        public const uint LUMP_SURFACE_EDGES = 13;
        public const uint LUMP_MODELS = 14;
        public const uint LUMPS_COUNT = 15;

        public const uint VERSION_QUAKE = 29;
        public const uint VERSION_GOLDSRC = 30;
        public const uint MAX_MAP_HULLS = 4;

        public struct Lump
        {
            public uint offset;
            public uint length;
        }

        public struct Header
        {
            public uint version;
            public Lump[] lumps;
        }

        // not needed?
        public struct TextureHeader
        {
            public uint totalMipTextures;  // Number of BSPMIPTEX structures
        }

        const int MAXTEXTURENAME = 16;
        const int MIPLEVELS = 4;
        // MipTexture moved to it's own file file

        public struct Edge
        {
            public ushort v1, v2;
        }

        public struct VisOffset
        {
            // what the heck do I put here?
            public int pvs; // potentially visible sets offset (in bytes) from the beginning of the visibility lump
            public int phs; // potentially hearable(?) sets offset
        }

        //public struct PVS
        //{
        //    byte[] bitVectors;
        //}

        public struct Node
        {
            public uint planeIndex;         // index of the splitting plane (in the plane array)

            public short frontChild, backChild;     // index of the child node, or bitwise inversed index of leaf if negative or 0

            public short minX, minY, minZ;  // minimum x, y and z of the bounding box
            public short maxX, maxY, maxZ;  // maximum x, y and z of the bounding box

            public ushort firstFace;        // index of the first face (in the face array)
            public ushort nFaces;         // number of consecutive edges (in the face array)
            //short area;
            //short padding;
        }

        public struct TextureInfo
        {
            public Vector3 vS;
            public float fSShift;    // Texture shift in s direction
            public Vector3 vT;
            public float fTShift;    // Texture shift in t direction
            public uint iMiptex; // Index into textures array
            public uint nFlags;  // Texture flags, seem to always be 0
        }

        public struct Face
        {
            public ushort plane;       // Plane the face is parallel to
            public ushort planeSide;             // Set if different normals orientation

            public uint firstEdge;          // Index of the first surfedge
            public ushort nEdges;         // Number of consecutive surfedges

            public ushort textureInfo;      // Index of the texture info structure
            public uint styles;             // Specify lighting styles
            public int lightmapOffset;     // Offsets into the raw lightmap data
        }

        //public struct LightmapPixel
        //{
        //    public byte r;
        //    public byte g;
        //    public byte b;
        //}

        public struct Leaf
        {
            public int contents;           // ?

            public int visOffset;          // -1 for cluster indicates no visibility information
            //ushort area;                  // ?

            public short minX, minY, minZ;  // bounding box minimums
            public short maxX, maxY, maxZ;  // bounding box maximums

            public ushort firstMarkSurface, nMarkSurfaces; // Index and count into marksurfaces array

            public uint ambientLevels;      // Ambient sound levels

            //ushort firstLeafBrush;        // ?
            //ushort numLeafBrushes;        // ?
        }

        public struct MapModel
        {
            public Vector3 min, max;
            public Vector3 origin;
            //public int[] headnodes;
            public int
                node1, // the first BSP node
                node2, // the first clip node
                node3, // the second clip node
                node4; // apparently usually empty
            public int visleafs;
            public int firstFace, nFaces;

            //public const uint ByteSize = sizeof(float) * 6 + sizeof(int) * 4 + sizeof(int) + sizeof(int) * 2;
            //public const uint ByteSize = sizeof(MapModel);
        }

        //struct NewPlane 
        //{
        //    public Vector3 normal;
        //    public float distance;
        //    public uint type;
        //}

        public Header header;
        public string[] entityData;
        public TextureHeader texHeader;
        public int[] mipTextureOffsets;
        public MipTexture[] textures;
        public Vector3[] vertices;
        //public VisOffset[] visOffsets;
        public byte[] visData;
        public Node[] nodes;
        public TextureInfo[] textureInfos;
        public Plane[] planes;
        public Face[] faces;
        public byte[] lightmapData;
        public Leaf[] leaves;
        public ushort[] markSurfaces;
        public Edge[] edges;
        public int[] surfaceEdges;
        public MapModel[] models;

        public BspFile(string path)
        {
            Console.WriteLine("Loading BSP " + path);

            Stream fs = TitleContainer.OpenStream(path);
            BinaryReader reader = new BinaryReader(fs);

            Console.WriteLine("Read Header");
            ReadHeader(reader);
            Console.WriteLine("Read Entities");
            ReadEntities(reader);
            Console.WriteLine("Read Planes");
            ReadPlanes(reader);
            Console.WriteLine("Read Textures");
            ReadTextures(reader);
            Console.WriteLine("Read Vertices");
            ReadVertices(reader);
            Console.WriteLine("Read Visibility");
            ReadVisibility(reader);
            Console.WriteLine("Read Nodes");
            ReadNodes(reader);
            Console.WriteLine("Read Texture Info");
            ReadTextureInfo(reader);
            Console.WriteLine("Read Faces");
            ReadFaces(reader);
            Console.WriteLine("Read Lightmaps");
            ReadLightmaps(reader);
            Console.WriteLine("Read Leaves");
            ReadLeaves(reader);
            Console.WriteLine("Read MarkSurfaces");
            ReadMarkSurfaces(reader);
            Console.WriteLine("Read Edges");
            ReadEdges(reader);
            Console.WriteLine("Read SurfaceEdges");
            ReadSurfaceEdges(reader);
            Console.WriteLine("Read Models");
            ReadModels(reader);

            reader.Close();

            Console.WriteLine("Done loading BSP");
            // build map model
            //BuildVisualData();
        }

        //void BuildVisualData()
        //{
        //    if (vb != null)
        //        vb.Dispose();

        //    //Random rand = new Random();

        //    List<VertexPositionNormalTexture> v = new List<VertexPositionNormalTexture>();
        //    List<Vector3> points = new List<Vector3>();

        //    Vector3[] physVert;
        //    int[] physInd;

        //    for (int i = models[0].firstFace; i < models[0].totalFaces; i++)
        //    {
        //        Vector3 normal = planes[faces[i].planeIndex].Normal;

        //        if (faces[i].side != 0)
        //            normal *= -1f;

        //        uint edgeIndex = faces[i].firstSurfEdge;
        //        for (int j = 0; j < faces[i].totalSurfEdges; j++)
        //        {
        //            int surfEdge = surfaceEdges[edgeIndex + j];
        //            Edge edge = edges[Math.Abs(surfEdge)];

        //            int index = surfEdge >= 0 ? edge.v1 : edge.v2;
        //            points.Add(vertices[index]);
        //        }

        //        // convert to triangles
        //        Vector3 p0 = points[0];
        //        for (int k = 2; k < points.Count; k++)
        //        {
        //            //Color color = new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());

        //            v.Add(new VertexPositionNormalTexture(p0, normal, Vector2.Zero));
        //            v.Add(new VertexPositionNormalTexture(points[k - 1], normal, Vector2.Zero));
        //            v.Add(new VertexPositionNormalTexture(points[k], normal, Vector2.Zero));
        //        }

        //        points.Clear();
        //    }

        //    physVert = new Vector3[v.Count];
        //    physInd = new int[v.Count];

        //    for (int i = 0; i < v.Count; i++)
        //    {
        //        physVert[i] = v[i].Position;
        //        physInd[i] = i;
        //    }

        //    StaticMesh mesh = new StaticMesh(physVert, physInd);
        //    game.space.Add(mesh);

        //    vb = new VertexBuffer(game.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, v.Count, BufferUsage.WriteOnly);
        //    vb.SetData(v.ToArray());

        //    v.Clear();
        //    //VertexPositionColor[] v = new VertexPositionColor[edges.Length * 2];

        //    //int index = 0;
        //    //for (int i = 0; i < edges.Length; i++)
        //    //{
        //    //    v[index].Position = vertices[edges[i].v1];
        //    //    v[index].Color = Color.White;
        //    //    index++;
        //    //    v[index].Position = vertices[edges[i].v2];
        //    //    v[index].Color = Color.White;
        //    //    index++;
        //    //}

        //    //vb.SetData(v);
        //}

        //public void Draw(Matrix view, Matrix projection)
        //{
        //    effect.World = Matrix.Identity;
        //    effect.View = view;
        //    effect.Projection = projection;
        //    effect.CurrentTechnique.Passes[0].Apply();

        //    game.GraphicsDevice.SetVertexBuffer(vb);
        //    game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vb.VertexCount / 3);
        //    game.GraphicsDevice.SetVertexBuffer(null);
        //    game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
        //}

        void ReadHeader(BinaryReader reader)
        {
            // Read version and check it
            header.version = reader.ReadUInt32();
            if (header.version != VERSION_GOLDSRC)
            {
                //throw new Exception("Map version mismatch, must be " + VERSION + ", was " + header.version);
                Console.WriteLine("Map version mismatch, should be " + VERSION_GOLDSRC + ", was " + header.version);
            }

            // Read lumps
            header.lumps = new Lump[LUMPS_COUNT];
            for (int i = 0; i < LUMPS_COUNT; i++)
            {
                header.lumps[i].offset = reader.ReadUInt32();
                header.lumps[i].length = reader.ReadUInt32();
            }
        }

        void ReadEntities(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_ENTITIES].offset;
            int size = (int)header.lumps[LUMP_ENTITIES].length;

            string entityLump = new string(reader.ReadChars(size));
            string[] lines = entityLump.Trim().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            entityData = lines;

            //Dictionary<string, string> keyValues = new Dictionary<string, string>();

            //char[] chars = reader.ReadChars((int)header.lumps[LUMP_ENTITIES].length - 1);
            //string[] lines = new string(chars).Trim().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            //bool readingEntities = false;

            //for (int i = 0; i < lines.Length; i++)
            //{
            //    string line = lines[i];

            //    //if (line.Trim() == "")
            //    //continue;

            //    if (line == "{")
            //    {
            //        //Console.WriteLine("Begin entity block");
            //        readingEntities = true;
            //        continue; // skip so it doesn't try and parse this line
            //    }

            //    if (line == "}")
            //    {
            //        //Console.WriteLine("End entity block");

            //        // construct an entity
            //        //ConstructEntityFromDataBlock(keyValues);

            //        keyValues.Clear();
            //        readingEntities = false;
            //        continue;
            //    }

            //    if (readingEntities)
            //    {
            //        int keyStart = line.IndexOf('"', 0);
            //        int keyEnd = line.IndexOf('"', keyStart + 1);

            //        string key = line.Substring(keyStart + 1, keyEnd - keyStart - 1);

            //        int valueStart = line.IndexOf('"', keyEnd + 1);
            //        int valueEnd = line.IndexOf('"', valueStart + 1);

            //        string value = line.Substring(valueStart + 1, valueEnd - valueStart - 1);

            //        //Console.WriteLine("Key: " + key);
            //        //Console.WriteLine("Value: " + value);
            //        keyValues[key] = value;
            //    }
            //}

            //File.WriteAllLines("entities.txt", lines);
        }

        //void ConstructEntityFromDataBlock(Dictionary<string, string> data)
        //{
        //    //if (!data.ContainsKey("classname"))
        //    //return;

        //    string classname = data["classname"];

        //    Type classtype = assembly.GetType("TerrainTest.Entities." + classname, false, true);

        //    if (classtype != null)
        //    {
        //        Entity entity = (Entity)Activator.CreateInstance(classtype);

        //        entity.InitializeFromDataBlock(data);
        //        game.AddEntity(entity);

        //        //Console.WriteLine("Entity " + classname + " constructed successfully");
        //    }
        //    else
        //    {
        //        //Console.WriteLine("Entity " + classname + " was not constructed");
        //    }
        //}

        void ReadPlanes(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_PLANES].offset;
            uint totalPlanes = header.lumps[LUMP_PLANES].length / 20u;

            planes = new Plane[totalPlanes];
            for (int i = 0; i < totalPlanes; i++)
            {
                Vector3 normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                float distance = reader.ReadSingle();

                // might not need this
                uint type = reader.ReadUInt32();

                planes[i] = new Plane(normal, distance);
            }

        }

        void ReadTextures(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_TEXTURES].offset;

            if (header.lumps[LUMP_TEXTURES].length == 0)
                //throw new Exception("LUMP_TEXTURES has length of 0");
                return;

            uint totalTextures = reader.ReadUInt32();
            texHeader.totalMipTextures = totalTextures;

            mipTextureOffsets = new int[totalTextures];
            for (int i = 0; i < totalTextures; i++)
            {
                mipTextureOffsets[i] = reader.ReadInt32();
            }

            textures = new MipTexture[totalTextures];
            for (int i = 0; i < totalTextures; i++)
            {
                uint baseMiptexPosition = header.lumps[LUMP_TEXTURES].offset + (uint)mipTextureOffsets[i];
                reader.BaseStream.Position = baseMiptexPosition;
                //textures[i].name = "";

                byte[] name = reader.ReadBytes(MAXTEXTURENAME);
                string namestring = ASCIIEncoding.ASCII.GetString(name);

                namestring = namestring.Substring(0, namestring.IndexOf((char)0));

                textures[i].name = namestring;

                Console.WriteLine(namestring);

                textures[i].width = reader.ReadInt32();
                textures[i].height = reader.ReadInt32();

                uint mip0 = reader.ReadUInt32();
                uint mip1 = reader.ReadUInt32();
                uint mip2 = reader.ReadUInt32();
                uint mip3 = reader.ReadUInt32();

                if (textures[i].width > 4096 || textures[i].height > 4096)
                {
                    Console.WriteLine("Texture size too big! " + namestring + " " + textures[i].width + "x" + textures[i].height);
                    continue;
                }

                if (mip0 != 0)
                {
                    reader.BaseStream.Position = baseMiptexPosition + mip0;
                    textures[i].mip0data = ReadTextureData(reader, textures[i].width, textures[i].height);
                }
                if (mip1 != 0)
                {
                    reader.BaseStream.Position = baseMiptexPosition + mip1;
                    textures[i].mip1data = ReadTextureData(reader, textures[i].width / 2, textures[i].height / 2);
                }
                if (mip2 != 0)
                {
                    reader.BaseStream.Position = baseMiptexPosition + mip2;
                    textures[i].mip2data = ReadTextureData(reader, textures[i].width / 4, textures[i].height / 4);
                }
                if (mip3 != 0)
                {
                    reader.BaseStream.Position = baseMiptexPosition + mip3;
                    textures[i].mip3data = ReadTextureData(reader, textures[i].width / 8, textures[i].height / 8);
                }

                if (mip0 != 0 || mip1 != 0 || mip2 != 0 || mip3 != 0)
                {
                    //reader.BaseStream.Position = header.lumps[LUMP_TEXTURES].offset + mip3 + ((textures[i].width / 8) * (textures[i].height / 8));
                    reader.BaseStream.Position += 2;
                    textures[i].colorPalette = ReadColorPalette(reader);
                }
            }
        }

        // set stream position before reading!
        byte[] ReadTextureData(BinaryReader reader, int width, int height)
        {
            //byte[] colors = new byte[width * height];

            byte[] colors = reader.ReadBytes(width * height);
            //for (int j = 0; j < colors.Length; j++)
            //{
            //    colors[j] = reader.ReadByte();
            //}

            return colors;
        }

        Color[] ReadColorPalette(BinaryReader reader)
        {
            Color[] palette = new Color[256];

            for (int i = 0; i < palette.Length; i++)
            {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();

                palette[i] = new Color(r, g, b);
            }

            // last index is always transparent?
            //if (palette[255].R == 0 && palette[255].G == 0 && palette[255].B == 255)
            //    palette[255] = Color.Transparent;

            return palette;
        }

        void ReadFaces(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_FACES].offset;
            uint totalFaces = header.lumps[LUMP_FACES].length / 20u;

            //int faceSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Face));

            faces = new Face[totalFaces];
            for (int i = 0; i < totalFaces; i++)
            {
                faces[i].plane = reader.ReadUInt16();
                faces[i].planeSide = reader.ReadUInt16();
                faces[i].firstEdge = reader.ReadUInt32();
                faces[i].nEdges = reader.ReadUInt16();
                faces[i].textureInfo = reader.ReadUInt16();
                faces[i].styles = reader.ReadUInt32();
                faces[i].lightmapOffset = reader.ReadInt32();
            }
        }

        void ReadLightmaps(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_LIGHTING].offset;
            uint length = header.lumps[LUMP_LIGHTING].length;// / 3;

            if (length == 0)
                return;

            //lightmapData = new LightmapPixel[length];
            //for (int i = 0; i < length; i++)
            //{
            //    lightmapData[i].r = reader.ReadByte();
            //    lightmapData[i].g = reader.ReadByte();
            //    lightmapData[i].b = reader.ReadByte();
            //}
            lightmapData = reader.ReadBytes((int)length);
        }

        void ReadLeaves(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_LEAVES].offset;

            int size = Marshal.SizeOf(typeof(Leaf));
            uint totalLeaves = header.lumps[LUMP_NODES].length / (uint)size;

            leaves = new Leaf[totalLeaves];
            for (int i = 0; i < totalLeaves; i++)
            {
                leaves[i].contents = reader.ReadInt32();

                leaves[i].visOffset = reader.ReadInt32();

                leaves[i].minX = reader.ReadInt16();
                leaves[i].minY = reader.ReadInt16();
                leaves[i].minZ = reader.ReadInt16();

                leaves[i].maxX = reader.ReadInt16();
                leaves[i].maxY = reader.ReadInt16();
                leaves[i].maxZ = reader.ReadInt16();

                leaves[i].firstMarkSurface = reader.ReadUInt16();
                leaves[i].nMarkSurfaces = reader.ReadUInt16();

                leaves[i].ambientLevels = reader.ReadUInt32();
            }

        }

        void ReadMarkSurfaces(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_MARKSURFACE].offset;

            uint totalMarkSurfaces = header.lumps[LUMP_MARKSURFACE].length / sizeof(ushort);

            markSurfaces = new ushort[totalMarkSurfaces];
            for (int i = 0; i < totalMarkSurfaces; i++)
            {
                markSurfaces[i] = reader.ReadUInt16();
            }
        }

        void ReadVertices(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_VERTICES].offset;
            uint totalVerts = header.lumps[LUMP_VERTICES].length / 12u;
            vertices = new Vector3[totalVerts];

            for (int i = 0; i < totalVerts; i++)
            {
                vertices[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }
        }

        void ReadVisibility(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_VISIBILITY].offset;
            // this might be irrelevent
            uint lumpSize = header.lumps[LUMP_VISIBILITY].length;// / 8u; 

            if (lumpSize == 0)
                return;

            // I do not believe GoldSrc uses PAS or a cluster count since these both return garbage
            //uint clusters = reader.ReadUInt16();
            //uint pvsOffset = reader.ReadUInt16();
            //uint pasOffset = reader.ReadUInt16();

            //reader.BaseStream.Position = header.lumps[LUMP_VISIBILITY].offset + pvsOffset;
            visData = reader.ReadBytes((int)lumpSize);
        }

        void ReadNodes(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_NODES].offset;

            int size = Marshal.SizeOf(typeof(Node));
            uint totalNodes = header.lumps[LUMP_NODES].length / (uint)size;

            nodes = new Node[totalNodes];
            for (int i = 0; i < totalNodes; i++)
            {
                //nodes[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                nodes[i].planeIndex = reader.ReadUInt32();

                nodes[i].frontChild = reader.ReadInt16();
                nodes[i].backChild = reader.ReadInt16();

                nodes[i].minX = reader.ReadInt16();
                nodes[i].minY = reader.ReadInt16();
                nodes[i].minZ = reader.ReadInt16();

                nodes[i].maxX = reader.ReadInt16();
                nodes[i].maxY = reader.ReadInt16();
                nodes[i].maxZ = reader.ReadInt16();

                nodes[i].firstFace = reader.ReadUInt16();
                nodes[i].nFaces = reader.ReadUInt16();
            }
        }

        void ReadTextureInfo(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_TEXTURE_INFO].offset;

            int size = Marshal.SizeOf(typeof(TextureInfo));
            uint totalInfos = header.lumps[LUMP_TEXTURE_INFO].length / (uint)size;

            textureInfos = new TextureInfo[totalInfos];
            for (int i = 0; i < totalInfos; i++)
            {
                textureInfos[i].vS.X = reader.ReadSingle();
                textureInfos[i].vS.Y = reader.ReadSingle();
                textureInfos[i].vS.Z = reader.ReadSingle();
                textureInfos[i].fSShift = reader.ReadSingle();

                textureInfos[i].vT.X = reader.ReadSingle();
                textureInfos[i].vT.Y = reader.ReadSingle();
                textureInfos[i].vT.Z = reader.ReadSingle();
                textureInfos[i].fTShift = reader.ReadSingle();

                textureInfos[i].iMiptex = reader.ReadUInt32();
                textureInfos[i].nFlags = reader.ReadUInt32();
            }
        }

        void ReadEdges(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_EDGES].offset;
            uint totalEdges = header.lumps[LUMP_EDGES].length / 4u;

            edges = new Edge[totalEdges];
            for (int i = 0; i < totalEdges; i++)
            {
                edges[i].v1 = reader.ReadUInt16();
                edges[i].v2 = reader.ReadUInt16();
            }
        }

        void ReadSurfaceEdges(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_SURFACE_EDGES].offset;
            uint totalSurfaceEdges = header.lumps[LUMP_SURFACE_EDGES].length / 4u;

            surfaceEdges = new int[totalSurfaceEdges];
            for (int i = 0; i < totalSurfaceEdges; i++)
            {
                surfaceEdges[i] = reader.ReadInt32();
            }
        }

        void ReadModels(BinaryReader reader)
        {
            reader.BaseStream.Position = header.lumps[LUMP_MODELS].offset;

            int size = Marshal.SizeOf(typeof(MapModel));
            uint totalModels = header.lumps[LUMP_MODELS].length / (uint)size;

            models = new MapModel[totalModels];
            for (int i = 0; i < totalModels; i++)
            {
                models[i].min = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                models[i].max = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                models[i].origin = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                //models[i].headnodes = new int[MAX_MAP_HULLS];
                //for (int j = 0; j < MAX_MAP_HULLS; j++)
                //{
                //    models[i].headnodes[j] = reader.ReadInt32();
                //}

                models[i].node1 = reader.ReadInt32(); 
                models[i].node2 = reader.ReadInt32();
                models[i].node3 = reader.ReadInt32();
                models[i].node4 = reader.ReadInt32();

                models[i].visleafs = reader.ReadInt32();
                models[i].firstFace = reader.ReadInt32();
                models[i].nFaces = reader.ReadInt32();
            }
        }
    }
}
