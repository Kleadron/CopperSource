using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.Collections;
using System.Text;

namespace CopperSource
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Matrix view;
        Matrix projection;

        Vector3 playerPosition = new Vector3(0, 0, 0);
        Vector3 visPosition;
        float cameraYawAngle = 0f;
        float cameraPitchAngle = 0f;

        //BspFile mapFile;
        SpriteFont font;

        BasicEffect lineEffect;
        BasicEffect worldEffect;
        //DualTextureEffect lightmapWorldEffect;
        LightmapEffect lightmapWorldEffect;

        VertexBuffer vb;
        IndexBuffer ib;

        public List<WorldVertex> vertList = new List<WorldVertex>();
        public List<ushort> indexList = new List<ushort>();

        Face[] mapFaces;
        Dictionary<string, List<int>> textureNameToID = new Dictionary<string,List<int>>();
        Texture2D[] textures;
        List<Texture2D> texList = new List<Texture2D>();
        Texture2D[] lightmapTextures;
        List<Texture2D> lightmapList = new List<Texture2D>();
        int nextTexIndex = 1;

        Texture2D grid, pixel, graypixel;

        RasterizerState wireframeRS;

        bool wireframeOn = false;
        bool freezeVisibility = false;
        bool defaultLighting = false;

        Node rootNode;
        Node[] nodes;
        Leaf[] leaves;
        bool[] leafVisList;
        Leaf cameraLeaf;

        BspModel[] models;

        byte[] visData;
        ushort[] markSurfaces;

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

        public Game1()
        {
            //Vector3 v = DataHelper.ValueToVector3("3.0 4.1 333");

            Point res = new Point(1280, 720);

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = res.X;
            graphics.PreferredBackBufferHeight = res.Y;
            graphics.PreferMultiSampling = false;

            KConsole.SetResolution(res.X, res.Y);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
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

        int CreateLightmapTexture(byte[] lightmapData, int offset, int width, int height)
        {
            //Console.WriteLine(width + "x" + height);
            
            int texIndex = lightmapList.Count;
            Texture2D tex = new Texture2D(GraphicsDevice, width, height, false, SurfaceFormat.Color);
            lightmapList.Add(tex);

            Color[] colors = new Color[width * height];
            byte[] lmData = lightmapData;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dataIndex = (x + (y * width)) * 3;
                    int colorIndex = (x + (y * width));
                    Color c = Color.White;
                    c.R = lmData[offset + dataIndex];
                    c.G = lmData[offset + dataIndex + 1];
                    c.B = lmData[offset + dataIndex + 2];

                    colors[colorIndex] = c;
                }
            }

            tex.SetData(colors);

            return texIndex;
        }

        Node BuildNode(BspFile mapData, int index)
        {
            Node node = new Node();
            node.id = index;
            //node.parentNode = parent;
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
                node.frontLeaf = BuildLeaf(mapData, frontChild);
            }
            else
            {
                node.frontNode = BuildNode(mapData, frontChild);
            }

            short backChild = mapNode.backChild;
            // <= 0 is a leaf node
            if (backChild <= 0)
            {
                backChild = (short)~backChild;
                node.backLeaf = BuildLeaf(mapData, backChild);
            }
            else
            {
                node.backNode = BuildNode(mapData, backChild);
            }

            return node;
        }

        Leaf BuildLeaf(BspFile mapData, int index)
        {
            Leaf leaf = new Leaf();
            leaf.id = index;
            //leaf.parentNode = parent;
            leaves[index] = leaf;
            BspFile.Leaf mapLeaf = mapData.leaves[index];

            leaf.bb = new BoundingBox(new Vector3(mapLeaf.minX, mapLeaf.minY, mapLeaf.minZ),
                new Vector3(mapLeaf.maxX, mapLeaf.maxY, mapLeaf.maxZ));

            leaf.firstMarkSurface = mapLeaf.firstMarkSurface;
            leaf.nMarkSurfaces = mapLeaf.nMarkSurfaces;
            leaf.visCluster = mapLeaf.visOffset;

            return leaf;
        }

        public Leaf GetLeafFromPosition(Vector3 pos)
        {
            Node searchNode = rootNode;
            while (true)
            {
                float dot;
                Vector3.Dot(ref searchNode.plane.Normal, ref pos, out dot);
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

            for (int c = 1; c < leaves.Length; v++)
            {

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

        BspFile.MipTexture missingTex;

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            font = Content.Load<SpriteFont>("Fonts/Font");

            pixel = Content.Load<Texture2D>("Textures/pixel");
            graypixel = Content.Load<Texture2D>("Textures/graypixel");

            wireframeRS = new RasterizerState();
            wireframeRS.CullMode = CullMode.None;
            wireframeRS.FillMode = FillMode.WireFrame;
            //wireframeRS.

            lineEffect = new BasicEffect(GraphicsDevice);
            worldEffect = new BasicEffect(GraphicsDevice);

            if (true)
            {
                worldEffect.EnableDefaultLighting();

                worldEffect.DirectionalLight0.Hammerize();
                worldEffect.DirectionalLight1.Hammerize();
                worldEffect.DirectionalLight2.Hammerize();

                worldEffect.PreferPerPixelLighting = true;
                //worldEffect.SpecularColor = Vector3.Zero;
            }

            worldEffect.TextureEnabled = true;
            grid = worldEffect.Texture = Content.Load<Texture2D>("Textures/tiledark_s");

            BspFile mapFile = new BspFile("Content/Maps/de_dust2.bsp");
            //File.WriteAllText("entities.txt", mapFile.entityData);

            missingTex = new BspFile.MipTexture();
            missingTex.name = "missing";
            missingTex.width = (uint)grid.Width;
            missingTex.height = (uint)grid.Height;

            lightmapWorldEffect = new LightmapEffect(Content.Load<Effect>("Effects/LightmapEffect"));

            //lightmapWorldEffect.DetailTextureEnabled = true;
            lightmapWorldEffect.DetailScale = new Vector2(3, 3);
            lightmapWorldEffect.DetailTexture = Content.Load<Texture2D>("Textures/ai_detail");

            lightmapWorldEffect.FogEnabled = true;
            lightmapWorldEffect.FogStart = fogStart;
            lightmapWorldEffect.FogEnd = fogEnd;
            lightmapWorldEffect.FogColor = skyColor.ToVector3();

            //textureNameToID = new Dictionary<string, int>();
            
            mapFaces = new Face[mapFile.faces.Length];

            texList.Add(grid);
            textureNameToID["missing"] = new List<int>();
            textureNameToID["missing"].Add(0);

            if (mapFile.textures != null)
            for (int i = 0; i < mapFile.textures.Length; i++)
            {
                BspFile.MipTexture miptex = mapFile.textures[i];
                string fileName = miptex.name;
                string realName = miptex.name;
                bool isRandomized = false;
                int texNum = 0;

                if (miptex.name[0] == '-')
                {
                    realName = miptex.name.Substring(2);
                    texNum = int.Parse(miptex.name[1].ToString());
                    isRandomized = true;
                }

                //Console.WriteLine(miptex.name);
                if (File.Exists(Content.RootDirectory + "/Textures/Maptextures/" + fileName + ".xnb"))
                {
                    Texture2D tex = Content.Load<Texture2D>("Textures/Maptextures/" + fileName);

                    if (isRandomized)
                    {
                        Console.WriteLine(realName + " " + texNum);

                        if (!textureNameToID.ContainsKey(realName))
                        {
                            textureNameToID[realName] = new List<int>();
                        }
                        textureNameToID[realName].Add(nextTexIndex++);
                        texList.Add(tex);
                    }
                    else
                    {
                        textureNameToID[realName] = new List<int>();
                        textureNameToID[realName].Add(nextTexIndex++);
                        texList.Add(tex);
                    }
                }
                else
                {
                    textureNameToID[realName] = new List<int>();
                    textureNameToID[realName].Add(0);
                }
            }

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

                mdl.firstFace = model.firstFace;
                mdl.numFaces = model.nFaces;
                mdl.numLeaves = model.visleafs;
                mdl.bb = new BoundingBox(model.min, model.max);
                mdl.rotationalOrigin = model.origin;
                mdl.center = model.min + ((mdl.bb.Max - mdl.bb.Min) / 2);

                for (int i = model.firstFace; i < model.firstFace + model.nFaces; i++)
                {
                    BuildFace(mapFile, i);
                    // THIS IS THE END OF A FACE GENERATION LOOP. DO NOT PUT STUFF HERE.
                }

                mdl.rootNode = BuildNode(mapFile, model.node1);
                //Console.WriteLine("Generated BSP for model " + mI);
                models[mI] = mdl;
            }

            Console.WriteLine("Models generated: " + mapFile.models.Length);

            rootNode = nodes[0];

            lightmapTextures = lightmapList.ToArray();
            lightmapList.Clear();
            textures = texList.ToArray();
            texList.Clear();

            Console.WriteLine(lightmapTextures.Length + " generated lightmap textures");
            Console.WriteLine(vertList.Count + " generated vertices");
            Console.WriteLine(indexList.Count + " generated indices");
            Console.WriteLine(mapFaces.Length + " generated faces");
            Console.WriteLine((indexList.Count / 3) + " generated triangles");

            Console.WriteLine(nodes.Length + " total bsp nodes");
            Console.WriteLine(leaves.Length + " total bsp leaves");

            vb = new VertexBuffer(GraphicsDevice, WorldVertex.VertexDeclaration, vertList.Count, BufferUsage.WriteOnly);
            vb.SetData(vertList.ToArray());
            vertList = null;

            ib = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indexList.Count, BufferUsage.WriteOnly);
            ib.SetData(indexList.ToArray());
            indexList = null;

            visData = mapFile.visData;
            markSurfaces = mapFile.markSurfaces;

            GC.Collect();
        }

        List<Vector3> f_points = new List<Vector3>();
        List<Vector2> f_uvs = new List<Vector2>();
        List<Vector2> f_luvs = new List<Vector2>();

        void BuildFace(BspFile mapFile, int i)
        {
            BspFile.Face face = mapFile.faces[i];

            BspFile.TextureInfo texinfo = mapFile.textureInfos[face.textureInfo];
            BspFile.MipTexture miptex;
            if (mapFile.textures == null)
            {
                miptex = missingTex;
            }
            else
            {
                miptex = mapFile.textures[texinfo.iMiptex];
            }

            Face mf = new Face();
            //mf.textureName = miptex.name;
            mapFaces[i] = mf;

            if (miptex.name == "sky" || miptex.name == "clip" || miptex.name == "aaatrigger")
                mf.type = FaceType.DontDraw;

            int texIndex = 0;
            {
                string texName = miptex.name;

                if (texName[0] == '-')
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

            float texWidth = texList[texIndex].Width;
            float texHeight = texList[texIndex].Height;

            Vector3 normal = mapFile.planes[face.plane].Normal;
            mf.plane = mapFile.planes[face.plane];

            if (face.planeSide != 0)
            {
                normal *= -1f;
                mf.plane.Normal *= -1f;
            }

            Vector2 minUV = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 maxUV = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

            uint edgeIndex = face.firstEdge;
            for (int j = 0; j < face.nEdges; j++)
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
                if (face.lightmapOffset != -1)
                    lightmapTexIndex = CreateLightmapTexture(mapFile.lightmapData, face.lightmapOffset, lightMapWidth, lightMapHeight);
            }
            mf.lightmapID = lightmapTexIndex;

            //int blockSize = 256;

            Vector2 min16 = new Vector2((int)Math.Floor(minUV.X / 16), (int)Math.Floor(minUV.Y / 16));
            min16 *= 16;
            Vector2 diff = minUV - min16;

            for (int j = 0; j < f_uvs.Count; j++)
            {
                //Vector2 lmUV = (f_luvs[j] - minUV) / uvDim;
                Vector2 lmUV = f_luvs[j];

                lmUV -= minUV;

                // some half-offset
                lmUV += new Vector2(8);
                // use the lightmap atlas size instead when that is added
                lmUV /= new Vector2(lightMapWidth * 16, lightMapHeight * 16);

                f_luvs[j] = lmUV;
                f_uvs[j] = new Vector2(f_uvs[j].X / texWidth, f_uvs[j].Y / texHeight);
            }

            mf.start = indexList.Count;
            mf.baseVertex = vertList.Count;

            // create vertices
            for (int k = 0; k < f_points.Count; k++)
            {
                vertList.Add(new WorldVertex(f_points[k], normal, f_uvs[k], f_luvs[k]));
                mf.numVerts++;
            }

            // convert to triangles
            for (int k = 2; k < f_points.Count; k++)
            {
                indexList.Add(0);
                indexList.Add((ushort)(k - 1));
                indexList.Add((ushort)k);
                mf.triCount++;
            }

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
            for (int i = 0; i < lightmapTextures.Length; i++)
            {
                lightmapTextures[i].Dispose();
            }

            vb.Dispose();
            ib.Dispose();
        }

        KeyboardState oldKB;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
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
        }

        EffectPass defaultPass, lightmapPass;
        //EffectParameter defaultTex, lightmapTex1, lightmapTex2;

        int lastDrawnTexID = -1;

        void DrawFace(Face face)
        {
            if (face.type == FaceType.DontDraw)
                return;

            if (defaultLighting || face.lightmapID == -1)
            {
                if (lastDrawnTexID != face.textureID)
                {
                    worldEffect.Texture = textures[face.textureID];
                    lastDrawnTexID = face.textureID;
                }
                defaultPass.Apply();
            }
            else
            {
                if (lastDrawnTexID != face.textureID)
                {
                    //lightmapTex1.SetValue(textures[face.textureID]);
                    lightmapWorldEffect.DiffuseTexture = textures[face.textureID];
                    lastDrawnTexID = face.textureID;
                }
                //lightmapTex2.SetValue(lightmapTextures[face.lightmapID]);
                lightmapWorldEffect.LightmapTexture = lightmapTextures[face.lightmapID];
                lightmapPass.Apply();
            }

            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, face.baseVertex, face.start, face.numVerts, face.start, face.triCount);
            drawnFaces++;
        }

        int drawnLeaves = 0;
        int drawnFaces = 0;

        void DrawLeaf(Leaf leaf)
        {
            //if (!leafVisList[leaf.id])
            //    return;

            for (int surf = leaf.firstMarkSurface; surf < leaf.firstMarkSurface + leaf.nMarkSurfaces; surf++)
            {
                int markSurface = markSurfaces[surf];

                Face face = mapFaces[markSurface];
                //if (face != null)
                {
                    DrawFace(face);
                }
            }

            drawnLeaves++;
        }

        // REFERENCE CODE
        //
        //Node searchNode = rootNode;
        //while (true)
        //{
        //    // is the position in front or behind of the plane
        //    if (Vector3.Dot(searchNode.plane.Normal, pos) > searchNode.plane.D)
        //    {
        //        // in front of parition plane
        //        if (searchNode.frontLeaf != null)
        //        {
        //            return searchNode.frontLeaf;
        //        }
        //        else
        //        {
        //            searchNode = searchNode.frontNode;
        //        }
        //    }
        //    else
        //    {
        //        // behind partition plane
        //        if (searchNode.backLeaf != null)
        //        {
        //            return searchNode.backLeaf;
        //        }
        //        else
        //        {
        //            searchNode = searchNode.backNode;
        //        }
        //    }
        //}

        // draws the visible leaves of a bsp tree, front to back
        void RecursiveTreeDraw(Node node, Vector3 pos, bool[] vislist)
        {
            float dot;
            Vector3.Dot(ref node.plane.Normal, ref pos, out dot);
            // in front
            if (dot > node.plane.D)
            {
                if (node.frontNode != null)
                    RecursiveTreeDraw(node.frontNode, pos, vislist);
                else if (node.frontLeaf != null && vislist[node.frontLeaf.id])
                    DrawLeaf(node.frontLeaf);

                if (node.backNode != null)
                    RecursiveTreeDraw(node.backNode, pos, vislist);
                else if (node.backLeaf != null && vislist[node.backLeaf.id])
                    DrawLeaf(node.backLeaf);
            }
            else // behind
            {
                if (node.backNode != null)
                    RecursiveTreeDraw(node.backNode, pos, vislist);
                else if (node.backLeaf != null && vislist[node.backLeaf.id])
                    DrawLeaf(node.backLeaf);

                if (node.frontNode != null)
                    RecursiveTreeDraw(node.frontNode, pos, vislist);
                else if (node.frontLeaf != null && vislist[node.frontLeaf.id])
                    DrawLeaf(node.frontLeaf);
            }
        }

        // draws all leaves of a bsp tree, front to back
        void RecursiveTreeDraw(Node node, Vector3 pos)
        {
            float dot;
            Vector3.Dot(ref node.plane.Normal, ref pos, out dot);
            // in front
            if (dot > node.plane.D)
            {
                if (node.frontNode != null)
                    RecursiveTreeDraw(node.frontNode, pos);
                else if (node.frontLeaf != null)
                    DrawLeaf(node.frontLeaf);

                if (node.backNode != null)
                    RecursiveTreeDraw(node.backNode, pos);
                else if (node.backLeaf != null)
                    DrawLeaf(node.backLeaf);
            }
            else // behind
            {
                if (node.backNode != null)
                    RecursiveTreeDraw(node.backNode, pos);
                else if (node.backLeaf != null)
                    DrawLeaf(node.backLeaf);

                if (node.frontNode != null)
                    RecursiveTreeDraw(node.frontNode, pos);
                else if (node.frontLeaf != null)
                    DrawLeaf(node.frontLeaf);
            }
        }

        void DrawScene(GameTime gameTime, int addRotation, string viewName = null)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            if (wireframeOn)
                GraphicsDevice.RasterizerState = wireframeRS;

            // diffuse texture and detail texture
            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            GraphicsDevice.SamplerStates[2] = SamplerState.AnisotropicWrap;
            // lightmap texture
            //GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.
            // TODO: Add your drawing code here

            //GraphicsDevice.gr


            Matrix cameraRotation = Matrix.CreateRotationY(MathHelper.ToRadians(cameraPitchAngle)) * Matrix.CreateRotationZ(MathHelper.ToRadians(cameraYawAngle + addRotation));

            // camera offset is 64, player height is 72
            Vector3 cameraOffset = Vector3.Zero;//new Vector3(0, 0, 64);
            Vector3 normal = Vector3.TransformNormal(Vector3.UnitX, cameraRotation);
            view = Matrix.CreateLookAt(playerPosition + cameraOffset, playerPosition + cameraOffset + normal, Vector3.UnitZ);

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), GraphicsDevice.Viewport.AspectRatio, 1f, cameraClipDistance);

            viewFrustum = new BoundingFrustum(view * projection);

            worldEffect.View = view;
            worldEffect.Projection = projection;
            defaultPass = worldEffect.CurrentTechnique.Passes[0];

            lightmapWorldEffect.View = view;
            lightmapWorldEffect.Projection = projection;
            lightmapPass = lightmapWorldEffect.CurrentTechnique.Passes[0];

            if (!freezeVisibility)
            {
                visPosition = playerPosition;
                cameraLeaf = GetLeafFromPosition(visPosition);
                UpdateVisiblityFromLeaf(cameraLeaf);

                // do frustum check on visible leaves
                for (int i = 0; i < leaves.Length; i++)
                {
                    Leaf leaf = leaves[i];
                    if (leafVisList[i] && leaf != null)
                    {
                        ContainmentType contain = viewFrustum.Contains(leaf.bb);
                        leafVisList[i] = contain == ContainmentType.Contains || contain == ContainmentType.Intersects;
                    }
                    else
                    {
                        leafVisList[i] = false;
                    }
                }
            }

            GraphicsDevice.SetVertexBuffer(vb);
            GraphicsDevice.Indices = ib;

            RecursiveTreeDraw(rootNode, visPosition, leafVisList);
            //for (int i = 1; i < models.Length; i++)
            //{
            //    BspModel model = models[i];
            //    Leaf leaf = GetLeafFromPosition(model.center);
            //    bool isVisible = leafVisList[leaf.id];
            //    if (isVisible)
            //    {
            //        ContainmentType viewContain = viewFrustum.Contains(model.bb);
            //        if (viewContain == ContainmentType.Contains || viewContain == ContainmentType.Intersects)
            //        {
            //            RecursiveTreeDraw(models[i].rootNode, model.center);
            //        }
            //    }
            //}

            if (viewName != null)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font, viewName, new Vector2(0, GraphicsDevice.Viewport.Height - font.LineSpacing), Color.Red);
                spriteBatch.End();
            }
        }

        StringBuilder gcsb = new StringBuilder(64);

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(skyColor);

            //Viewport defaultViewport = GraphicsDevice.Viewport;
            //Viewport splitTL = new Viewport(defaultViewport.X, defaultViewport.Y, defaultViewport.Width / 2, defaultViewport.Height / 2);
            //Viewport splitBL = new Viewport(splitTL.X, splitTL.Bounds.Bottom, splitTL.Width, splitTL.Height);
            //Viewport splitTR = new Viewport(splitTL.Bounds.Right, defaultViewport.Y, splitTL.Width, splitTL.Height);
            //Viewport splitBR = new Viewport(splitTL.Bounds.Right, splitTL.Bounds.Bottom, splitTL.Width, splitTL.Height);

            lastDrawnTexID = -1;
            drawnFaces = 0;
            drawnLeaves = 0;

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

            spriteBatch.Begin(SpriteSortMode.Deferred, 
                BlendState.AlphaBlend, 
                SamplerState.PointWrap, 
                DepthStencilState.None, 
                RasterizerState.CullCounterClockwise);

            KConsole.SetResolution(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            KConsole.Draw(spriteBatch, font, (float)gameTime.ElapsedGameTime.TotalSeconds, (float)gameTime.TotalGameTime.TotalSeconds);

            spriteBatch.DrawString(font, "Camera position: " + playerPosition.ToString(), Vector2.Zero + Vector2.One, Color.Black);
            spriteBatch.DrawString(font, "Camera position: " + playerPosition.ToString(), Vector2.Zero, Color.White);

            string visString = "Leaf/PVS: " + cameraLeaf.id.ToString() + "/" + cameraLeaf.visCluster + " " + leafVisList[cameraLeaf.id].ToString();
            if (freezeVisibility)
                visString += " (VIS FROZEN)";
            spriteBatch.DrawString(font, visString, new Vector2(0, font.LineSpacing) + Vector2.One, Color.Black);
            spriteBatch.DrawString(font, visString, new Vector2(0, font.LineSpacing), Color.White);

            string drawCountString = "Drawn Leaves/Faces: " + drawnLeaves + "/" + drawnFaces;
            spriteBatch.DrawString(font, drawCountString, new Vector2(0, font.LineSpacing * 2) + Vector2.One, Color.Black);
            spriteBatch.DrawString(font, drawCountString, new Vector2(0, font.LineSpacing * 2), Color.White);

            frameCounter++;

            string fps = string.Format("fps: {0}", frameRate);

            spriteBatch.DrawString(font, fps, new Vector2(0, font.LineSpacing * 3) + Vector2.One, Color.Black);
            spriteBatch.DrawString(font, fps, new Vector2(0, font.LineSpacing * 3), Color.White);

            //string gc = "GC: " + GC.GetTotalMemory(false) + " bytes";
            gcsb.Clear();
            gcsb.Append("GC: ");
            gcsb.Append(GC.GetTotalMemory(false));
            gcsb.Append(" bytes");
            spriteBatch.DrawString(font, gcsb, new Vector2(0, font.LineSpacing * 4) + Vector2.One, Color.Black);
            spriteBatch.DrawString(font, gcsb, new Vector2(0, font.LineSpacing * 4), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
