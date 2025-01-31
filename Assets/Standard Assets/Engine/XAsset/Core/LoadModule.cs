﻿#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    Assets.cs
 Author:      Zeng Zhiwei
 Time:        2020/12/9 10:03:36
=====================================================
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;
using UnityEngine;
using Object = UnityEngine.Object;

public delegate void LoadedCallback(AssetRequest assetRequest);

public sealed class LoadModule : ModuleBase {
    #region Instance

    private static LoadModule instance;
    public static LoadModule Instance {
        get {
            if(instance == null) {
                instance = new LoadModule();
            }

            return instance;
        }
    }

    #endregion

    public static readonly string ManifestAsset = "Assets/Manifest.asset";
    public static readonly string Extension = ".unity3d";

    public static bool runtimeMode = false;
    public static Func<string, Type, Object> loadDelegate = null;
    private const string TAG = "[Assets]";

    #region API

    public override void Init()
    {
        base.Init();
        runtimeMode = Config.Instance.UseAssetBundle;
    }

    /// <summary>
    /// 读取所有资源路径
    /// </summary>
    /// <returns></returns>
    public static string[] GetAllAssetPaths()
    {
        var assets = new List<string>();
        assets.AddRange(_assetToBundles.Keys);
        return assets.ToArray();
    }
    public static string basePath { get; set; }

    public static string updatePath { get; set; }

    public static void AddSearchPath(string path)
    {
        searchPaths.Add(path);
    }

    public static ManifestRequest Initialize()
    {
        //改成不依赖MonoBehavior
        //var instance = FindObjectOfType<Assets>();
        //if(instance == null)
        //{
        //    instance = new GameObject("Assets").AddComponent<Assets>();
        //    DontDestroyOnLoad(instance.gameObject);
        //}

        if(string.IsNullOrEmpty(basePath))
            basePath = Application.streamingAssetsPath + Path.DirectorySeparatorChar;

        if(string.IsNullOrEmpty(updatePath))
            updatePath = Application.persistentDataPath + Path.DirectorySeparatorChar;

        Clear();

        GameLog.Log(string.Format(
         "Initialize with: runtimeMode={0}\nbasePath：{1}\nupdatePath={2}",
         runtimeMode, basePath, updatePath));

        var request = new ManifestRequest { name = ManifestAsset };
        AddAssetRequest(request);
        return request;
    }

    public static void Clear()
    {
        searchPaths.Clear();
        _activeVariants.Clear();
        _assetToBundles.Clear();
        _bundleToDependencies.Clear();
    }

    private static SceneAssetRequest _runningScene;
    public static SceneAssetRequest LoadSceneAsync(string path, bool additive, LoadedCallback loadedCallback = null)
    {
        if(string.IsNullOrEmpty(path))
        {
            GameLog.LogError("invalid path");
            return null;
        }

        path = GetExistPath(path);
        var asset = new SceneAssetRequestAsync(path, additive);
        if(!additive)
        {
            if(_runningScene != null)
            {
                _runningScene.Release(); ;
                _runningScene = null;
            }
            _runningScene = asset;
        }
        if(loadedCallback != null)
            asset.completed += loadedCallback;

        asset.Load();
        asset.AddReferance();
        _scenes.Add(asset);
        GameLog.Log(string.Format("LoadScene:{0}", path));
        return asset;
    }

    public static void UnloadScene(SceneAssetRequest scene) {
        scene.Release();
    }

    public static AssetRequest LoadAssetAsync(string path, Type type, LoadedCallback loadedCallback = null) {
        return LoadAsset(path, type, true, loadedCallback);
    }

    public static AssetRequest LoadAsset(string path, Type type) {
        return LoadAsset(path, type, false);
    }

    public static void UnloadAsset(AssetRequest asset) {
        asset.Release();
    }

    #region 业务

    public const string BULLET_PATH_PREFIX = "Assets/Data/bullet/";
    public const string MODEL_PATH_PREFIX = "Assets/Data/character/";
    private static string BundlePathPrefix = "Assets/Data/";
    private static string UIPathPrefix = "Assets/Data/ui/panel/";
    private static string JsonPathPrefix = "Assets/Scripts/DataTable/json/";
    private static StringBuilder stringBuilder = new StringBuilder();

    public static AssetRequest LoadModel(string assetName, LoadedCallback loadedCallback = null) {
        Type type = typeof(GameObject);
        stringBuilder.Clear();
        stringBuilder.Append(MODEL_PATH_PREFIX);
        stringBuilder.Append(assetName);
        stringBuilder.Append(GetAssetPostfix(type));
        return LoadAssetAsync(stringBuilder.ToString(), type, loadedCallback);
    }

