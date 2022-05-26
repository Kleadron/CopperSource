using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using QoiSharp;
using KSoft.Client.GFX;

// ksoft content manager :D
// a version of content manager that allows you to use normal files with extensions instead of needing to be processed

namespace KSoft.Client.Common
{
    public class AssetManager
    {
        Dictionary<string, IDisposable> disposableAssets = new Dictionary<string, IDisposable>();
        Dictionary<string, object> loadedAssets = new Dictionary<string, object>();

        Dictionary<Type, AssetLoader> loaders = new Dictionary<Type,AssetLoader>();

        // caching
        Dictionary<string, AssetLoader> loaderForFileType = new Dictionary<string, AssetLoader>();
        Dictionary<string, Type> typeForFileType = new Dictionary<string, Type>();

        public GraphicsDevice device;
        public string rootDirectory;

        public AssetManager(GraphicsDevice device, string rootDirectory)
        {
            this.device = device;
            this.rootDirectory = rootDirectory;

            RegisterLoader<Texture2D, Texture2DLoader>();
            RegisterLoader<SoundEffect, SoundEffectLoader>();
            RegisterLoader<ModelData, ModelDataLoader>();
        }

        string GetCorrectPath(string assetName)
        {
            return Path.Combine(rootDirectory, assetName);
        }

        public void RegisterLoader<AssetType, LoaderType>() where LoaderType : AssetLoader
        {
            Type loaderType = typeof(LoaderType);
            Type assetType = typeof(AssetType);

            AssetLoader loader = (AssetLoader)Activator.CreateInstance(loaderType, this);

            loaders.Add(assetType, loader);
            string[] fileTypes = loader.GetFileTypes();
            foreach (string fileType in fileTypes)
            {
                loaderForFileType.Add(fileType, loader);
                typeForFileType.Add(fileType, assetType);
            }
        }

        /// <summary>
        /// Returns an already loaded asset or loads the asset from the path specified.
        /// </summary>
        /// <typeparam name="T">The asset type. A loader must be registered for it.</typeparam>
        /// <param name="assetName">The path to the asset file.</param>
        /// <returns>Requested asset.</returns>
        public T Load<T>(string assetName) 
        {
            string realPath = GetCorrectPath(assetName);

            // check if it is loaded already
            if (loadedAssets.ContainsKey(assetName))
            {
                object asset = loadedAssets[assetName];
                if (asset is T)
                {
                    return (T)asset;
                }
                else
                {
                    throw new Exception("ASSET TYPE COLLISION: Tried getting asset of type " + typeof(T).FullName
                        + " but the loaded asset was type " + asset.GetType().FullName + ", asset " + assetName);
                }
            }

            // second step, check if the file exists
            if (!File.Exists(realPath))
            {
                throw new Exception("Asset \"" + assetName + "\" does not exist");
            }

            // third step, can it even be loaded
            Type t = typeof(T);
            if (loaders.ContainsKey(t))
            {
                //Console.WriteLine("LOAD: " + realPath);

                object asset = loaders[t].LoadAsset(realPath);
                loadedAssets.Add(assetName, asset);
                if (asset is IDisposable)
                {
                    disposableAssets.Add(assetName, (IDisposable)asset);
                }
                return (T)asset;
            }

            throw new Exception("No loaders for asset \"" + assetName + "\" found");
        }

        public string[] GetLoadedAssetList()
        {
            return loadedAssets.Keys.ToArray();
        }

        public void Preload(string assetName)
        {
            string realPath = GetCorrectPath(assetName);

            // check if it is loaded already
            if (loadedAssets.ContainsKey(assetName))
            {
                return;
            }

            // second step, check if the file exists
            if (!File.Exists(realPath))
            {
                throw new Exception("Asset \"" + assetName + "\" does not exist");
            }

            // third step, can it even be loaded
            string fileType = Path.GetExtension(assetName);
            Type t = typeForFileType[fileType];

            if (loaders.ContainsKey(t))
            {
                object asset = loaders[t].LoadAsset(realPath);
                loadedAssets.Add(assetName, asset);
                if (asset is IDisposable)
                {
                    disposableAssets.Add(assetName, (IDisposable)asset);
                }
                return;
            }

            throw new Exception("No loaders for asset \"" + assetName + "\" found");
        }

