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
using KSoft.Client.GFX;
using KSoft.Client.Screens;
using KSoft.Client.Common;
using XboxTest.Sound;
using KSoft.Client.GameObjects;
using KSoft.Client.World;
using System.IO;
using System.Diagnostics;
//using KSoft.Client.Networking;
using System.Reflection;

namespace KSoft.Client
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Engine : Microsoft.Xna.Framework.Game
    {
        public static Engine I;

        public AssetManager assets;
        public InputManager input;
        public ScreenManager screens;

        public GraphicsMeshBuilder modelBuilder;
        //public ModelCache modelCache;

        // update/draw through screen
        public GameWorld world;
        //public Renderer renderer;

        //public NetworkManager network;

        public OggSong currentMusic;

        GraphicsDeviceManager graphics;
        // keep the spritebatch private so that rendering exceptions don't result in an already-begun spritebatch being used
        SpriteBatch spriteBatch;

        public GraphicsSettings gfxSettings;

        //float fov = 70;

        // 1280x720
        public int screenWidth = 1280;
        public int screenHeight = 720;
        public bool fullscreen = false;

        RasterizerState rsWireframe;

        RasterizerState rsNormal, rsShadow;
        DepthStencilState dssShadow;
        BlendState bsMultiply;

        #region Crash Handler Variables

        bool crashed = false;
        bool showCrashDebugger = true;
        bool crashDisableDraw = false;
        bool allowCrash = true;
        string crashMessage;
        int crashBannerSize;

        string[] crashButtonNames = new string[] { "F1-Exit", "F2-Continue", "F3-Disable crash handler", "F4-Copy to clipboard" };
        int crashButtonSpacing = 5;
        Rectangle[] crashButtonRects;

        #endregion Crash Handler Variables

        SpriteFont font;

        Texture2D square;
        Texture2D cursor;

        public Engine()
        {
            I = this;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //IsMouseVisible = true;
            //graphics.PreferredBackBufferWidth = screenWidth;
            //graphics.PreferredBackBufferHeight = screenHeight;

            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

            //graphics.PreferMultiSampling = true;
            //graphics.IsFullScreen = fullscreen;

            //IsFixedTimeStep = false;
            //Window.AllowUserResizing = true;

            //graphics.SynchronizeWithVerticalRetrace = false;

            
        }

        public void CreateRasterizerStates(bool wireframe)
        {
            rsShadow = new RasterizerState();
            rsShadow.CullMode = CullMode.None;
            rsShadow.DepthBias = -0.00001f;
            rsShadow.FillMode = wireframe ? FillMode.WireFrame : FillMode.Solid;

            //rsShadow = RasterizerState.CullCounterClockwise;

            rsNormal = new RasterizerState();
            rsNormal.CullMode = CullMode.CullClockwiseFace;
            rsNormal.FillMode = wireframe ? FillMode.WireFrame : FillMode.Solid;
            //
        }

        public void PlayMusic(string name, bool loop)
        {
            if (currentMusic != null)
                currentMusic.Dispose();
            currentMusic = new OggSong("Content/music/" + name + ".ogg");
            currentMusic.IsLooped = true;
            currentMusic.Play();

            //new SoundEffect(
        }

        public void StopMusic()
        {
            if (currentMusic != null)
                currentMusic.Stop();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // load graphics settings
            gfxSettings = new GraphicsSettings(this, GraphicsDevice, graphics);
            gfxSettings.SetDefaultModern();
            if (!gfxSettings.Load())
            {
                gfxSettings.SetDefaultModern();
                gfxSettings.Save();
            }
            gfxSettings.ApplyChanges();

            screenWidth = gfxSettings.screenWidth;
            screenHeight = gfxSettings.screenHeight;

            // TODO: Add your initialization logic here
            ExtendedContentManager ecm = new ExtendedContentManager(Content.ServiceProvider);
            ecm.RootDirectory = Content.RootDirectory;
            Content.Dispose();
            Content = ecm;

            assets = new AssetManager(GraphicsDevice, Content.RootDirectory);

            DebugDraw.Load(GraphicsDevice);

            //PersistantAssets.Load(Content);

            world = new GameWorld(this);

            //renderer = new Renderer();
            //renderer.camera.view = Matrix.CreateLookAt(new Vector3(4, 3, 4), Vector3.Zero, Vector3.UnitY);
            //renderer.camera.proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), 
            //    (float)graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight, 
            //    0.1f, 100f);

            screens = new ScreenManager();

            rsWireframe = new RasterizerState();
            rsWireframe.FillMode = FillMode.WireFrame;
            rsWireframe.CullMode = CullMode.None;

            CreateRasterizerStates(false);

            dssShadow = new DepthStencilState();
            dssShadow.StencilEnable = true;
            dssShadow.ReferenceStencil = 0;
            dssShadow.StencilFunction = CompareFunction.Equal;
            dssShadow.StencilPass = StencilOperation.Increment;

            //dssShadow.DepthBufferFunction = CompareFunction.LessEqual;

            bsMultiply = new BlendState();
            bsMultiply.ColorBlendFunction = BlendFunction.Add;
            bsMultiply.ColorSourceBlend = Blend.DestinationColor;
            bsMultiply.ColorDestinationBlend = Blend.Zero;
            bsMultiply.AlphaSourceBlend = Blend.DestinationAlpha;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            modelBuilder = new GraphicsMeshBuilder(GraphicsDevice, Content);
            //modelCache = new ModelCache();

            #region setting default models
            modelBuilder.SetAddTransform(Matrix.Identity);

            modelBuilder.SetDataSource(assets.Load<ModelData>("models/primitives/cube.obj"));
            modelBuilder.AddAll();
            ModelCache.Put(ModelNames.Cube, modelBuilder.Build());

            modelBuilder.SetDataSource(assets.Load<ModelData>("models/primitives/ball.obj"));
            modelBuilder.AddAll();
            ModelCache.Put(ModelNames.Sphere, modelBuilder.Build());

            modelBuilder.SetDataSource(assets.Load<ModelData>("models/primitives/cylinder.obj"));
            modelBuilder.AddAll();
            ModelCache.Put(ModelNames.Cylinder, modelBuilder.Build());

            modelBuilder.SetDataSource(null);
            #endregion

            //Assets.Load(Content);

            //world.LoadAssets(assets);

            font = Content.Load<SpriteFont>("Fonts/debug");
            cursor = assets.Load<Texture2D>("images/cursor.png");

            square = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            square.SetData(new Color[] { new Color(1f, 1f, 1f, 1f) });

            //screens.Add(new StartupScreen());
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            screens.Reset();
            ModelCache.Clear();

            // TODO: Unload any non ContentManager content here
            if (currentMusic != null)
            {
                currentMusic.Stop();
                currentMusic.Dispose();
            }

            assets.UnloadAll();
        }

        public string CreateErrorReport(Exception e)
        {
            string pathRemove = @"C:\GitHub\KSoftTanks\TanksEXP\";
            string exceptionString = e.ToString().Replace(pathRemove, "");

            AssemblyName aName = Assembly.GetAssembly(typeof(Engine)).GetName();

            exceptionString = DateTime.Now.ToString() + "\r\n" 
                + aName.ToString() + "\r\n"
                + exceptionString;

            return exceptionString;
        }

        public void Crash(Exception e, bool disableDraw, string where)
        {
            crashed = true;
            crashDisableDraw = disableDraw;
            string exceptionString = CreateErrorReport(e);
            bool couldSave = false;
            try
            {
                File.WriteAllText("clientcrash.txt", exceptionString);
                couldSave = true;
            }
            catch
            {
                couldSave = false;
            }
            crashMessage = "KSoft Tanks has encountered a serious error and has stopped running.\n";
            crashMessage += "=== Guru Meditation - " + where + " ===\n" + exceptionString;
            if (couldSave)
                crashMessage += "\n=== Saved as clientcrash.txt ===";
            else
                crashMessage += "\n=== Could NOT save clientcrash.txt ===";
            crashMessage += "\nPlease send the crash report (and preferably a screenshot) to Kleadron!";

            crashBannerSize = (int)font.MeasureString(crashMessage).Y + 1;

            crashButtonRects = new Rectangle[crashButtonNames.Length];

            for (int i = 0; i < crashButtonRects.Length; i++)
            {
                Vector2 buttonStringSize = font.MeasureString(crashButtonNames[i]) + new Vector2(1, 1);

                crashButtonRects[i] = new Rectangle(
                    crashButtonSpacing,
                    crashButtonSpacing + crashBannerSize,
                    (int)buttonStringSize.X,
                    (int)buttonStringSize.Y);

                if (i > 0)
                    crashButtonRects[i].X = crashButtonRects[i - 1].Right + crashButtonSpacing;
            }
        }

        KeyboardState kbs;
        KeyboardState oldkbs;

        MouseState oldms;
        MouseState ms;

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

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float total = (float)gameTime.TotalGameTime.TotalSeconds;

            ms = Mouse.GetState();
            kbs = Keyboard.GetState();

            if (crashed)
            {
                if (kbs.IsKeyDown(Keys.Tab) && oldkbs.IsKeyUp(Keys.Tab))
                    showCrashDebugger = !showCrashDebugger;
            }

            if (crashed)
            {
                if (showCrashDebugger && IsActive)
                {
                    for (int i = 0; i < crashButtonRects.Length; i++)
                    {
                        if (ms.LeftButton == ButtonState.Pressed && oldms.LeftButton == ButtonState.Released)
                        {
                            if (crashButtonRects[i].Contains(ms.X, ms.Y))
                            {
                                // quit
                                if (i == 0)
                                {
                                    Exit();
                                }

                                // continue
                                if (i == 1)
                                {
                                    crashed = false;
                                }

                                // ignore all
                                if (i == 2)
                                {
                                    crashed = false;
                                    allowCrash = false;
                                }

                                // copy to clipboard
                                if (i == 3)
                                {
                                    System.Windows.Forms.Clipboard.SetText(crashMessage);
                                }
                            }
                        }
                    }
                }

                if (crashed)
                {
                    if (kbs.IsKeyDown(Keys.F1) && oldkbs.IsKeyUp(Keys.F1))
                    {
                        Exit();
                    }
                    if (kbs.IsKeyDown(Keys.F2) && oldkbs.IsKeyUp(Keys.F2))
                    {
                        crashed = false;
                    }
                    if (kbs.IsKeyDown(Keys.F3) && oldkbs.IsKeyUp(Keys.F3))
                    {
                        crashed = false;
                        allowCrash = false;
                    }
                    if (kbs.IsKeyDown(Keys.F4) && oldkbs.IsKeyUp(Keys.F4))
                    {
                        System.Windows.Forms.Clipboard.SetText(crashMessage);
                    }
                }
            }

            // TODO: Add your update logic here
            if (!Debugger.IsAttached && !crashed)
            {
                try
                {
                    
                    UpdateFunction(delta, total);
                    //throw new Exception("Lol!");
                }
                catch (Exception e)
                {
                    if (allowCrash)
                    {
                        Crash(e, false, "Exception occured in Update");
                    }
                }
            }

            if (Debugger.IsAttached && !crashed)
                UpdateFunction(delta, total);

            oldms = ms;
            oldkbs = kbs;

            base.Update(gameTime);
        }

        void UpdateFunction(float delta, float total)
        {
            screens.Update(delta, total);
        }

        public void Set3DState()
        {
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
        }

        public void Set3DShadowState()
        {
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.RasterizerState = rsShadow;

            GraphicsDevice.DepthStencilState = dssShadow;
            GraphicsDevice.BlendState = bsMultiply;
        }

        void DrawFunction(float delta, float total)
        {
            screens.Draw(delta, total);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            //GraphicsDevice.Clear(new Color(0.5f, 0.5f, 0.5f));

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float total = (float)gameTime.TotalGameTime.TotalSeconds;

            //Set3DState();

            if (!Debugger.IsAttached)
            {
                if (!crashed)
                {
                    try
                    {

                        DrawFunction(delta, total);
                        //throw new Exception("Lol!");
                    }
                    catch (Exception e)
                    {
                        if (allowCrash)
                        {
                            Crash(e, true, "Exception occured in Draw");
                        }
                    }
                }
                else
                {
                    if (!crashDisableDraw)
                    {
                        try
                        {

                            DrawFunction(delta, total);
                            //throw new Exception("Lol!");
                        }
                        catch (Exception e)
                        {
                            // it's probably caused by the update exception
                            crashDisableDraw = true;
                        }
                    }
                }
            }
            else
            {
                DrawFunction(delta, total);
            }

            Color cursorColor = Color.White;

            if (crashed)
            {
                Color colorBG = Color.DarkBlue;
                Color colorHighlight = Color.LightBlue;
                //cursorColor = Color.DarkOrange;

                if (crashDisableDraw)
                {
                    colorBG = Color.DarkRed;
                    colorHighlight = Color.Pink;
                    //cursorColor = Color.Red;
                } 

                if (showCrashDebugger)
                {
                    spriteBatch.Begin();
                    spriteBatch.Draw(square, new Rectangle(0, 0, screenWidth, crashBannerSize), colorBG * 0.75f);
                    spriteBatch.DrawString(font, crashMessage, new Vector2(1, 1), Color.Black);
                    spriteBatch.DrawString(font, crashMessage, new Vector2(0, 0), Color.White);

                    for (int i = 0; i < crashButtonRects.Length; i++)
                    {
                        Rectangle buttonRect = crashButtonRects[i];

                        if (buttonRect.Contains(ms.X, ms.Y))
                        {
                            spriteBatch.Draw(square, buttonRect, colorHighlight * 0.75f);
                            spriteBatch.DrawString(font, crashButtonNames[i], new Vector2(buttonRect.X + 1, buttonRect.Y + 1), Color.Black);
                            spriteBatch.DrawString(font, crashButtonNames[i], new Vector2(buttonRect.X, buttonRect.Y), Color.White);
                        }
                        else
                        {
                            spriteBatch.Draw(square, buttonRect, colorBG * 0.75f);
                            spriteBatch.DrawString(font, crashButtonNames[i], new Vector2(buttonRect.X + 1, buttonRect.Y + 1), Color.Black);
                            spriteBatch.DrawString(font, crashButtonNames[i], new Vector2(buttonRect.X, buttonRect.Y), Color.White);
                        }
                    }

                    spriteBatch.End();
                }
                else
                {
                    spriteBatch.Begin();

                    if (crashDisableDraw)
                    {
                        spriteBatch.Draw(square, new Rectangle(33, 33, 100, 2), Color.Black);
                        spriteBatch.Draw(square, new Rectangle(32, 32, 100, 2), Color.Red);
                    }
                    else
                    {
                        spriteBatch.Draw(square, new Rectangle(33, 33, 100, 2), Color.Black);
                        spriteBatch.Draw(square, new Rectangle(32, 32, 100, 2), Color.DarkOrange);
                    }

                    spriteBatch.End();
                }
            }

            if (!gfxSettings.hardwareCursor)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(cursor, new Rectangle(ms.X, ms.Y, 16, 16), cursorColor);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