    public static AssetRequest LoadUI(string path, LoadedCallback loadedCallback = null) {
        Type type = typeof(GameObject);
        stringBuilder.Clear();
        stringBuilder.Append(UIPathPrefix);
        stringBuilder.Append(path);
        stringBuilder.Append(GetAssetPostfix(type));
        return LoadAssetAsync(stringBuilder.ToString(), type, loadedCallback);
    }

    public static JsonData LoadJson(string path) {
        stringBuilder.Clear();
        stringBuilder.Append(JsonPathPrefix);
        stringBuilder.Append(path);

        path = stringBuilder.ToString();
        //Debug.Log("LoadJson: " + path);
        if(FileHelper.IsFileExist(path)) {
            using(StreamReader streamReader = new StreamReader(path)) {
                string jsonText = streamReader.ReadToEnd();
                JsonData jsonData = JsonMapper.ToObject(jsonText);
                return jsonData;
            }
        } else {
            GameLog.LogError(path + "不存在!");
            return null;
        }
    }
    
    #endregion

    #endregion

    #region Private

    /// <summary>
    /// Manifest加载结束回调
    /// </summary>
    /// <param name="manifest"></param>
    internal static void OnManifestLoaded(Manifest manifest)
    {
        _activeVariants.AddRange(manifest.activeVariants);

        AssetRef[] assets = manifest.assets;
        string[] dirs = manifest.dirs;
        BundleRef[] bundles = manifest.bundles;

        foreach(var item in bundles)
            _bundleToDependencies[item.name] = Array.ConvertAll(item.deps, id => bundles[id].name);

        foreach(AssetRef item in assets)
        {
            string path = string.Format("{0}/{1}", dirs[item.dir], item.name);
            if(item.bundle >= 0 && item.bundle < bundles.Length)
            {
                // 初始化路径映射表：路径==》bundle名字
                _assetToBundles[path] = bundles[item.bundle].name;
            }
            else
            {
                GameLog.LogError(string.Format("{0} bundle {1} not exist.", path, item.bundle));
            }
        }
    }

    /// <summary>
    /// 每帧最大加载bundle数
    /// </summary>
    private static List<AssetRequest> _unusedAssets = new List<AssetRequest>();
    private static List<AssetRequest> _loadingAssets = new List<AssetRequest>();
    private static List<SceneAssetRequest> _scenes = new List<SceneAssetRequest>();
    private static Dictionary<string, AssetRequest> _assets = new Dictionary<string, AssetRequest>();


    // update 驱动
    public override void Update(float dt)
    {
        UpdateAssets();
        UpdateBundles();
    }

    /// <summary>
    /// 更新加载请求
    /// </summary>
    private static void UpdateAssets()
    {
        for(int i = 0; i < _loadingAssets.Count; i++)
        {
            var request = _loadingAssets[i];
            if(request.Update())
                continue;
            _loadingAssets.RemoveAt(i);
            --i;
        }

        foreach(var item in _assets)
        {
            if(item.Value.isDone && item.Value.IsUnused())
            {
                _unusedAssets.Add(item.Value);
            }
        }

        //TODO 惰性GC
        //之所以叫惰性GC，是因为和上一个版本相比，上一个版本是每帧都会检查和清理未使用的资源，
        //这个版本底层只会在切换场景或者主动调用Assets.RemoveUnusedAssets(); 
        //的时候才会清理未使用的资源，这样用户可以按需调整资源回收的频率，在没有内存压力的时候，不回收可以获得更好的性能。
        if(_unusedAssets.Count > 0)
        {
            for(int i = 0; i < _unusedAssets.Count; ++i)
            {
                AssetRequest request = _unusedAssets[i];
                GameLog.Log(string.Format("UnloadAsset:{0}", request.name));
                _assets.Remove(request.name);
                request.Unload();
            }
            _unusedAssets.Clear();
        }

        for(var i = 0; i < _scenes.Count; ++i)
        {
            var request = _scenes[i];
            if(request.Update() || !request.IsUnused())
                continue;
            _scenes.RemoveAt(i);
            GameLog.Log(string.Format("UnloadScene:{0}", request.name));
            request.Unload();
            --i;
        }
    }

