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
    /// ��ȡ��meta�ļ���Ϊ�����ļ�����������Ӧ����bool����
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
        //��λָ���ļ���
        if (!Directory.Exists(directoryPath))
        {
            return null;
        }
        //��ȡ�ļ������ļ�
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
        FileInfo[] fileInfos = directoryInfo.GetFiles();
        //�����ļ��ҵ���׺��Ч�ļ�
        for (int i = 0; i < fileInfos.Length; i++)
        {
            var file = fileInfos[i];
            if (!IsValidExtentionName(file.Extension))
            {
                continue;
            }
            //����·��
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
    //��ǰAssetBundle����,�������ڴ�����������
    public string PackageName = string.Empty;
    //��ǰ������������������е���Դ����
    public List<UnityEngine.Object> Assets = new List<UnityEngine.Object>();
}
public class AssetInfo
{
    /// <summary>
    /// ��Դ����
    /// </summary>
    public string AssetName;

    /// <summary>
    /// ��Դ����AssetBundle����
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