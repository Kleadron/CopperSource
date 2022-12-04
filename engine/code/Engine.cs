using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.Collections;
using System.Text;
using CopperSource.Entities;
using System.Diagnostics;

namespace CopperSource
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Engine : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Matrix view;
        Matrix projection;

        public Vector3 playerPosition = new Vector3(0, 0, 0);
        public Vector3 visPosition;

        BoundingBox visBox;
        void CalcVisBox()
        {
            visBox = new BoundingBox(Vector3.One * float.MaxValue, Vector3.One * float.MinValue);
            for (int i = 0; i < leaves.Length; i++)
            {
                if (leafVisList[i] && leaves[i] != null && leaves[i].modelID == 0)
                {
                    BoundingBox leafBox = leaves[i].bb;

                    if (leafBox.Min.X < visBox.Min.X)
                        visBox.Min.X = leafBox.Min.X;
                    if (leafBox.Min.Y < visBox.Min.Y)
                        visBox.Min.Y = leafBox.Min.Y;
                    if (leafBox.Min.Z < visBox.Min.Z)
                        visBox.Min.Z = leafBox.Min.Z;

                    if (leafBox.Max.X > visBox.Max.X)
                        visBox.Max.X = leafBox.Max.X;
                    if (leafBox.Max.Y > visBox.Max.Y)
                        visBox.Max.Y = leafBox.Max.Y;
                    if (leafBox.Max.Z > visBox.Max.Z)
                        visBox.Max.Z = leafBox.Max.Z;
                }
            }
        }

        const string KSOFT_DATA_DIRECTORY = "ksoft";

        Matrix modelTransform;
        // absolutely not correct and needs testing
        public Vector3 TransformedVisPosition
        {
            get
            {
                //return Vector3.Transform(visPosition, modelTransform);
                return visPosition;
            }
        }

        float cameraYawAngle = 0f;
        float cameraPitchAngle = 0f;

        string mapToLoad;// = KSOFT_DATA_DIRECTORY + "/maps/c1a0.bsp";

        //BspFile mapFile;
        //SpriteFont font;
        HLFont hlFont;

        BasicEffect lineEffect;
        BasicEffect worldEffect;
        BasicEffect overdrawEffect;
        //DualTextureEffect lightmapWorldEffect;
        LightmapEffect lightmapWorldEffect;
        AlphaTestEffect alphaTestEffect;

        VertexBuffer vb;
        IndexBuffer ib;

        public List<WorldVertex> vertList = new List<WorldVertex>();
        public List<int> indexList = new List<int>();

        Surface[] surfaces;
        Dictionary<string, List<int>> textureNameToID = new Dictionary<string,List<int>>();
        List<Texture2D> texList = new List<Texture2D>();
        List<TextureProperties> texPropList = new List<TextureProperties>();
        Texture2D[] textures;
        TextureProperties[] textureProperties;
        //Texture2D[] lightmapTextures;
        //List<Texture2D> lightmapList = new List<Texture2D>();
        int nextTexIndex = 1;

        LightmapAtlas lightmapAtlas;
        List<LMTexEntry> lightmapList = new List<LMTexEntry>();

        Queue<Surface>[] textureFaceQueues;
        int[] indices;
        int[] dynamicIndices;
        int dynamicIndex = 0;
        DynamicIndexBuffer dib;
        Queue<RenderGroup> textureGroupQueue;

        bool[] faceQueued;

        struct RenderGroup
        {
            public int texture;     // texture ID to use
            public int start;       // start of texture group's indices
            public int numVerts;    // number of vertices in group
            public int triCount;    // number of triangles in group
        }

        //const int MODELQUEUE_STATIC = 0;
        //const int MODELQUEUE_DYNAMIC = 1;
        //Queue<ModelDrawEntry>[] modelDrawQueue = new Queue<ModelDrawEntry>[] 
        //{
        //    new Queue<ModelDrawEntry>(),
        //    new Queue<ModelDrawEntry>()
        //};

        Queue<ModelDrawEntry> modelQueueStatic = new Queue<ModelDrawEntry>();
        Queue<ModelDrawEntry> modelQueueDynamic = new Queue<ModelDrawEntry>();

        struct ModelDrawEntry
        {
            public BspModel model;
            public Matrix transform;
            public RenderMode renderMode;
            public Color renderColor;
            public bool[] vislist;
        }

        Texture2D grid, pixel, graypixel, stencilMask;

        RasterizerState wireframeRS;
        RasterizerState scissorRS;

        BlendState multiplyBS;

        DepthStencilState overlayDSS;
        DepthStencilState ditherCreateDSS, ditherApplyDSS;

        SamplerState worldSS;

        bool wireframeOn = false;
        bool freezeVisibility = false;
        bool defaultLighting = false;
        bool overdrawView = false;

        Node rootNode;
        Node[] nodes;
        Leaf[] leaves;
        bool[] leafVisList;
        Leaf cameraLeaf;

        public BspModel[] models;

        byte[] visData;
        ushort[] leafFaces;

        BoundingFrustum viewFrustum;
        float fov = 75f;

        Random rand = new Random();

        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;

        float fogStart = 5000;
        float fogEnd = 11000;
        float cameraClipDistance = 15000;

        Color skyColor = Color.SkyBlue;

        const int MAX_ENTITIES = 2048;
        Entity[] entities = new Entity[MAX_ENTITIES];

        Stopwatch updateTimer;
        Stopwatch drawTimer;

        TimeSpan lastUpdateTime;
        TimeSpan lastDrawTime;

        bool preferSuperSampling = true;
        bool useSuperSampling = false;
        RenderTarget2D superSampleRT;
        Point superSampleScale = new Point(2, 2);

        bool doDitherFlip = false;

        string videoDriver = "D3D9";

        // please give me a better way to do this
        void DriverListener(string msg)
        {
#if !XNA
            if (msg.StartsWith("FNA3D Driver: "))
            {
                string[] split = msg.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                videoDriver = split[split.Length - 1];
                FNALoggerEXT.LogInfo -= DriverListener;
            }
#endif
        }

        public Engine()
        {
            //Vector3 v = DataHelper.ValueToVector3("3.0 4.1 333");
            Console.Title = "KSoft Copper Console";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("KSoft Copper Game Engine");
            Console.ResetColor();

            Point res = new Point(1280, 720);

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = res.X;
            graphics.PreferredBackBufferHeight = res.Y;
            graphics.PreferMultiSampling = false;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

            KConsole.SetResolution(res.X, res.Y);

#if !XNA
            FNALoggerEXT.LogInfo += (msg) => KConsole.Log("[FNA/Info] " + msg);
            FNALoggerEXT.LogWarn += (msg) => KConsole.Log("[FNA/Warning] " + msg);
            FNALoggerEXT.LogError += (msg) => KConsole.Log("[FNA/Error] " + msg);
            FNALoggerEXT.LogInfo += DriverListener;
#endif

#if DEBUG
            KConsole.Log("DEBUG BUILD! Performance will be sub-optimal.");
#endif
            if (Debugger.IsAttached)
            {
                KConsole.Log("Debugger is attached!");
            }

            KConsole.listeners += Engine_CommandListener;

            Content.RootDirectory = "Content";
            //Content.Dispose();
            //Content = null;
            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            //TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d);

            bool capFramerate = true;
            IsFixedTimeStep = capFramerate;
            graphics.SynchronizeWithVerticalRetrace = capFramerate;

            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            if (LaunchParameters.ContainsKey("map"))
            {
                mapToLoad = KSOFT_DATA_DIRECTORY + "/maps/" + LaunchParameters["map"] + ".bsp";
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Listing maps from " + KSOFT_DATA_DIRECTORY + "/maps/");

                string searchterm = "*";

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                string path = KSOFT_DATA_DIRECTORY + "/maps/";
                string[] filenames = Directory.GetFiles(path, searchterm, SearchOption.AllDirectories);
                for (int i = 0; i < filenames.Length; i++)
                {
                    Console.WriteLine(filenames[i].Substring(path.Length));
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Enter a map name to load");
                Console.ResetColor();
                bool validMap = false;
                while (!validMap)
                {
                    string map = Console.ReadLine();
                    mapToLoad = KSOFT_DATA_DIRECTORY + "/maps/" + map + ".bsp";
                    bool exists = File.Exists(mapToLoad);
                    if (exists)
                    {
                        validMap = true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Map file \"" + mapToLoad + "\" does not exist");
                        Console.ResetColor();
                    }
                }
                
            }
        }

        bool refreshSSRT = true;
        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            refreshSSRT = true;
        }

        void CreateSuperSampleRT(int width, int height)
        {
            if (superSampleRT != null)
                superSampleRT.Dispose();

            // untested on FNA but should be allowed
#if XNA
            if (width > 4096 || height > 4096)
            {
                useSuperSampling = false;
                return;
            }
#endif

            useSuperSampling = true;
            superSampleRT = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        }

        void Engine_CommandListener(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "lightmap_save")
                {
                    FileStream fs = File.Open("lightmap.png", FileMode.Create);
                    lightmapAtlas.texture.SaveAsPng(fs, lightmapAtlas.atlasSize, lightmapAtlas.atlasSize);
                    fs.Close();
                    KConsole.Log("Saved lightmap atlas as \"lightmap.png\"");
                }

                if (args[0] == "maps")
                {
                    string searchterm = "*";
                    if (args.Length > 1)
                    {
                        searchterm = args[1];
                    }

                    string path = KSOFT_DATA_DIRECTORY + "/maps/";
                    string[] filenames = Directory.GetFiles(path, searchterm, SearchOption.AllDirectories);
                    for (int i = 0; i < filenames.Length; i++)
                    {
                        KConsole.Log(filenames[i].Substring(path.Length));
                    }
                }

                if (args[0] == "map")
                {
                    KConsole.Log("Map command not implemented");
                }

                if (args[0] == "setpos") 
                {
                    if (args.Length < 4)
                    {
                        KConsole.Log("Provide the X Y Z coordinate you want to go to");
                        KConsole.Log("Example: \"setpos 500 500 20\"");
                    }
                    else
                    {
                        Vector3 newPos;
                        if (
                            float.TryParse(args[1], out newPos.X) &&
                            float.TryParse(args[2], out newPos.Y) &&
                            float.TryParse(args[3], out newPos.Z)
                            )
                        {
                            playerPosition = newPos;
                        }
                        else
                        {
                            KConsole.Log("Could not set position");
                        }
                    } 
                }

                // r_ probably means renderer or rasterizer or some crap
                if (args[0] == "r_ss")
                {
                    if (args.Length > 1)
                    {
                        if (args[1][0] == '1')
                        {
                            preferSuperSampling = true;
                            refreshSSRT = true;
                            superSampleScale = new Point(2, 2);
                            KConsole.Log("SuperSampling mode 1, render res 2x2 (full super sampling)");
                        }
                        else if (args[1][0] == '2')
                        {
                            preferSuperSampling = true;
                            refreshSSRT = true;
                            superSampleScale = new Point(1, 2);
                            KConsole.Log("SuperSampling mode 2, render res 1x2 (vertical dither blend)");
                        }
                        else if (args[1][0] == '3')
                        {
                            preferSuperSampling = true;
                            refreshSSRT = true;
                            superSampleScale = new Point(2, 1);
                            KConsole.Log("SuperSampling mode 3, render res 2x1 (horizontal dither blend)");
                        }
                        else if (args[1][0] == '0')
                        {
                            preferSuperSampling = false;
                            if (superSampleRT != null)
                            {
                                superSampleRT.Dispose();
                                superSampleRT = null;
                                refreshSSRT = true;
                            }
                            KConsole.Log("SuperSampling mode 0, render res 1x1 (disabled)");
                        }
                        else
                        {
                            KConsole.Log("Unrecognized mode");
                        }
                    }
                }

                if (args[0] == "r_ditherflip")
                {
                    if (args.Length > 1)
                    {
                        if (args[1][0] == '1')
                        {
                            doDitherFlip = true;
                            KConsole.Log("Dither-flip enabled");
                        }
                        else if (args[1][0] == '0')
                        {
                            doDitherFlip = false;
                            KConsole.Log("Dither-flip disabled");
                        }
                        else
                        {
                            KConsole.Log("Unrecognized mode");
                        }
                    }
                }

                // :)
                if (args[0] == "quit" || args[0] == "quti")
                {
                    Exit();
                }
            }
        }

        T GetEntityByType<T>() where T : Entity
        {
            foreach (Entity entity in entities)
            {
                if (entity != null && entity is T)
                {
                    return (T)entity;
                }
            }

            return null;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            LoadCameraInfo();
            //entities = new Entity[2048];

            drawTimer = new Stopwatch();
            updateTimer = new Stopwatch();

            //RenderTarget2D rt = new RenderTarget2D(GraphicsDevice, 10, 10, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);

            scissorRS = new RasterizerState();
            scissorRS.MultiSampleAntiAlias = false;
            scissorRS.ScissorTestEnable = true;
            scissorRS.FillMode = FillMode.Solid;
            scissorRS.CullMode = CullMode.CullCounterClockwiseFace;
            scissorRS.DepthBias = 0;
            scissorRS.SlopeScaleDepthBias = 0;

            multiplyBS = new BlendState();
            multiplyBS.ColorBlendFunction = BlendFunction.Add;
            multiplyBS.ColorSourceBlend = Blend.DestinationColor;
            multiplyBS.ColorDestinationBlend = Blend.Zero;
            multiplyBS.AlphaSourceBlend = Blend.DestinationAlpha;
            //multiplyBS.ColorDestinationBlend = Blend.Zero;

            overlayDSS = new DepthStencilState();
            overlayDSS.DepthBufferFunction = CompareFunction.LessEqual;
            overlayDSS.DepthBufferWriteEnable = false;
            overlayDSS.DepthBufferEnable = true;

            ditherCreateDSS = new DepthStencilState();
            ditherCreateDSS.StencilEnable = true;
            ditherCreateDSS.StencilFunction = CompareFunction.Always;
            ditherCreateDSS.StencilPass = StencilOperation.Replace;
            ditherCreateDSS.ReferenceStencil = 1;
            ditherCreateDSS.DepthBufferEnable = false;

            ditherApplyDSS = new DepthStencilState();
            ditherApplyDSS.StencilEnable = true;
            ditherApplyDSS.StencilFunction = CompareFunction.NotEqual;
            ditherApplyDSS.StencilPass = StencilOperation.Keep;
            ditherApplyDSS.ReferenceStencil = 1;
            ditherApplyDSS.DepthBufferEnable = true;

            worldSS = new SamplerState();
            worldSS.AddressU = TextureAddressMode.Wrap;
            worldSS.AddressV = TextureAddressMode.Wrap;
            worldSS.Filter = TextureFilter.Point;
            worldSS.MaxMipLevel = 0;
            worldSS.MaxAnisotropy = 16;
            //worldSS.
            //worldSS.

            string windowTitle = "KSoft Copper - " + mapToLoad;

            windowTitle += " - " + videoDriver;
            if (System.Diagnostics.Debugger.IsAttached)
            {
                windowTitle += " - DEBUGGING";
            }

            Window.Title = windowTitle;

            //CreateSuperSampleRT(graphics.PreferredBackBufferWidth * 2, graphics.PreferredBackBufferHeight * 2);

            base.Initialize();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            SaveCameraInfo();
            base.OnExiting(sender, args);
        }

        void SaveCameraInfo()
        {
            Stream s = File.Open("cam.bin", FileMode.Create);
            BinaryWriter w = new BinaryWriter(s);

            w.Write(playerPosition.X);
            w.Write(playerPosition.Y);
            w.Write(playerPosition.Z);

            w.Write(cameraYawAngle);
            w.Write(cameraPitchAngle);

            w.Close();
            s.Close();
        }

        void LoadCameraInfo()
        {
            if (!File.Exists("cam.bin"))
                return;

            Stream s = File.OpenRead("cam.bin");
            BinaryReader r = new BinaryReader(s);

            playerPosition.X = r.ReadSingle();
            playerPosition.Y = r.ReadSingle();
            playerPosition.Z = r.ReadSingle();

            cameraYawAngle = r.ReadSingle();
            cameraPitchAngle = r.ReadSingle();

            r.Close();
            s.Close();
        }

        int CreateLightmapTexture(int offset, int width, int height)
        {
            //Console.WriteLine(width + "x" + height);
            
            int texIndex = lightmapList.Count;

            LMTexEntry lmtex = new LMTexEntry();
            lmtex.id = texIndex;
            lmtex.offset = offset;
            lmtex.width = width;
            lmtex.height = height;
            lightmapList.Add(lmtex);

            //Texture2D tex = new Texture2D(GraphicsDevice, width, height, false, SurfaceFormat.Color);
            //lightmapList.Add(tex);

            //Color[] colors = new Color[width * height];
            //byte[] lmData = lightmapData;

            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {
            //        int dataIndex = (x + (y * width)) * 3;
            //        int colorIndex = (x + (y * width));
            //        Color c = Color.White;
            //        c.R = lmData[offset + dataIndex];
            //        c.G = lmData[offset + dataIndex + 1];
            //        c.B = lmData[offset + dataIndex + 2];

            //        colors[colorIndex] = c;
            //    }
            //}

            //tex.SetData(colors);

            return texIndex;
        }

        Node BuildNode(BspFile mapData, int index, Node parent, int modelID)
        {
            Node node = new Node();
            node.id = index;
            node.modelID = modelID;
            node.parentNode = parent;
            nodes[index] = node;

            BspFile.Node mapNode = mapData.nodes[index];

            node.bb = new BoundingBox(new Vector3(mapNode.minX, mapNode.minY, mapNode.minZ), 
                new Vector3(mapNode.maxX, mapNode.maxY, mapNode.maxZ));

            node.plane = mapData.planes[mapNode.planeIndex];

            node.firstFace = mapNode.firstFace;
            node.nFaces = mapNode.nFaces;

            short frontChild = mapNode.frontChild;
            // <= 0 is a leaf node
            if (frontChild <= 0)
            {
                frontChild = (short)~frontChild;
                node.frontLeaf = BuildLeaf(mapData, frontChild, node, modelID);
            }
            else
            {
                node.frontNode = BuildNode(mapData, frontChild, node, modelID);
            }

            short backChild = mapNode.backChild;
            // <= 0 is a leaf node
            if (backChild <= 0)
            {
                backChild = (short)~backChild;
                node.backLeaf = BuildLeaf(mapData, backChild, node, modelID);
            }
            else
            {
                node.backNode = BuildNode(mapData, backChild, node, modelID);
            }

            return node;
        }

        Leaf BuildLeaf(BspFile mapData, int index, Node parent, int modelID)
        {
            Leaf leaf = new Leaf();
            leaf.id = index;
            leaf.modelID = modelID;
            leaf.parentNode = parent;
            leaves[index] = leaf;
            BspFile.Leaf mapLeaf = mapData.leaves[index];

            leaf.bb = new BoundingBox(new Vector3(mapLeaf.minX, mapLeaf.minY, mapLeaf.minZ),
                new Vector3(mapLeaf.maxX, mapLeaf.maxY, mapLeaf.maxZ));

            leaf.firstLeafFace = mapLeaf.firstLeafSurface;
            leaf.leafFaceCount = mapLeaf.leafSurfaceCount;
            leaf.visCluster = mapLeaf.visCluster;

            return leaf;
        }

        //public bool BoundingBoxIsVisible(Vector3 pos)
        //{
        //    Leaf leaf = GetLeafFromPosition(pos);
        //    return leafVisList[leaf.id];
        //}

        public bool PointIsVisible(Vector3 pos)
        {
            Leaf leaf = GetLeafFromPosition(pos);
            return leafVisList[leaf.id];
        }

        public bool BBIsVisible(ref BoundingBox bb)
        {
            // first check, inaccurate but is a rough idea what should be visible
            ContainmentType ct;
            visBox.Contains(ref bb, out ct);
            if (ct == ContainmentType.Disjoint)
            {
                return false;
            }

            // second check, frustum cull
            viewFrustum.Contains(ref bb, out ct);
            if (ct == ContainmentType.Disjoint)
            {
                return false;
            }

            // more accurate but slow as hell
            //int visibleIntersections = 0;
            //for (int i = 0; i < leaves.Length; i++)
            //{
            //    Leaf leaf = leaves[i];
            //    if (leafVisList[i] && leaf != null && leaf.modelID == 0)
            //    {
            //        leaf.bb.Contains(ref bb, out ct);
            //        if (ct == ContainmentType.Contains || ct == ContainmentType.Intersects)
            //        {
            //            visibleIntersections++;
            //        }
            //    }
            //}

            //if (visibleIntersections == 0)
            //    return false;

            // v3
            //return RecursiveBoxVisCheck(rootNode, ref bb);

            return true;
        }

        // checks a bounding box agains the BSP tree to see if the bounding box is visible.
        // this method will check both sides of a node if the bounding box intersects it.
        // It should return true if one of the leaves encountered are visible but the leaves are never visible??
        bool RecursiveBoxVisCheck(Node node, ref BoundingBox bb)
        {
            PlaneIntersectionType it;
            node.plane.Intersects(ref bb, out it);

            // front side check
            if (it == PlaneIntersectionType.Front || it == PlaneIntersectionType.Intersecting)
            {
                if (node.frontNode != null)
                {
                    // return true if the recursive check succeeded, otherwise continue
                    bool recursiveCheck = RecursiveBoxVisCheck(node.frontNode, ref bb);
                    if (recursiveCheck)
                        return true;
                }
                else
                {
                    bool leafIsVisible = leafVisList[node.frontLeaf.id];
                    if (leafIsVisible)
                    {
                        return true;
                    }
                }
            }
            // back side check
            if (it == PlaneIntersectionType.Back || it == PlaneIntersectionType.Intersecting)
            {
                if (node.backNode != null)
                {
                    // return true if the recursive check succeeded, otherwise continue
                    bool recursiveCheck = RecursiveBoxVisCheck(node.backNode, ref bb);
                    if (recursiveCheck)
                        return true;
                }
                else
                {
                    bool leafIsVisible = leafVisList[node.backLeaf.id];
                    if (leafIsVisible)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Leaf GetLeafFromPosition(Vector3 pos)
        {
            Node searchNode = rootNode;
            while (true)
            {
                float dot;
                Vector3.Dot(ref searchNode.plane.Normal, ref pos, out dot);
                //searchNode.plane.DotCoordinate(ref pos, out dot);
                // is the position in front or behind of the plane
                if (dot > searchNode.plane.D)
                {
                    // in front of parition plane
                    if (searchNode.frontLeaf != null)
                    {
                        return searchNode.frontLeaf;
                    }
                    else
                    {
                        searchNode = searchNode.frontNode;
                    }
                }
                else
                {
                    // behind partition plane
                    if (searchNode.backLeaf != null)
                    {
                        return searchNode.backLeaf;
                    }
                    else
                    {
                        searchNode = searchNode.backNode;
                    }
                }
            }
        }

        void UpdateVisiblityFromLeaf(Leaf leaf)
        {
            int v = leaf.visCluster;

            if (v == 0 || v == -1)
            {
                for (int i = 0; i < leafVisList.Length; i++)
                {
                    leafVisList[i] = true;
                }
                return;
            }

            //memset(cluster_visible, 0, num_clusters);
            for (int i = 0; i < leafVisList.Length; i++)
            {
                leafVisList[i] = false;
            }

            int visDataLength = visData.Length-1;
            for (int c = 1; c < leaves.Length; v++)
            {
                // idk why this happens but going to implement a check now
                if (v > visDataLength)
                    break;

                if (visData[v] == 0)
                {
                    v++;
                    c += 8 * visData[v];
                }
                else
                {
                    for (byte bit = 1; bit != 0; bit *= 2, c++)
                    {
                        if ((visData[v] & bit) > 0)
                        {
                            if (c < leafVisList.Length)
                                leafVisList[c] = true;
                        }
                    }
                }

            }
        }

        MipTexture missingTex;
        // Mip mapped textures in XNA generate more than 4 levels which is kinda an issue
        bool enableMipMaps = false;

        Texture2D LoadMipTex(MipTexture miptex)
        {
            Texture2D tex = new Texture2D(GraphicsDevice, miptex.width, miptex.height, miptex.mipData[1] != null && enableMipMaps, SurfaceFormat.Color);

            tex.Tag = miptex.name.ToUpper();

            MipTextureProperties texProperties = new MipTextureProperties(miptex.name);

            // id 255 is always transparent on textures
            if (texProperties.flags.HasFlag(MipTexPropertyFlags.Transparent))
                miptex.colorPalette[255].A = 0;

            Color[] colors = new Color[miptex.width * miptex.height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = miptex.colorPalette[miptex.mipData[0][i]];
            }
            tex.SetData(0, null, colors, 0, colors.Length);

            //int mipcount = tex.LevelCount;

            //if (enableMipMaps)
            //{
            //    if (miptex.mip1data != null)
            //    {
            //        int mip1length = (miptex.width / 2) * (miptex.height / 2);
            //        for (int i = 0; i < mip1length; i++)
            //        {
            //            colors[i] = miptex.colorPalette[miptex.mip1data[i]];
            //        }
            //        tex.SetData(1, null, colors, 0, mip1length);
            //    }

            //    if (miptex.mip2data != null)
            //    {
            //        int mip2length = (miptex.width / 4) * (miptex.height / 4);
            //        for (int i = 0; i < mip2length; i++)
            //        {
            //            colors[i] = miptex.colorPalette[miptex.mip2data[i]];
            //        }
            //        tex.SetData(2, null, colors, 0, mip2length);
            //    }

            //    if (miptex.mip3data != null)
            //    {
            //        int mip3length = (miptex.width / 8) * (miptex.height / 8);
            //        for (int i = 0; i < mip3length; i++)
            //        {
            //            colors[i] = miptex.colorPalette[miptex.mip3data[i]];
            //        }
            //        tex.SetData(3, null, colors, 0, mip3length);
            //    }
            //}

            //tex.Name = miptex.name;

            string texfoldername = "allmiptextures";

            if (false)
            {
                if (!Directory.Exists(texfoldername))
                {
                    Directory.CreateDirectory(texfoldername);
                }

                FileStream fs = File.Open(texfoldername + "/" + miptex.name + ".png", FileMode.Create);
                tex.SaveAsPng(fs, miptex.width, miptex.height);
                fs.Close();
            }

            return tex;
        }

        Texture2D LoadImage(string path)
        {
            Texture2D tex = Texture2D.FromStream(GraphicsDevice, File.OpenRead(KSOFT_DATA_DIRECTORY + "/images/" + path + ".png"));
            return tex;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //font = Content.Load<SpriteFont>("Fonts/Font");

            pixel = LoadImage("pixel");//Content.Load<Texture2D>("Textures/pixel");
            graypixel = LoadImage("graypixel");
            stencilMask = LoadImage("stencilmask_50");

            wireframeRS = new RasterizerState();
            wireframeRS.CullMode = CullMode.None;
            wireframeRS.FillMode = FillMode.WireFrame;
            //wireframeRS.

            lineEffect = new BasicEffect(GraphicsDevice);
            worldEffect = new BasicEffect(GraphicsDevice);
            overdrawEffect = new BasicEffect(GraphicsDevice);

            overdrawEffect.DiffuseColor = Color.LightGreen.ToVector3() * 0.15f;

            if (true)
            {
                worldEffect.EnableDefaultLighting();

                worldEffect.DirectionalLight0.Hammerize();
                worldEffect.DirectionalLight1.Hammerize();
                worldEffect.DirectionalLight2.Hammerize();

                worldEffect.AmbientLightColor = Vector3.One * 0.25f;

                worldEffect.PreferPerPixelLighting = true;
                worldEffect.SpecularColor = Vector3.Zero;
            }

            worldEffect.TextureEnabled = true;
            grid = worldEffect.Texture = LoadImage("tiledark_s");

            WadFile fontWad = new WadFile(KSOFT_DATA_DIRECTORY + "/fonts.wad"); // gfx.wad / fonts.wad
            WadFile.Font fontData;
            fontWad.TryReadFont("FONT2", out fontData); // CONCHARS / FONT2
            hlFont = new HLFont(GraphicsDevice, fontData);
            fontWad.Close();

            BspFile mapFile = new BspFile(mapToLoad);
            //File.WriteAllText("entities.txt", mapFile.entityData);

            int entityLoadIndex = 0;
            //int entitieswithcollider = 0;
            bool readingEntity = false;

            Dictionary<string, string> keyValues = new Dictionary<string, string>();

            for (int i = 0; i < mapFile.entityData.Length; i++)
            {
                string line = mapFile.entityData[i];
                if (line[0] == '{')
                {
                    readingEntity = true;
                    keyValues.Clear();
                    continue;
                }

                if (line[0] == '}')
                {
                    readingEntity = false;
                    // make entity
                    Entity ent = null;

                    if (keyValues["classname"].StartsWith("trigger_"))
                    {
                        continue;
                    }

                    if (keyValues["classname"] == "func_ladder")
                    {
                        continue;
                    }

                    //if (keyValues["classname"] == "func_build_zone")
                    //{
                    //    continue;
                    //}

                    if (keyValues.ContainsKey("model") && keyValues["model"].StartsWith("*"))
                    {
                        ent = new BrushEntity(this);
                        
                    }
                    else if (keyValues["classname"] == "worldspawn")
                    {
                        ent = new EntityWorldspawn(this);
                    }
                    else
                    {
                        ent = new Entity(this);
                    }

                    foreach (KeyValuePair<string, string> pair in keyValues)
                    {
                        ent.SetKeyValue(pair.Key, pair.Value);
                    }
                    entities[entityLoadIndex++] = ent;
                    continue;
                }

                if (readingEntity)
                {
                    int keyStart = line.IndexOf('"', 0);
                    int keyEnd = line.IndexOf('"', keyStart + 1);

                    string key = line.Substring(keyStart + 1, keyEnd - keyStart - 1);

                    int valueStart = line.IndexOf('"', keyEnd + 1);
                    int valueEnd = line.IndexOf('"', valueStart + 1);

                    string value = line.Substring(valueStart + 1, valueEnd - valueStart - 1);

                    //Console.WriteLine("Key: " + key);
                    //Console.WriteLine("Value: " + value);
                    keyValues[key] = value;
                    continue;
                }
            }

            //Example of getting a component of one type from all entities.

            /*foreach (Entity entity in entities)
            {
                if (entity == null)
                    continue;

                if (entity.GetComponent<Collider>() != null)
                {
                    entitieswithcollider++;
                }
            }*/

            Console.WriteLine(entityLoadIndex + " entities");
           // Console.WriteLine(entitieswithcollider + " entities with a Collider Component");

            missingTex = new MipTexture();
            missingTex.name = "missing";
            missingTex.width = grid.Width;
            missingTex.height = grid.Height;

            //lightmapWorldEffect = new LightmapEffect(Content.Load<Effect>("Effects/LightmapEffect"));

#if XNA
            string effectExtension = ".xfxb";
#else
            string effectExtension = ".fxb";
#endif

            lightmapWorldEffect = new LightmapEffect(GraphicsDevice, File.ReadAllBytes(KSOFT_DATA_DIRECTORY + "/effects/LightmapEffect" + effectExtension));

            lightmapWorldEffect.Gamma = 2.2f;
            //lightmapWorldEffect.DetailTextureEnabled = true;
            //lightmapWorldEffect.DetailScale = new Vector2(3, 3);
            //detailTex = lightmapWorldEffect.DetailTexture = Content.Load<Texture2D>("Textures/ai_detail");

            //lightmapWorldEffect.LightmapEnabled = false;

            lightmapWorldEffect.FogEnabled = true;
            lightmapWorldEffect.FogStart = fogStart;
            lightmapWorldEffect.FogEnd = fogEnd;
            lightmapWorldEffect.FogColor = skyColor.ToVector3();

            alphaTestEffect = new AlphaTestEffect(GraphicsDevice);
            //alphaTestEffect.ReferenceAlpha = 1;
            alphaTestEffect.AlphaFunction = CompareFunction.Equal;

            //textureNameToID = new Dictionary<string, int>();
            
            surfaces = new Surface[mapFile.surfaces.Length];
            faceQueued = new bool[surfaces.Length];

            texList.Add(grid);
            textureNameToID["missing"] = new List<int>();
            textureNameToID["missing"].Add(0);

            texPropList.Add(new TextureProperties());

            EntityWorldspawn worldspawn = GetEntityByType<EntityWorldspawn>();

            int wadCount = 0;
            if (worldspawn.wads != null)
                wadCount = worldspawn.wads.Length;
            WadFile[] wads = new WadFile[wadCount];

            for (int i = 0; i < wads.Length; i++)
            {
                string wadPath = KSOFT_DATA_DIRECTORY + "/" + Path.GetFileName(worldspawn.wads[i]);

                if (File.Exists(wadPath))
                {
                    wads[i] = new WadFile(wadPath);
                }
                else
                {
                    Console.WriteLine("Wad file does not exist! " + wadPath);
                    KConsole.Log("Could not load WAD \"" + wadPath + "\"");
                }
            }

            if (mapFile.textures != null)
            for (int i = 0; i < mapFile.textures.Length; i++)
            {
                MipTexture miptex = mapFile.textures[i];
                MipTextureProperties miptexProperties = new MipTextureProperties(miptex.name);
                TextureProperties texProperties = new TextureProperties();
                texProperties.light = miptexProperties.flags.HasFlag(MipTexPropertyFlags.Light);
                string fileName = miptex.name.ToUpper();
                string realName = miptex.name;
                bool isRandomized = false;
                int texNum = 0;

                if (miptex.name.Length > 0 && miptex.name[0] == '-')
                {
                    realName = miptex.name.Substring(2);
                    texNum = int.Parse(miptex.name[1].ToString());
                    isRandomized = true;
                }

                //Console.WriteLine(miptex.name);
                bool embeddedTexture = miptex.mipData[0] != null;
                bool hasTextureFile = false;//File.Exists(Content.RootDirectory + "/Textures/Maptextures/" + fileName + ".xnb");

                int wadIndex = 0;
                bool existsInWad = false;

                for (int j = 0; j < wads.Length; j++)
                {
                    if (wads[j] != null && wads[j].HasFile(fileName))
                    {
                        existsInWad = true;
                        wadIndex = j;
                    }
                }

                if (embeddedTexture || hasTextureFile || existsInWad)
                {
                    Texture2D tex = null;
                    if (embeddedTexture)
                        tex = LoadMipTex(miptex);
                    else if (existsInWad)
                    {
                        MipTexture wadMipTex;
                        wads[wadIndex].TryReadTexture(fileName, out wadMipTex);
                        tex = LoadMipTex(wadMipTex);
                    }
                    //else
                    //    tex = Content.Load<Texture2D>("Textures/Maptextures/" + fileName);

                    if (isRandomized)
                    {
                        //Console.WriteLine(realName + " " + texNum);

                        if (!textureNameToID.ContainsKey(realName))
                        {
                            textureNameToID[realName] = new List<int>();
                        }
                        textureNameToID[realName].Add(nextTexIndex++);
                        texList.Add(tex);
                        texPropList.Add(texProperties);
                    }
                    else
                    {
                        textureNameToID[realName] = new List<int>();
                        textureNameToID[realName].Add(nextTexIndex++);
                        texList.Add(tex);
                        texPropList.Add(texProperties);
                    }
                }
                else
                {
                    textureNameToID[realName] = new List<int>();
                    textureNameToID[realName].Add(0);

                    Console.WriteLine("Cannot load texture \"" + fileName + "\"");
                }
            }

            for (int j = 0; j < wads.Length; j++)
            {
                if (wads[j] != null)
                    wads[j].Close();
            }

            textures = texList.ToArray();
            texList.Clear();
            texList = null;

            textureProperties = texPropList.ToArray();
            texPropList.Clear();
            texPropList = null;

            nodes = new Node[mapFile.nodes.Length];
            leaves = new Leaf[mapFile.leaves.Length];
            leafVisList = new bool[mapFile.leaves.Length];

            Console.WriteLine("Creating BSP Models");

            models = new BspModel[mapFile.models.Length];
            for(int mI = 0; mI < mapFile.models.Length; mI++)
            {
                //Console.WriteLine("Generate model " + mI);
                BspFile.MapModel model = mapFile.models[mI];
                BspModel mdl = new BspModel();

                mdl.id = mI;

                mdl.firstFace = model.firstFace;
                mdl.numFaces = model.faceCount;
                mdl.numLeaves = model.visleafs;
                mdl.bb = new BoundingBox(model.min, model.max);
                //mdl.rotationalOrigin = model.origin;
                mdl.center = model.min + ((mdl.bb.Max - mdl.bb.Min) / 2);

                for (int i = model.firstFace; i < model.firstFace + model.faceCount; i++)
                {
                    BuildFace(mapFile, i);
                    // THIS IS THE END OF A FACE GENERATION LOOP. DO NOT PUT STUFF HERE.
                }

                mdl.rootNode = BuildNode(mapFile, model.node1, null, mI);
                //Console.WriteLine("Generated BSP for model " + mI);
                models[mI] = mdl;
            }

            Console.WriteLine("Models generated: " + mapFile.models.Length);

            rootNode = models[0].rootNode;

            //lightmapTextures = lightmapList.ToArray();
            lightmapAtlas = new LightmapAtlas(GraphicsDevice, lightmapList, mapFile.lightmapData);
            lightmapList.Clear();
            lightmapList = null;

            // adjust lightmap uvs for lightmap atlas
            for (int i = 0; i < surfaces.Length; i++)
            {
                Surface face = surfaces[i];
                CalcFaceLightmapUVs(face);
            }

            Console.WriteLine(textures.Length + " textures loaded");

            textureFaceQueues = new Queue<Surface>[textures.Length];

            for (int i = 0; i < textureFaceQueues.Length; i++)
            {
                textureFaceQueues[i] = new Queue<Surface>();
            }

            textureGroupQueue = new Queue<RenderGroup>();

            //Console.WriteLine(lightmapTextures.Length + " generated lightmap textures");
            Console.WriteLine(vertList.Count + " generated vertices");
            Console.WriteLine(indexList.Count + " generated indices");
            Console.WriteLine(surfaces.Length + " generated surfaces");
            Console.WriteLine((indexList.Count / 3) + " generated triangles");

            Console.WriteLine(nodes.Length + " total bsp nodes");
            Console.WriteLine(leaves.Length + " total bsp leaves");

            vb = new VertexBuffer(GraphicsDevice, WorldVertex.VertexDeclaration, vertList.Count, BufferUsage.WriteOnly);
            vb.SetData(vertList.ToArray());
            vertList = null;

            //ib = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indexList.Count, BufferUsage.WriteOnly);
            indices = indexList.ToArray();
            dynamicIndices = new int[indices.Length];
            //ib.SetData(indexList.ToArray());
            indexList = null;

            dib = new DynamicIndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, dynamicIndices.Length, BufferUsage.WriteOnly);

            visData = mapFile.visData;
            leafFaces = mapFile.leafFaces;

            foreach (Entity entity in entities)
            {
                if (entity != null)
                    entity.Initialize();
            }

            GC.Collect();

            KConsole.Log("Loaded map " + mapToLoad);
        }

        List<Vector3> f_points = new List<Vector3>();
        List<Vector2> f_uvs = new List<Vector2>();
        List<Vector2> f_luvs = new List<Vector2>();

        Vector2 ScaleUV0To1(Vector2 uv, Vector2 min, Vector2 max)
        {
            Vector2 difference = max - min;
            uv = (uv - min) / difference;
            return uv;
        }

        // ehh
        Vector2 GetLightmapUV(Vector2 uv, int lightmapID)
        {
            return (uv + lightmapAtlas.uvs[lightmapID].min) / new Vector2(lightmapAtlas.atlasSize);
        }

        void CalcFaceLightmapUVs(Surface face)
        {
            if (face.lightmapID != -1)
            {
                Vector2 minUV = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                Vector2 maxUV = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

                for (int j = 0; j < face.numVerts; j++)
                {
                    int vertexIndex = face.baseVertex + j;
                    WorldVertex vertex = vertList[vertexIndex];
                    Vector2 uv = vertex.LightmapCoordinate;

                    if (uv.X < minUV.X)
                        minUV.X = uv.X;
                    if (uv.Y < minUV.Y)
                        minUV.Y = uv.Y;

                    if (uv.X > maxUV.X)
                        maxUV.X = uv.X;
                    if (uv.Y > maxUV.Y)
                        maxUV.Y = uv.Y;
                }

                //Vector2 properMinUV = new Vector2();
                //properMinUV.X = (int)(minUV.X / 8f) * 8f;
                //properMinUV.Y = (int)(minUV.Y / 8f) * 8f;

                for (int j = 0; j < face.numVerts; j++)
                {
                    int vertexIndex = face.baseVertex + j;
                    WorldVertex vertex = vertList[vertexIndex];
                    Vector2 luv = vertex.LightmapCoordinate;
                    
                    luv -= minUV;
                    luv /= new Vector2(lightmapAtlas.atlasSize * 16, lightmapAtlas.atlasSize * 16);
                    luv += lightmapAtlas.uvs[face.lightmapID].min;

                    vertex.LightmapCoordinate = luv;
                    vertList[vertexIndex] = vertex;
                }
            }
        }

        void BuildFace(BspFile mapFile, int i)
        {
            BspFile.Surface face = mapFile.surfaces[i];

            BspFile.TextureInfo texinfo = mapFile.textureInfos[face.textureInfo];
            MipTexture miptex;
            if (mapFile.textures == null)
            {
                miptex = missingTex;
            }
            else
            {
                miptex = mapFile.textures[texinfo.iMiptex];
            }

            Surface mf = new Surface();
            //mf.textureName = miptex.name;
            mf.id = i;
            surfaces[i] = mf;

            if (miptex.name == "sky" || miptex.name == "clip" || miptex.name == "aaatrigger")
                mf.type = SurfaceRenderType.DontDraw;

            int texIndex = 0;
            {
                string texName = miptex.name;

                if (texName.Length > 0 && texName[0] == '-')
                {
                    texName = miptex.name.Substring(2);
                }

                List<int> texSet = textureNameToID[texName];
                if (texSet.Count > 1)
                {
                    int pick = rand.Next() % texSet.Count;
                    texIndex = texSet[pick];
                }
                else
                {
                    texIndex = texSet[0];
                }
                
            }

            mf.textureID = texIndex;

            float texWidth = textures[texIndex].Width;
            float texHeight = textures[texIndex].Height;

            Vector3 normal = mapFile.planes[face.plane].Normal;
            mf.plane = mapFile.planes[face.plane];

            if (face.planeSide != 0)
            {
                normal *= -1f;
                mf.plane.Normal *= -1f;
            }

            Vector2 minUV = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 maxUV = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

            uint edgeIndex = face.firstSurfaceEdge;
            for (int j = 0; j < face.surfaceEdgeCount; j++)
            {
                int surfEdge = mapFile.surfaceEdges[edgeIndex + j];
                BspFile.Edge edge = mapFile.edges[Math.Abs(surfEdge)];

                int index = surfEdge >= 0 ? edge.v1 : edge.v2;
                Vector3 vertex = mapFile.vertices[index];
                f_points.Add(vertex);

                //u = x * u_axis.x + y * u_axis.y + z * u_axis.z + u_offset
                //v = x * v_axis.x + y * v_axis.y + z * v_axis.z + v_offset

                //float s = (Vector3.Dot(vertex, texinfo.vS) + texinfo.fSShift);
                //float t = (Vector3.Dot(vertex, texinfo.vT) + texinfo.fTShift);
                //Vector2 uv = new Vector2(s, t);

                // makes no real difference I guess but probably faster
                Vector2 uv = new Vector2(
                    vertex.X * texinfo.vS.X + vertex.Y * texinfo.vS.Y + vertex.Z * texinfo.vS.Z + texinfo.fSShift,
                    vertex.X * texinfo.vT.X + vertex.Y * texinfo.vT.Y + vertex.Z * texinfo.vT.Z + texinfo.fTShift);

                if (uv.X < minUV.X)
                    minUV.X = uv.X;
                if (uv.Y < minUV.Y)
                    minUV.Y = uv.Y;

                if (uv.X > maxUV.X)
                    maxUV.X = uv.X;
                if (uv.Y > maxUV.Y)
                    maxUV.Y = uv.Y;

                f_uvs.Add(uv);
                f_luvs.Add(uv);
            }

            Vector2 uvDim = maxUV - minUV;

            int lightMapWidth = (int)Math.Ceiling(maxUV.X / 16) - (int)Math.Floor(minUV.X / 16) + 1;
            int lightMapHeight = (int)Math.Ceiling(maxUV.Y / 16) - (int)Math.Floor(minUV.Y / 16) + 1;

            int lightmapTexIndex = -1;
            if (mapFile.lightmapData != null)
            {
                int styleCount = 0;

                for (int j = 0; j < face.lightStyles.Length; j++)
                {
                    if (face.lightStyles[j] == 255)
                        break;
                    styleCount++;
                }

                if (face.lightmapOffset != -1)
                {
                    lightmapTexIndex = CreateLightmapTexture(face.lightmapOffset, lightMapWidth, lightMapHeight);
                    //if (styleCount >= 2)
                    //    lightmapTexIndex = CreateLightmapTexture(face.lightmapOffset + (lightMapWidth * lightMapHeight), lightMapWidth, lightMapHeight);
                }


            }
            mf.lightmapID = lightmapTexIndex;

            //int blockSize = 256;

            Vector2 min16 = new Vector2((int)Math.Floor(minUV.X / 16), (int)Math.Floor(minUV.Y / 16));
            min16 *= 16;
            Vector2 diff = minUV - min16;

            for (int j = 0; j < f_uvs.Count; j++)
            {
                //Vector2 lmUV = (f_luvs[j] - minUV) / uvDim;
                //Vector2 lmUV = f_luvs[j];

                //lmUV -= minUV;

                //// some half-offset
                ////lmUV += new Vector2(8);
                //// use the lightmap atlas size instead when that is added
                ////lmUV /= new Vector2(lightMapWidth * 16, lightMapHeight * 16);
                ////lmUV /= new Vector2(LightmapAtlas.PAGE_WIDTH * 16, LightmapAtlas.PAGE_HEIGHT * 16);

                //f_luvs[j] = lmUV;
                f_uvs[j] = new Vector2(f_uvs[j].X / texWidth, f_uvs[j].Y / texHeight);
            }

            mf.start = mf.indicesStart = indexList.Count;
            mf.baseVertex = vertList.Count;
            int baseIndex = vertList.Count;

            // create vertices
            for (int k = 0; k < f_points.Count; k++)
            {
                vertList.Add(new WorldVertex(f_points[k], normal, f_uvs[k], f_luvs[k]));
                mf.numVerts++;
            }

            // convert to triangles
            for (int k = 2; k < f_points.Count; k++)
            {
                indexList.Add(baseIndex);
                indexList.Add(baseIndex + (k - 1));
                indexList.Add(baseIndex + k);
                mf.triCount++;
            }

            mf.indicesLength = indexList.Count - mf.indicesStart;

            f_points.Clear();
            f_uvs.Clear();
            f_luvs.Clear();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            //for (int i = 0; i < lightmapTextures.Length; i++)
            //{
            //    lightmapTextures[i].Dispose();
            //}

            vb.Dispose();
            //ib.Dispose();
            dib.Dispose();
        }

        KeyboardState oldKB;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //updateTimer.Reset();
            //updateTimer.Start();


            //ColliderSystem.Update(gameTime);

            updateTimer.Restart();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            Input.Update((float)gameTime.ElapsedGameTime.TotalSeconds, (float)gameTime.TotalGameTime.TotalSeconds);
            KConsole.Update();

            // TODO: Add your update logic here
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds * 1;

            // TODO: Add your update logic here
            //space.Update();
            KeyboardState kb = Keyboard.GetState();

            if (!KConsole.Active)
            {
                if (kb.IsKeyDown(Keys.Left))
                    cameraYawAngle += delta * 90f;
                if (kb.IsKeyDown(Keys.Right))
                    cameraYawAngle -= delta * 90f;

                if (kb.IsKeyDown(Keys.Up))
                    cameraPitchAngle -= delta * 45f;
                if (kb.IsKeyDown(Keys.Down))
                    cameraPitchAngle += delta * 45f;

                if (cameraYawAngle < 0f)
                    cameraYawAngle += 360f;
                cameraYawAngle %= 360f;
            }

            Vector3 proposedMove = Vector3.Zero;

            float walkSpeed = 190f;

            if (!KConsole.Active)
            {
                if (kb.IsKeyDown(Keys.LeftShift))
                    walkSpeed *= 3f;

                if (kb.IsKeyDown(Keys.Q))
                    proposedMove -= Vector3.UnitZ;
                if (kb.IsKeyDown(Keys.E))
                    proposedMove += Vector3.UnitZ;
                if (kb.IsKeyDown(Keys.W))
                    proposedMove += Vector3.UnitX;
                if (kb.IsKeyDown(Keys.S))
                    proposedMove -= Vector3.UnitX;
                if (kb.IsKeyDown(Keys.A))
                    proposedMove += Vector3.UnitY;
                if (kb.IsKeyDown(Keys.D))
                    proposedMove -= Vector3.UnitY;

            }

            if (proposedMove.LengthSquared() > 0f)
            {
                proposedMove.Normalize();
                proposedMove = Vector3.TransformNormal(proposedMove, Matrix.CreateRotationZ(MathHelper.ToRadians(cameraYawAngle)));
                playerPosition += proposedMove * delta * walkSpeed;
            }

            if (!KConsole.Active)
            {
                if (kb.IsKeyDown(Keys.D1) && oldKB.IsKeyUp(Keys.D1))
                {
                    wireframeOn = !wireframeOn;
                }

                if (kb.IsKeyDown(Keys.D2) && oldKB.IsKeyUp(Keys.D2))
                {
                    freezeVisibility = !freezeVisibility;
                }

                if (kb.IsKeyDown(Keys.D3) && oldKB.IsKeyUp(Keys.D3))
                {
                    defaultLighting = !defaultLighting;
                }

                if (kb.IsKeyDown(Keys.D4) && oldKB.IsKeyUp(Keys.D4))
                {
                    overdrawView = !overdrawView;
                }
            }
            
            base.Update(gameTime);
            oldKB = kb;

            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }

            // artificial load
            //System.Threading.Thread.Sleep(2);

            //updateTimer.Stop();
            lastUpdateTime = updateTimer.Elapsed;
        }

        EffectPass defaultPass, lightmapPass, overdrawPass;
        //EffectParameter defaultTex, lightmapTex1, lightmapTex2;

        int lastDrawnTexID = -1;

        void ClearFaceQueueBlock()
        {
            //for (int i = 0; i < faceQueued.Length; i++)
            //{
            //    faceQueued[i] = false;
            //}
            Array.Clear(faceQueued, 0, faceQueued.Length);
        }

        int duplicateFaceQueues = 0;

        void QueueFace(Surface face)
        {
            if (face.type == SurfaceRenderType.DontDraw)
                return;

            // For some dumbass reason, leaves can overlap with the faces they contain.
            // So, to "solve" the issue, I'll just check if a face already got queued for rendering.
            // Faces get checked against a boolean array that is reset every time before queueing happens.
            //if (!textureFaceQueues[face.textureID].Contains(face))
            if (!faceQueued[face.id])
            {
                textureFaceQueues[face.textureID].Enqueue(face);
                faceQueued[face.id] = true;
            }
            else
            {
                duplicateFaceQueues++;
            }


        }

        //void DrawFace(Face face)
        //{
        //    if (face.type == FaceType.DontDraw)
        //        return;

        //    if (defaultLighting || face.lightmapID == -1)
        //    {
        //        if (lastDrawnTexID != face.textureID)
        //        {
        //            worldEffect.Texture = textures[face.textureID];
        //            lastDrawnTexID = face.textureID;
        //        }
        //        defaultPass.Apply();
        //    }
        //    else
        //    {
        //        if (lastDrawnTexID != face.textureID)
        //        {
        //            //lightmapTex1.SetValue(textures[face.textureID]);
        //            lightmapWorldEffect.DiffuseTexture = textures[face.textureID];
        //            lastDrawnTexID = face.textureID;
        //        }
        //        //lightmapTex2.SetValue(lightmapTextures[face.lightmapID]);
        //        lightmapWorldEffect.LightmapTexture = lightmapTextures[face.lightmapID];
        //        lightmapPass.Apply();
        //    }

        //    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, face.baseVertex, face.start, face.numVerts, face.start, face.triCount);
        //    drawnFaces++;
        //}

        int drawnLeaves = 0;
        int drawnFaces = 0;
        int drawnGroups = 0;

        void DrawLeaf(Leaf leaf)
        {
            //if (!leafVisList[leaf.id])
            //    return;

            //ContainmentType contain = viewFrustum.Contains(leaf.bb);
            //bool canDraw = contain == ContainmentType.Contains || contain == ContainmentType.Intersects;
            //if (!canDraw)
            //    return;

            for (int surf = leaf.firstLeafFace; surf < leaf.firstLeafFace + leaf.leafFaceCount; surf++)
            {
                int markSurface = leafFaces[surf];

                Surface face = surfaces[markSurface];
                //if (face != null)
                {
                    //DrawFace(face);
                    QueueFace(face);
                }
            }

            drawnLeaves++;
            //if (drawnLeaves > 1)
            //    throw new Exception();
        }

        // draws the visible leaves of a bsp tree, front to back
        public void RecursiveTreeDraw(Node node, Vector3 pos, bool[] vislist)
        {
            float dot;
            Vector3.Dot(ref node.plane.Normal, ref pos, out dot);
            // in front
            if (dot > node.plane.D)
            {
                if (node.frontNode != null)
                    RecursiveTreeDraw(node.frontNode, pos, vislist);
                else if (vislist[node.frontLeaf.id])
                    DrawLeaf(node.frontLeaf);

                if (node.backNode != null)
                    RecursiveTreeDraw(node.backNode, pos, vislist);
                else if (vislist[node.backLeaf.id])
                    DrawLeaf(node.backLeaf);
            }
            else // behind
            {
                if (node.backNode != null)
                    RecursiveTreeDraw(node.backNode, pos, vislist);
                else if (vislist[node.backLeaf.id])
                    DrawLeaf(node.backLeaf);

                if (node.frontNode != null)
                    RecursiveTreeDraw(node.frontNode, pos, vislist);
                else if (vislist[node.frontLeaf.id])
                    DrawLeaf(node.frontLeaf);
            }
        }

        public void SetModelTransform(Matrix world)
        {
            //lightmapWorldEffect.World = world;
            //worldEffect.World = world;
            modelTransform = world;
        }

        //public void DrawBspModel(BspModel model, Matrix transform)
        //{

        //}

        // draws all leaves of a bsp tree, front to back
        public void RecursiveTreeDraw(Node node, Vector3 pos)
        {
            float dot;
            Vector3.Dot(ref node.plane.Normal, ref pos, out dot);
            // in front
            if (dot > node.plane.D)
            {
                if (node.frontNode != null)
                    RecursiveTreeDraw(node.frontNode, pos);
                else
                    DrawLeaf(node.frontLeaf);

                if (node.backNode != null)
                    RecursiveTreeDraw(node.backNode, pos);
                else
                    DrawLeaf(node.backLeaf);
            }
            else // behind
            {
                if (node.backNode != null)
                    RecursiveTreeDraw(node.backNode, pos);
                else
                    DrawLeaf(node.backLeaf);

                if (node.frontNode != null)
                    RecursiveTreeDraw(node.frontNode, pos);
                else
                    DrawLeaf(node.frontLeaf);
            }
        }

        // for models that have no positional offset or no special effects
        public void QueueStaticBspModel(BspModel model, bool[] vislist = null)
        {
            ModelDrawEntry entry = new ModelDrawEntry();
            entry.model = model;
            entry.transform = Matrix.Identity;
            entry.vislist = vislist;
            entry.renderMode = RenderMode.Normal;
            entry.renderColor = Color.White;
            modelQueueStatic.Enqueue(entry);
        }

        // models that have been moved or need special rendering done should be queued with this
        public void QueueDynamicBspModel(BspModel model, Matrix transform, RenderMode mode, Color color, bool[] vislist = null)
        {
            ModelDrawEntry entry = new ModelDrawEntry();
            entry.model = model;
            entry.transform = transform;
            entry.renderMode = mode;
            entry.renderColor = color;
            entry.vislist = vislist;
            modelQueueDynamic.Enqueue(entry);
        }

        void BuildBatchIndices()
        {
            dynamicIndex = 0;
            GraphicsDevice.Indices = null;
            int numTextures = textures.Length;
            for (int i = 0; i < numTextures; i++)
            {
                Queue<Surface> faceQueue = textureFaceQueues[i];

                if (faceQueue.Count == 0)
                    continue;

                RenderGroup group = new RenderGroup();
                group.start = dynamicIndex;
                group.texture = i;

                while (faceQueue.Count > 0)
                {
                    Surface face = faceQueue.Dequeue();
                    for (int j = 0; j < face.indicesLength; j++)
                    {
                        //if (dynamicIndex < dynamicIndices.Length)
                        dynamicIndices[dynamicIndex++] = indices[face.indicesStart + j];
                    }
                    group.numVerts += face.numVerts;
                    group.triCount += face.triCount;
                    drawnFaces++;
                }

                textureGroupQueue.Enqueue(group);
            }
            // if no triangles were added don't set data otherwise xna will scream at you
            if (dynamicIndex > 0)
                dib.SetData(dynamicIndices, 0, dynamicIndex, SetDataOptions.Discard);

            GraphicsDevice.SetVertexBuffer(vb);
            GraphicsDevice.Indices = dib;
        }

        void DrawModelBatch(Matrix transform)
        {
            while (textureGroupQueue.Count > 0)
            {
                RenderGroup group = textureGroupQueue.Dequeue();
                //worldEffect.LightingEnabled = true;

                //GraphicsDevice.BlendState = BlendState.Opaque;
                //GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                Texture2D texture = textures[group.texture];
                TextureProperties properties = textureProperties[group.texture];

                //if (texture.Tag != null && ((string)texture.Tag).StartsWith("GLASS_"))
                //{
                //    GraphicsDevice.DepthStencilState = ditherApplyDSS;
                //}
                //else
                //{
                //    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                //}

                // set up the appropriate effects
                if (overdrawView)
                {
                    overdrawEffect.World = transform;
                    overdrawPass.Apply();
                }
                else if (defaultLighting)
                {
                    worldEffect.World = transform;
                    worldEffect.Texture = texture;
                    defaultPass.Apply();
                }
                else
                {
                    lightmapWorldEffect.World = transform;
                    lightmapWorldEffect.DiffuseTexture = texture;
                    // incorrect
                    //if (properties.light)
                    //    lightmapWorldEffect.LightmapEnabled = false;
                    //else
                    //    lightmapWorldEffect.LightmapEnabled = true;
                    lightmapWorldEffect.LightmapTexture = lightmapAtlas.texture;
                    lightmapPass.Apply();
                }

                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, group.start, group.numVerts, group.start, group.triCount);
                drawnGroups++;
            }
        }

        int staticModels = 0;
        int dynamicModels = 0;
       
        void DrawBspQueue()
        {
            staticModels = modelQueueStatic.Count;
            dynamicModels = modelQueueDynamic.Count;

            if (overdrawView)
            {
                GraphicsDevice.BlendState = BlendState.Additive;
                GraphicsDevice.DepthStencilState = DepthStencilState.None;
            }

            // draw static models, all non-transformed models can be drawn at once
            ClearFaceQueueBlock();
            SetModelTransform(Matrix.Identity);

            lightmapWorldEffect.LightmapEnabled = true;
            lightmapWorldEffect.DiffuseColor = Vector3.One;

            while (modelQueueStatic.Count > 0)
            {
                ModelDrawEntry entry = modelQueueStatic.Dequeue();

                if (entry.vislist == null)
                {
                    RecursiveTreeDraw(entry.model.rootNode, TransformedVisPosition);
                }
                else
                {
                    RecursiveTreeDraw(entry.model.rootNode, TransformedVisPosition, entry.vislist);
                }
            }

            BuildBatchIndices();
            DrawModelBatch(Matrix.Identity);

            // draw dynamic models individually since they most likely have different transforms
            while (modelQueueDynamic.Count > 0)
            {
                ModelDrawEntry entry = modelQueueDynamic.Dequeue();

                lightmapWorldEffect.DiffuseColor = entry.renderColor.ToVector3();

                if (overdrawView)
                {
                    GraphicsDevice.BlendState = BlendState.Additive;
                    GraphicsDevice.DepthStencilState = DepthStencilState.None;
                }
                else
                {
                    GraphicsDevice.BlendState = BlendState.Opaque;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                    lightmapWorldEffect.LightmapEnabled = true;

                    if (entry.renderMode == RenderMode.Dither_EXT)
                    {
                        GraphicsDevice.DepthStencilState = ditherApplyDSS;
                        //lightmapWorldEffect.LightmapEnabled = false;
                    }
                    if (entry.renderMode == RenderMode.Additive) 
                    {
                        GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                        GraphicsDevice.BlendState = BlendState.Additive;
                        lightmapWorldEffect.LightmapEnabled = false;
                    }
                }

                ClearFaceQueueBlock();
                SetModelTransform(entry.transform);

                if (entry.vislist == null)
                {
                    RecursiveTreeDraw(entry.model.rootNode, TransformedVisPosition);
                }
                else
                {
                    RecursiveTreeDraw(entry.model.rootNode, TransformedVisPosition, entry.vislist);
                }

                BuildBatchIndices();
                DrawModelBatch(entry.transform);
            }

        }

        void DrawScene(GameTime gameTime, int addRotation, string viewName = null)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float total = (float)gameTime.TotalGameTime.TotalSeconds;

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            if (wireframeOn)
                GraphicsDevice.RasterizerState = wireframeRS;

            // diffuse texture and detail texture
            GraphicsDevice.SamplerStates[0] = worldSS; // AnisotropicWrap
            //GraphicsDevice.SamplerStates[2] = worldSS;

            // lightmap texture
            //GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            GraphicsDevice.BlendState = BlendState.Opaque;

            Matrix cameraRotation = Matrix.CreateRotationY(MathHelper.ToRadians(cameraPitchAngle)) * Matrix.CreateRotationZ(MathHelper.ToRadians(cameraYawAngle + addRotation));

            // camera offset is 64, player height is 72
            Vector3 cameraOffset = Vector3.Zero;//new Vector3(0, 0, 64);
            Vector3 normal = Vector3.TransformNormal(Vector3.UnitX, cameraRotation);
            view = Matrix.CreateLookAt(playerPosition + cameraOffset, playerPosition + cameraOffset + normal, Vector3.UnitZ);

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), 
                (float)Window.ClientBounds.Width / (float)Window.ClientBounds.Height, 
                10f, cameraClipDistance);

            //viewFrustum = new BoundingFrustum(view * projection);

            worldEffect.View = view;
            worldEffect.Projection = projection;
            defaultPass = worldEffect.CurrentTechnique.Passes[0];

            lightmapWorldEffect.View = view;
            lightmapWorldEffect.Projection = projection;
            lightmapPass = lightmapWorldEffect.CurrentTechnique.Passes[0];

            overdrawEffect.View = view;
            overdrawEffect.Projection = projection;
            overdrawPass = overdrawEffect.CurrentTechnique.Passes[0];

            if (!freezeVisibility)
            {
                viewFrustum = new BoundingFrustum(view * projection);
                visPosition = playerPosition;
                cameraLeaf = GetLeafFromPosition(visPosition);
                UpdateVisiblityFromLeaf(cameraLeaf);
                CalcVisBox();
            }

            // ======================== DRAW DRAW DRAW DRAW

            QueueStaticBspModel(models[0], leafVisList);

            for (int i = 0; i < entities.Length; i++)
            {
                if (entities[i] != null)
                {
                    entities[i].Draw(delta, total);
                }
            }

            DrawBspQueue();

            // ======================== DRAW DRAW DRAW DRAW

           /* spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            foreach (Entity entity in entities)
            {
                if (entity != null)
                {
                    bool isVisible = entity.IsOriginVisible;

                    if (!isVisible)
                        continue;


                    Vector3 screenPosition = GraphicsDevice.Viewport.Project(entity.WorldOrigin, projection, view, Matrix.Identity);
                    if (screenPosition.Z >= 0 && screenPosition.Z <= 1)
                    {
                        spriteBatch.Draw(pixel, new Rectangle((int)screenPosition.X - 8, (int)screenPosition.Y - 8, 16, 16), Color.DarkRed);
                        spriteBatch.DrawString(hlFont, entity.classname, (int)screenPosition.X, (int)screenPosition.Y, Color.Red);
                    }
                }
            }
            spriteBatch.End();
           */
            if (viewName != null)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                //spriteBatch.DrawString(hlFont, viewName, new Vector2(0, GraphicsDevice.Viewport.Height - hlFont.LineSpacing) + Vector2.One, Color.Black);
                spriteBatch.DrawString(hlFont, viewName, 0, GraphicsDevice.Viewport.Height - hlFont.LineSpacing, Color.Red);
                spriteBatch.End();
            }
        }

        StringBuilder gcsb = new StringBuilder(64);

        int debugLineOffset = 0;
        void DrawDebugLine(string s, Color color)
        {
            //spriteBatch.DrawString(font, s, new Vector2(0, debugLineOffset) + Vector2.One, Color.Black);
            spriteBatch.DrawString(hlFont, s, 0, debugLineOffset, color);
            debugLineOffset += hlFont.LineSpacing;
        }
        void DrawDebugLine(StringBuilder s, Color color)
        {
            //spriteBatch.DrawString(font, s, new Vector2(0, debugLineOffset) + Vector2.One, Color.Black);
            spriteBatch.DrawString(hlFont, s, 0, debugLineOffset, color);
            debugLineOffset += hlFont.LineSpacing;
        }

        int timerOffset = 0;
        bool timerOffsetOdd = false;
        void DrawUpdateTimer(string label, TimeSpan timeSpan, TimeSpan target, Color coolColor, Color hotColor)
        {
            //spriteBatch.Begin();

            Rectangle frameTimeRect = new Rectangle(GraphicsDevice.Viewport.Width - 200, timerOffset, 200, hlFont.LineSpacing);

            Color bgColor = new Color(32, 32, 32);
            if (timerOffsetOdd)
                bgColor = new Color(40, 40, 40);
            

            double frameTimeMultiplier = timeSpan.TotalSeconds / target.TotalSeconds;

            Color frameTimeColor = coolColor;
            if (frameTimeMultiplier > 1d)
            {
                frameTimeColor = hotColor;
                frameTimeMultiplier = 1d;
            }

            spriteBatch.Draw(pixel, frameTimeRect, bgColor);
            //spriteBatch.DrawString(font, label, new Vector2(frameTimeRect.X, frameTimeRect.Y), frameTimeColor);
            frameTimeRect.Width = (int)(frameTimeRect.Width * frameTimeMultiplier);
            spriteBatch.Draw(pixel, frameTimeRect, frameTimeColor);
            //Rectangle oldScissor = GraphicsDevice.ScissorRectangle;

            //spriteBatch.End();

            //GraphicsDevice.RasterizerState = scissorRS;
            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, scissorRS);
            //GraphicsDevice.ScissorRectangle = frameTimeRect;
            spriteBatch.DrawString(hlFont, label, frameTimeRect.X, frameTimeRect.Y, Color.White);
            //GraphicsDevice.ScissorRectangle = oldScissor;
            //spriteBatch.End();
            //GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            timerOffset += hlFont.LineSpacing;
            timerOffsetOdd = !timerOffsetOdd;
        }

        bool ditherOdd = false;
        protected override void Draw(GameTime gameTime)
        {
            //drawTimer.Reset();
            //drawTimer.Start();
            drawTimer.Restart();

            ditherOdd = !ditherOdd;

            if (refreshSSRT && preferSuperSampling)
            {
                CreateSuperSampleRT(Window.ClientBounds.Width * superSampleScale.X, Window.ClientBounds.Height * superSampleScale.Y);
                refreshSSRT = false;
            }

            if (preferSuperSampling && useSuperSampling)
            {
                GraphicsDevice.SetRenderTarget(superSampleRT);
            }

            GraphicsDevice.Clear(Color.Black);

            int ditherOffset = (ditherOdd && doDitherFlip) ? 1 : 0;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, ditherCreateDSS, null, alphaTestEffect);
            spriteBatch.Draw(stencilMask,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                new Rectangle(ditherOffset, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            spriteBatch.End();

            if (overdrawView)
            {
                GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.Black, 1, 0);
            }
            else
            {
                GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, skyColor, 1, 0);
            }

            var alphaTestMatrix = Matrix.CreateOrthographicOffCenter(0,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height,
                0, 0, 1
            );

            alphaTestEffect.Projection = alphaTestMatrix;

            

            //Viewport defaultViewport = GraphicsDevice.Viewport;
            //Viewport splitTL = new Viewport(defaultViewport.X, defaultViewport.Y, defaultViewport.Width / 2, defaultViewport.Height / 2);
            //Viewport splitBL = new Viewport(splitTL.X, splitTL.Bounds.Bottom, splitTL.Width, splitTL.Height);
            //Viewport splitTR = new Viewport(splitTL.Bounds.Right, defaultViewport.Y, splitTL.Width, splitTL.Height);
            //Viewport splitBR = new Viewport(splitTL.Bounds.Right, splitTL.Bounds.Bottom, splitTL.Width, splitTL.Height);

            lastDrawnTexID = -1;
            drawnFaces = 0;
            drawnLeaves = 0;
            drawnGroups = 0;
            debugLineOffset = 0;
            timerOffset = 0;
            timerOffsetOdd = false;
            duplicateFaceQueues = 0;

            DrawScene(gameTime, 0);

            //GraphicsDevice.Viewport = splitTL;
            //DrawScene(gameTime, 0, "FRONT");
            //GraphicsDevice.Viewport = splitBL;
            //DrawScene(gameTime, 90, "LEFT");
            //GraphicsDevice.Viewport = splitTR;
            //DrawScene(gameTime, 180, "BACK");
            //GraphicsDevice.Viewport = splitBR;
            //DrawScene(gameTime, 270, "RIGHT");

            //GraphicsDevice.Viewport = defaultViewport;

            GraphicsDevice.SetRenderTarget(null);

            if (preferSuperSampling && useSuperSampling)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, null, null);
                spriteBatch.Draw(superSampleRT,
                    new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                    Color.White);
                spriteBatch.End();
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, 
                BlendState.AlphaBlend, 
                SamplerState.PointClamp, 
                DepthStencilState.None, 
                RasterizerState.CullCounterClockwise);

            KConsole.SetResolution(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            KConsole.Draw(spriteBatch, hlFont, (float)gameTime.ElapsedGameTime.TotalSeconds, (float)gameTime.TotalGameTime.TotalSeconds);

            DrawDebugLine("Position: " + playerPosition.ToString(), Color.White);

            string visString = "Leaf/Custer: " + cameraLeaf.id.ToString() + "/" + cameraLeaf.visCluster;
            if (freezeVisibility)
                visString += " (VIS FROZEN)";
            DrawDebugLine(visString, Color.White);

            string drawCountString = "Leaves/Faces/Groups: " + drawnLeaves + "/" + drawnFaces + "/" + drawnGroups;
            DrawDebugLine(drawCountString, Color.White);

            // Static/Dynamic
            string modelQueueString = "S/D Models: " + staticModels + "/" + dynamicModels;
            DrawDebugLine(modelQueueString, Color.White);


           
            //DrawDebugLine("Duplicate Faces: " + duplicateFaceQueues, Color.White);

            frameCounter++;

            string fps = string.Format("FPS: {0}", frameRate);
            DrawDebugLine(fps, Color.White);

            //string gc = "GC: " + GC.GetTotalMemory(false) + " bytes";
            //gcsb.Clear();
            //gcsb.Append("GC: ");
            //gcsb.Append(GC.GetTotalMemory(false));
            //gcsb.Append(" bytes");
            //DrawDebugLine(gcsb, Color.White);

            // update time
            DrawDebugLine("UT: " + lastUpdateTime.TotalMilliseconds.ToString("0.00") + " ms", Color.White);
            // frame time
            DrawDebugLine("FT: " + lastDrawTime.TotalMilliseconds.ToString("0.00") + " ms", Color.White);
            TimeSpan totalTime = (lastUpdateTime + lastDrawTime);
            // total time
            DrawDebugLine("TT: " + totalTime.TotalMilliseconds.ToString("0.00") + " ms", Color.White);
            // max time
            DrawDebugLine("MT: " + TargetElapsedTime.TotalMilliseconds.ToString("0.00") + " ms", Color.White);

            DrawUpdateTimer("update", lastUpdateTime, TargetElapsedTime, Color.DarkOrange, Color.Red);
            DrawUpdateTimer("draw", lastDrawTime, TargetElapsedTime, Color.Indigo, Color.Red);
            DrawUpdateTimer("total", totalTime, TargetElapsedTime, Color.Blue, Color.Red);

            //string newLineTestString = "This is a\nNewline test!\nOh my god so many\nL\ni\nn\ne\ns\n.";

            //Vector2 newLineTestSize = hlFont.MeasureString(newLineTestString);
            //spriteBatch.Draw(pixel, new Rectangle(200, 200, (int)newLineTestSize.X, (int)newLineTestSize.Y), Color.Gray);
            //spriteBatch.DrawString(hlFont, newLineTestString, new Vector2(200, 200), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);

            //drawTimer.Stop();
            lastDrawTime = drawTimer.Elapsed;
        }
    }
}