    private static void UpdateBundles()
    {
        int max = MAX_BUNDLES_PERFRAME;
        // 正在加载的bundle数量小于每帧可加载数量
        if(_toloadBundles.Count > 0 && max > 0 && _loadingBundles.Count < max)
            // 把能加载的bundle加进加载队列中
            for(int i = 0; i < Math.Min(max - _loadingBundles.Count, _toloadBundles.Count); ++i)
            {
                BundleRequest item = _toloadBundles[i];
                if(item.LoadState == AssetLoadState.Init)
                {
                    item.Load();
                    _loadingBundles.Add(item);
                    _toloadBundles.RemoveAt(i);
                    --i;
                }
            }
        // 加载中bundleUpdate，加载完就移除
        for(var i = 0; i < _loadingBundles.Count; i++)
        {
            var item = _loadingBundles[i];
            if(item.Update())
                continue;
            _loadingBundles.RemoveAt(i);
            --i;
        }

        // 加载完的bundle，并且没有引用，就加进无用bundle列表
        foreach(var item in _bundles)
        {
            if(item.Value.isDone && item.Value.IsUnused())
            {
                _unusedBundles.Add(item.Value);
            }
        }

        if(_unusedBundles.Count <= 0) 
            return;

        // 清理无用bundle列表
        for(int i = 0; i < _unusedBundles.Count; i++)
        {
            BundleRequest item = _unusedBundles[i];
            if(item.isDone)
            {
                item.Unload();
                _bundles.Remove(item.name);
                GameLog.Log("UnloadBundle: " + item.name);
            }
        }
        _unusedBundles.Clear();
    }

    private static void AddAssetRequest(AssetRequest request)
    {
        _assets.Add(request.name, request);
        _loadingAssets.Add(request);
    }

    private static AssetRequest LoadAsset(string path, Type type, bool async, LoadedCallback loadedCallback = null)
    {
        if(string.IsNullOrEmpty(path)) {
            GameLog.LogError("empty path!");
            return null;
        }

        path = GetExistPath(path);

        AssetRequest request;
        if(_assets.TryGetValue(path, out request)) {
            request.AddReferance();
            _loadingAssets.Add(request);
            if (loadedCallback != null) {
                loadedCallback(request);
            }
            
            return request;
        }

        string assetBundleName;
        if(GetAssetBundleName(path, out assetBundleName)) {
            request = async ? new BundleAssetRequestAsync(assetBundleName) : new BundleAssetRequest(assetBundleName);
        } else {
            if(path.StartsWith("http://", StringComparison.Ordinal) ||
                path.StartsWith("https://", StringComparison.Ordinal) ||
                path.StartsWith("file://", StringComparison.Ordinal) ||
                path.StartsWith("ftp://", StringComparison.Ordinal) ||
                path.StartsWith("jar:file://", StringComparison.Ordinal)) {
                request = new WebAssetRequest();
            } else
                request = new AssetRequest();
        }

        request.name = path;
        request.assetType = type;
        AddAssetRequest(request);
        if (loadedCallback != null) {
            request.completed += loadedCallback;
        }
        request.Load();
        request.AddReferance();
        GameLog.Log($"LoadAsset:{path}");
        return request;
    }

    #endregion

    #region Paths

    private static List<string> searchPaths = new List<string>();

    private static string GetExistPath(string path)
    {
#if UNITY_EDITOR
        if(runtimeMode == false) {
            // 编辑器模式
            if(File.Exists(path))
                return path;

            foreach(var item in searchPaths)
            {
                var existPath = string.Format("{0}/{1}", item, path);
                if(File.Exists(existPath))
                    return existPath;
            }

            GameLog.LogError("【找不到资源路径】" + path);
            return path;
        }
#endif
        if(_assetToBundles.ContainsKey(path))
            return path;

        foreach(var item in searchPaths)
        {
            var existPath = string.Format("{0}/{1}", item, path);
            if(_assetToBundles.ContainsKey(existPath))
                return existPath;
        }

        GameLog.LogError("资源没有收集打包" + path);
        return path;
    }

    // 获取资源在编辑器模式下的后缀
    public static string GetAssetPostfix(Type type) {
        if(type == typeof(Font))
            return BaseDef.FONT_SUFFIX;
        if(type == typeof(AudioClip))
            return BaseDef.MUSIC_SUFFIX;
        if(type == typeof(GameObject))
            return ".prefab";
        if(type == typeof(TextAsset))
            return ".bytes";
        if(type == typeof(Texture2D) || type == typeof(Sprite))
            return ".png";
        if(type == typeof(Material))
            return ".mat";
        if(type == typeof(ScriptableObject))
            return ".asset";

        return "";
    }
    
