using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AssetManagerConfig", menuName = "AssetManager/AssetManagerConfig", order = 1)]
public class AssetManagerConfigScirptableObject : ScriptableObject
{
    public string ManagerTitle = "AssetManagerEditor";
    public int AssetBundleToolVersion = 100;
    public int CurrentBuildVersion = 101;
    //public AssetBundlePattern AssetBundleOutputPattern;
    //public List<string> CurrentAllAssets;
    //public bool[] CurrentSelectedAssets;
    public IncrementalBuildMode BuildMode;
    public List<PackageInfoEditor> EditorPackageInfos;
    public DefaultAsset _AssetBundleDirectory;
    public DefaultAsset AssetBundleDirectory
    {
        get => _AssetBundleDirectory;
        set
        {
            if (_AssetBundleDirectory != value)
            {
                _AssetBundleDirectory = value;
                if (_AssetBundleDirectory != null)
                {
                    GetFolderAllAssets();
                }
            }
        }
    }
    public string[] InvalidExtentionName = new string[] { ".meta", ".cs" };




    [MenuItem(nameof(ManagerTitle) + "/CreateConfigFile")]
    public static void CreateaNewConfigScripttableObject()
    {
        AssetManagerConfigScirptableObject assetManagerConfig = ScriptableObject.CreateInstance<AssetManagerConfigScirptableObject>();
        AssetDatabase.CreateAsset(assetManagerConfig, "Assets/Editor/AssetManagerConfig.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 获取非meta文件后为根据文件数量建立相应长度bool数组
    /// </summary>
    /// 
    public void GetFolderAllAssets()
    {
        if (AssetBundleDirectory == null)
        {
            return;
        }
        string directoryPath = AssetDatabase.GetAssetPath(AssetBundleDirectory);
        //CurrentAllAssets = FindAllAssetFromFolder(directoryPath);
        //CurrentSelectedAssets = new bool[CurrentAllAssets.Count];
    }
    public List<string> FindAllAssetFromFolder(string directoryPath)
    {
        List<string> objectPaths = new List<string>();
        //定位指定文件夹
        if (!Directory.Exists(directoryPath))
        {
            return null;
        }
        //获取文件夹下文件
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
        FileInfo[] fileInfos = directoryInfo.GetFiles();
        //遍历文件找到后缀无效文件
        for (int i = 0; i < fileInfos.Length; i++)
        {
            var file = fileInfos[i];
            if (!IsValidExtentionName(file.Extension))
            {
                continue;
            }
            //根据路径
            string path = $"{directoryPath}/{file.Name}";
            objectPaths.Add(path);
        }
        return objectPaths;
    }

    public bool IsValidExtentionName(string fileName)
    {
        bool isValid = true;
        foreach (var invalidName in InvalidExtentionName)
        {
            if (fileName.Contains(invalidName))
            {
                isValid = false;
            }
        }
        return isValid;
    }

}
public enum IncrementalBuildMode
{
    UselncrementalBuild,
    ForcusRebuild
}
public enum AssetBundlePattern
{
    Local,
    EditorSimulation,
    Remote,
}
[System.Serializable]
public class PackageInfoEditor
{
    //当前AssetBundle名称,可以用于代表主包名称
    public string PackageName = string.Empty;
    //当前这个包中所包含的所有的资源名称
    public List<UnityEngine.Object> Assets = new List<UnityEngine.Object>();
}
public class AssetInfo
{
    /// <summary>
    /// 资源名称
    /// </summary>
    public string AssetName;

    /// <summary>
    /// 资源所属AssetBundle名称
    /// </summary>
    public string AssetBundleName;
}

public class PackageInfo
{
    public string PackageName = string.Empty;
    public bool IsSourcePackage = false;
    public List<AssetInfo> assetInfos = new List<AssetInfo>();
    public List<string> BundleDependencies = new List<string>();
}
public class AssetBundleVersionDiffrence
{
    public List<string> AdditionAssetBundles;
    public List<string> ReducedAssetBundles;
}