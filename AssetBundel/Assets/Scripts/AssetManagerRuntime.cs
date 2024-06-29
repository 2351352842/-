using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
public class AssetManagerRuntime
{
    Dictionary<string, PackageInfo> PackageDic;
    Dictionary<string, AssetPackage> LoadedPackages = new Dictionary<string, AssetPackage>();
    List<string> LocalAllPackages;

    public AssetBundleManifest MainBundleManifest;
    public static AssetManagerRuntime instance;
    AssetBundlePattern CurrentPattern;

    public string AssetBundleLoadPath;
    string DownloadPath;
    string LocalAssetVersion;
    void CheckAssetBundleLoadPath()
    {
        switch (CurrentPattern)
        {
            case AssetBundlePattern.EditorSimulation:
                //AssetBundleLoadPath = Path.Combine(Application.persistentDataPath, AssetBundleName);
                break;
            case AssetBundlePattern.Local:
                AssetBundleLoadPath = Path.Combine(Application.streamingAssetsPath, LocalAssetVersion);
                break;
            case AssetBundlePattern.Remote:
                DownloadPath = Path.Combine(Application.persistentDataPath, "DownloadAssetBundle");
                AssetBundleLoadPath = Path.Combine(DownloadPath, LocalAssetVersion);
                break;
        }
    }
    void CheckAssetBundlePath()
    {
        switch (CurrentPattern)
        {
            case AssetBundlePattern.EditorSimulation:
                //AssetBundleLoadPath = Path.Combine(Application.persistentDataPath, AssetBundleName);
                break;
            case AssetBundlePattern.Local:
                AssetBundleLoadPath = Path.Combine(Application.streamingAssetsPath);
                break;
            case AssetBundlePattern.Remote:
                DownloadPath = Path.Combine(Application.persistentDataPath, "DownloadAssetBundle");
                AssetBundleLoadPath = Path.Combine(DownloadPath, LocalAssetVersion);
                break;
        }
    }
    void CheckLocalAssetVersion()
    {
        string versionFilePath = Path.Combine(AssetBundleLoadPath, "assetversion");
        if (!File.Exists(versionFilePath))
        {
            LocalAssetVersion = "1.0.0";
            File.WriteAllText(versionFilePath, LocalAssetVersion);
            return;
        }
        LocalAssetVersion = File.ReadAllText(versionFilePath);
    }
    public static void AssetManagerInit(AssetBundlePattern assetBundlePattern)
    {
        if (instance == null)
        {
            instance = new AssetManagerRuntime();
        }
        instance.CurrentPattern = assetBundlePattern;

        instance.CheckAssetBundlePath();
        instance.CheckLocalAssetVersion();
        instance.CheckAssetBundleLoadPath();
    }
    public AssetPackage LoadPackage(string PackageName)
    {
        if (LocalAllPackages == null)
        {
            string packageListPath = Path.Combine(AssetBundleLoadPath, "Packages");
            string packageListString = File.ReadAllText(packageListPath);

            LocalAllPackages = JsonConvert.DeserializeObject<List<string>>(packageListString);
        }

        if (!LocalAllPackages.Contains(PackageName))
        {
            Debug.LogError($"本地不存在{PackageName}包");
        }

        if (MainBundleManifest == null)
        {
            string mainBundlePath = Path.Combine(AssetBundleLoadPath, "BuildOutput");

            AssetBundle mainBundle = AssetBundle.LoadFromFile(mainBundlePath);
            MainBundleManifest = mainBundle.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
        }

        if (LoadedPackages.Keys.Contains(PackageName))
        {
            Debug.LogWarning($"{PackageName}包已加载");
            return LoadedPackages[PackageName];
        }

        AssetPackage package = new AssetPackage();
        string packagePath = Path.Combine(AssetBundleLoadPath, PackageName);
        string packageString = File.ReadAllText(packagePath);
        package.Package = JsonConvert.DeserializeObject<PackageInfo>(packageString);

        LoadedPackages.Add(PackageName, package);

        foreach (string dependPackageName in package.Package.BundleDependencies)
        {
            LoadPackage(dependPackageName);
        }
        return package;
    }
}
public class AssetPackage
{
    public PackageInfo Package;
    public string PackageName { get { return Package.PackageName; } }

    /// <summary>
    /// 当前包中已加载出的资源
    /// </summary>
    public Dictionary<string, Object> LoadedAssets = new Dictionary<string, Object>();

    public T LoadAsset<T>(string assetName) where T : Object
    {
        T assetObject = default;

        foreach (AssetInfo info in Package.assetInfos)
        {
            if (info.AssetName == assetName)
            {
                if (LoadedAssets.Keys.Contains(assetName))
                {
                    return LoadedAssets[assetName] as T;
                }

                foreach (string dependAssetBundle in AssetManagerRuntime.instance.MainBundleManifest.GetAllDependencies(info.AssetBundleName))
                {
                    string dependPath = Path.Combine(AssetManagerRuntime.instance.AssetBundleLoadPath, dependAssetBundle);

                    AssetBundle.LoadFromFile(dependPath);
                }

                string assetBundlePath = Path.Combine(AssetManagerRuntime.instance.AssetBundleLoadPath, info.AssetBundleName);

                AssetBundle bundle = AssetBundle.LoadFromFile(assetBundlePath);

                assetObject = bundle.LoadAsset<T>(assetName);

            }
        }
        if (assetObject == null)
        {
            Debug.LogError($"没有找到{assetName}");
        }
        return assetObject;
    }
}