    #endregion

    #region Bundles
    private static readonly int MAX_BUNDLES_PERFRAME = 0;

    private static List<string> _activeVariants = new List<string>();
    private static List<BundleRequest> _loadingBundles = new List<BundleRequest>();
    private static List<BundleRequest> _toloadBundles = new List<BundleRequest>();
    private static List<BundleRequest> _unusedBundles = new List<BundleRequest>();
    private static Dictionary<string, BundleRequest> _bundles = new Dictionary<string, BundleRequest>();
    private static Dictionary<string, string> _assetToBundles = new Dictionary<string, string>();
    private static Dictionary<string, string[]> _bundleToDependencies = new Dictionary<string, string[]>();
    
    internal static bool GetAssetBundleName(string path, out string assetBundleName)
    {
        return _assetToBundles.TryGetValue(path, out assetBundleName);
    }

    internal static string[] GetAllDependencies(string bundle)
    {
        string[] deps;
        if(_bundleToDependencies.TryGetValue(bundle, out deps))
            return deps;

        return new string[0];
    }

    internal static BundleRequest LoadBundle(string assetBundleName)
    {
        return LoadBundle(assetBundleName, false);
    }

    internal static BundleRequest LoadBundleAsync(string assetBundleName)
    {
        return LoadBundle(assetBundleName, true);
    }

    internal static void UnloadBundle(BundleRequest bundle)
    {
        bundle.Release();
    }

    internal static BundleRequest LoadBundle(string assetBundleName, bool asyncMode)
    {
        if(string.IsNullOrEmpty(assetBundleName))
        {
            GameLog.LogError("assetBundleName == null");
            return null;
        }

        assetBundleName = RemapVariantName(assetBundleName);
        var url = GetDataPath(assetBundleName) + assetBundleName;

        BundleRequest bundle;

        if(_bundles.TryGetValue(url, out bundle))
        {
            bundle.AddReferance();
            _loadingBundles.Add(bundle);
            return bundle;
        }

        if(url.StartsWith("http://", StringComparison.Ordinal) ||
            url.StartsWith("https://", StringComparison.Ordinal) ||
            url.StartsWith("file://", StringComparison.Ordinal) ||
            url.StartsWith("ftp://", StringComparison.Ordinal))
        {
            bundle = new WebBundleRequest();
        }
        else
            bundle = asyncMode ? new BundleRequestAsync() : new BundleRequest();

        bundle.name = url;
        _bundles.Add(url, bundle);

        if(MAX_BUNDLES_PERFRAME > 0 && (bundle is BundleRequestAsync || bundle is WebBundleRequest))
        {
            _toloadBundles.Add(bundle);
        }
        else
        {
            bundle.Load();
            _loadingBundles.Add(bundle);
            GameLog.Log("LoadBundle: " + url);
        }

        bundle.AddReferance();
        return bundle;
    }

    // 这个函数没看懂
    private static string RemapVariantName(string assetBundleName)
    {
        var bundlesWithVariant = _activeVariants;
        // Get base bundle path
        var baseName = assetBundleName.Split('.')[0];

        var bestFit = int.MaxValue;
        var bestFitIndex = -1;
        // Loop all the assetBundles with variant to find the best fit variant assetBundle.
        for(var i = 0; i < bundlesWithVariant.Count; i++)
        {
            var curSplit = bundlesWithVariant[i].Split('.');
            var curBaseName = curSplit[0];
            var curVariant = curSplit[1];

            if(curBaseName != baseName)
                continue;

            var found = bundlesWithVariant.IndexOf(curVariant);

            // If there is no active variant found. We still want to use the first
            if(found == -1)
                found = int.MaxValue - 1;

            if(found >= bestFit)
                continue;
            bestFit = found;
            bestFitIndex = i;
        }

        if(bestFit == int.MaxValue - 1)
            GameLog.LogWarning(
                "Ambiguous asset bundle variant chosen because there was no matching active variant: " +
                bundlesWithVariant[bestFitIndex]);

        return bestFitIndex != -1 ? bundlesWithVariant[bestFitIndex] : assetBundleName;
    }

    private static string GetDataPath(string bundleName)
    {
        if(string.IsNullOrEmpty(updatePath))
            return basePath;

        if(File.Exists(updatePath + bundleName))
            return updatePath;

        return basePath;
    }

    #endregion
}