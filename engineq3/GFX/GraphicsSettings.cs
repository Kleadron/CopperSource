using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using KSoft.Client.Data;

namespace KSoft.Client.GFX
{
    public class GraphicsSettings
    {
        Game game;
        GraphicsDevice device;
        GraphicsDeviceManager graphics;

        public GraphicsSettings(Game game, GraphicsDevice device, GraphicsDeviceManager manager)
        {
            this.game = game;
            this.device = device;
            this.graphics = manager;
        }

        public int screenWidth;         // Screen width
        public int screenHeight;        // Screen height
        public bool vsync;

        public int multiSampleCount;    // Anti-Aliasing Quality

        public bool enableAA;           // Anti-Aliasing
        public bool enableBlur;         // Menu blur
        public bool fullscreen;         // Fullscreen mode
        public bool perPixelLighting;   // Per-pixel BasicEffect lighting
        public bool simplifiedLighting; // Static level lighting
        public bool enableShadows;      // Flat stencil shadows

        public bool hardwareCursor;

        public float GetAspectRatio()
        {
            return (float)screenWidth / (float)screenHeight;
        }

        public bool Load()
        {
            ConfigFile config = new ConfigFile();
            bool fileLoaded = config.Load("gfx");

            if (fileLoaded)
            {
                try
                {
                    string value = null;

                    value = config["screen_width"];
                    if (value != null)
                        screenWidth = int.Parse(value);

                    value = config["screen_height"];
                    if (value != null)
                        screenHeight = int.Parse(value);

                    value = config["screen_vsync"];
                    if (value != null)
                        vsync = int.Parse(value) == 1 ? true : false;

                    value = config["screen_fullscreen"];
                    if (value != null)
                        fullscreen = int.Parse(value) == 1 ? true : false;

                    value = config["multi_sample_count"];
                    if (value != null)
                        multiSampleCount = int.Parse(value);

                    value = config["multi_sample_enable"];
                    if (value != null)
                        enableAA = int.Parse(value) == 1 ? true : false;

                    value = config["menu_blur_enable"];
                    if (value != null)
                        enableBlur = int.Parse(value) == 1 ? true : false;

                    value = config["hardware_cursor"];
                    if (value != null)
                        hardwareCursor = int.Parse(value) == 1 ? true : false;
                }
                catch (Exception e)
                {
                    // an error occured parsing data
                    return false;
                }

                return true;
            }

            return false;
        }

        public bool Save()
        {
            ConfigFile config = new ConfigFile();

            config["screen_width"] = screenWidth.ToString();
            config["screen_height"] = screenHeight.ToString();
            config["screen_vsync"] = vsync ? "1" : "0";
            config["screen_fullscreen"] = fullscreen ? "1" : "0";

            config["multi_sample_count"] = multiSampleCount.ToString();
            config["multi_sample_enable"] = enableAA ? "1" : "0";

            config["menu_blur_enable"] = enableBlur ? "1" : "0";
            config["hardware_cursor"] = hardwareCursor ? "1" : "0";

            config.Save("gfx");

            return true;
        }

        public void SetDefaultBasic()
        {
            screenWidth = 640;
            screenHeight = 480;
            vsync = true;

            multiSampleCount = 0;

            enableAA = false;
            enableBlur = false;
            fullscreen = false;
            perPixelLighting = false;
            simplifiedLighting = false;
            enableShadows = true;

            hardwareCursor = false;
        }

        public void SetDefaultModern()
        {
            screenWidth = 1280;
            screenHeight = 720;
            vsync = true;

            multiSampleCount = 8;

            enableAA = true;
            enableBlur = true;
            fullscreen = false;
            perPixelLighting = true;
            simplifiedLighting = false;
            enableShadows = true;

            hardwareCursor = false;
        }

        public void ApplyChanges()
        {
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.SynchronizeWithVerticalRetrace = vsync;
            graphics.IsFullScreen = fullscreen;

            graphics.PreferMultiSampling = enableAA;
            device.PresentationParameters.MultiSampleCount = multiSampleCount;

            graphics.ApplyChanges();

            game.IsMouseVisible = hardwareCursor;
        }
    }
}