        public void Preload(string[] assetNames)
        {
            foreach (string assetName in assetNames)
            {
                Preload(assetName);
            }
        }

        public void Unload(string assetName)
        {
            loadedAssets.Remove(assetName);
            if (disposableAssets.ContainsKey(assetName))
            {
                IDisposable asset = disposableAssets[assetName];
                asset.Dispose();
                disposableAssets.Remove(assetName);
            }
        }

        public void UnloadAll()
        {
            string[] unloadList = loadedAssets.Keys.ToArray();
            foreach (string assetName in unloadList)
            {
                Unload(assetName);
            }
        }
    }

    public abstract class AssetLoader
    {
        protected AssetManager manager;

        public abstract string[] GetFileTypes();
        //public abstract Type GetAssetType();

        public AssetLoader(AssetManager manager)
        {
            this.manager = manager;
        }

        public abstract object LoadAsset(string path);
    }

    public sealed class Texture2DLoader : AssetLoader
    {
        public override string[] GetFileTypes()
        {
            return new string[] { ".gif", ".png", ".jpg", ".jpeg", ".qoi" };
        }

        public Texture2DLoader(AssetManager manager) : base(manager) { }

        public override object LoadAsset(string path)
        {
            Texture2D texture;
            string fileType = Path.GetExtension(path).ToLower();

            switch (fileType)
            {
                case ".qoi":
                    {
                        byte[] fileData = File.ReadAllBytes(path);
                        QoiImage image = QoiDecoder.Decode(fileData);
                        // 4 channel
                        byte[] srcData = image.Data;
                        byte[] convertData;
                        int srcI = 0;
                        int dstI = 0;

                        if (image.Channels == QoiSharp.Codec.Channels.Rgb)
                        {
                            // convert RGB image to RGBA
                            convertData = new byte[image.Width * image.Height * 4];
                            while (dstI < convertData.Length)
                            {
                                convertData[dstI++] = srcData[srcI++]; // R
                                convertData[dstI++] = srcData[srcI++]; // G
                                convertData[dstI++] = srcData[srcI++]; // B
                                convertData[dstI++] = 255; // A
                            }
                        }
                        else
                        {
                            // use it directly
                            convertData = srcData;
                        }


                        texture = new Texture2D(manager.device, image.Width, image.Height, false, SurfaceFormat.Color);
                        texture.SetData<byte>(convertData);
                        return texture;
                    }
                case ".gif":
                case ".png":
                case ".jpg":
                case ".jpeg":
                    {
                        FileStream s = File.OpenRead(path);
                        texture = Texture2D.FromStream(manager.device, s);
                        s.Close();
                        return texture;
                    }

                default:
                    throw new Exception("Texture2DLoader: Unknown file type \"" + fileType + "\"");
            }
            
            //return texture;
        }
    }

    public sealed class SoundEffectLoader : AssetLoader
    {
        public override string[] GetFileTypes()
        {
            return new string[] { ".wav" };
        }

        public SoundEffectLoader(AssetManager manager) : base(manager) { }

        public override object LoadAsset(string path)
        {
            FileStream s = File.OpenRead(path);
            SoundEffect sfx = SoundEffect.FromStream(s);
            s.Close();
            return sfx;
        }
    }

    public sealed class ModelDataLoader : AssetLoader
    {
        public override string[] GetFileTypes()
        {
            return new string[] { ".obj" };
        }

        public ModelDataLoader(AssetManager manager) : base(manager) { }

        public override object LoadAsset(string path)
        {
            ModelData data = new ModelData(path);
            return data;
        }
    }
}
