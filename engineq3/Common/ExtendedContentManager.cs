using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace KSoft.Client.Common
{
    public class ExtendedContentManager : ContentManager
    {
        // http://web.archive.org/web/20101225164440/http://blogs.msdn.com/b/shawnhar/archive/2007/03/09/contentmanager-readasset.aspx
        Dictionary<string, object> loadedAssets = new Dictionary<string, object>();
        Dictionary<string, List<IDisposable>> disposableAssets = new Dictionary<string, List<IDisposable>>();
        string recordedAsset;

        public ExtendedContentManager(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public void Preload(string assetName)
        {

        }

        public void Preload(string[] assetNames)
        {
            foreach (string assetName in assetNames)
            {
                //Load<
            }
        }

        public string[] GetLoadedAssetNames()
        {
            return loadedAssets.Keys.ToArray();
        }

        public override T Load<T>(string assetName)
        {
            // this did nothing the whole time :|
            //string fn = this.RootDirectory + "\\" + assetName;
            //return ReadAsset<T>(assetName, null);

            if (loadedAssets.ContainsKey(assetName))
                return (T)loadedAssets[assetName];

            recordedAsset = assetName;

            T asset = ReadAsset<T>(assetName, RecordDisposableAsset);

            loadedAssets.Add(assetName, asset);

            return asset;
        }

        public void SelectiveUnload(object asset)
        {
            string resolvedAssetName = null;
            foreach (KeyValuePair<string, object> loadedAsset in loadedAssets)
            {
                if (loadedAsset.Value == asset)
                {
                    resolvedAssetName = loadedAsset.Key;
                }
            }
            if (resolvedAssetName != null)
                SelectiveUnload(resolvedAssetName);
        }

        public void SelectiveUnload(string assetName)
        {
            if (disposableAssets.ContainsKey(assetName))
            {
                foreach (IDisposable disposable in disposableAssets[assetName])
                {
                    disposable.Dispose();
                }

                disposableAssets.Remove(assetName);
                loadedAssets.Remove(assetName);
            }
        }

        public override void Unload()
        {
            foreach (KeyValuePair<string, List<IDisposable>> disposableAssetCollection in disposableAssets)
            {
                foreach (IDisposable disposable in disposableAssetCollection.Value)
                {
                    disposable.Dispose();
                }
            }


            loadedAssets.Clear();
            disposableAssets.Clear();
        }


        void RecordDisposableAsset(IDisposable disposable)
        {
            if (!disposableAssets.ContainsKey(recordedAsset))
            {
                disposableAssets.Add(recordedAsset, new List<IDisposable>());
            }
            disposableAssets[recordedAsset].Add(disposable);
        }
    }
}
