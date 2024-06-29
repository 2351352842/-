using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

public class AssetManagerEditor
{
    #region ��������
    public static string OutputPath = Path.Combine(Application.streamingAssetsPath, "AssetBundles");
    public static string AssetBundleName = "SampleAssetBundle.ab";
    public static AssetBundlePattern LoadPattern;
    public static AssetManagerConfigScirptableObject AssetManagerConfig;

    #endregion

    [MenuItem("AssetManager/BuildAssetBundle")]
    public static void BuildAssetBundle()
    {
        CheckOutputPath();
        string assetBundelDirectory = "Assets/AssetBundles";
        BuildPipeline.BuildAssetBundles(assetBundelDirectory, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);
    }

    [MenuItem("AssetManager/AssetManagerWindow")]
    static void OpenAsstMangerWindow()
    {
        Rect wr = new Rect(0, 0, 500, 1000);
        AssetManagerEditorWindow window = (AssetManagerEditorWindow)EditorWindow.GetWindowWithRect(typeof(AssetManagerEditorWindow), wr, true, "AssetManger");
        window.Show();
    }

    /// <summary>
    /// ��Ŀ���ļ��л�ȡ��Ч�ļ�
    /// </summary>
    /// <param name="directoryPath">Ŀ���ļ���</param>
    /// <returns>Ŀ���ļ�����Ч�ļ�����</returns>
    static List<string> FindAllAssetFromFolder(string directoryPath)
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
    /// <summary>
    /// �ж��ļ���չ���Ƿ���Ч
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    static bool IsValidExtentionName(string fileName)
    {
        bool isValid = true;
        foreach (var invalidName in AssetManagerConfig.InvalidExtentionName)
        {
            if (fileName.Contains(invalidName))
            {
                isValid = false;
            }
        }
        return isValid;
    }

    public static void BuildAssetBundleFromFolder()
    {
        CheckOutputPath();
        AssetManagerConfig.GetFolderAllAssets();
        AssetBundleBuild[] builds = new AssetBundleBuild[1];
        //�����Asset�����һ��AssetBundle
        //assetBundleNameΪ�������
        builds[0].assetBundleName = AssetBundleName;
        //assetNameʵ��Ϊ���·��

        //builds[0].assetNames = AssetManagerConfig.CurrentAllAssets.ToArray();
        BuildPipeline.BuildAssetBundles(OutputPath, builds, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);
        AssetDatabase.Refresh();
    }

    public static void BuildAssetBundleFromEditorWindow()
    {
        CheckOutputPath();
        List<string> buildAssets = new List<string>();
        //for (int i = 0; i < AssetManagerConfig.CurrentSelectedAssets.Length; i++)
        //{
        //    if (AssetManagerConfig.CurrentSelectedAssets[i])
        //    {
        //        buildAssets.Add(AssetManagerConfig.CurrentAllAssets[i]);
        //    }
        //}
        /*�����Asset�����һ��AssetBundle
        //assetBundleNameΪ�������
        builds[0].assetBundleName = AssetBundleLoad.ObjectBundleName;
        //assetNameʵ��Ϊ���·��
        builds[0].assetNames = bulidAssets.ToArray();
        BuildPipeline.BuildAssetBundles(OutPutPath, builds, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        AssetDatabase.Refresh();*/
        AssetBundleBuild[] builds = new AssetBundleBuild[buildAssets.Count];
        for (int i = 0; i < builds.Length; i++)
        {
            //��·��ɾȥֻ�����ļ�����Ϊ����
            string directoryPath = AssetDatabase.GetAssetPath(AssetManagerConfig.AssetBundleDirectory);
            string assetName = buildAssets[i].Replace($"{directoryPath}/", string.Empty);
            assetName = assetName.Replace(".prefab", string.Empty);
            builds[i].assetBundleName = assetName;
            string[] assetNames = new string[] { buildAssets[i] };
            builds[i].assetNames = assetNames;
        }
        BuildPipeline.BuildAssetBundles(OutputPath, builds, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        AssetDatabase.Refresh();

    }

    public static void CheckOutputPath()
    {
        string versionString = AssetManagerConfig.CurrentBuildVersion.ToString();
        for (int i = versionString.Length - 1; i >= 1; i--)
        {
            versionString = versionString.Insert(i, ".");
        }
        switch (LoadPattern)
        {
            case AssetBundlePattern.EditorSimulation:
                //OutputPath = Path.Combine(Application.persistentDataPath, AssetBundleLoad.AssetBundleName);
                break;
            case AssetBundlePattern.Local:
                OutputPath = Path.Combine(Application.streamingAssetsPath, "Local");
                break;
            case AssetBundlePattern.Remote:
                OutputPath = Path.Combine(Application.persistentDataPath, "Remote");
                break;
        }
        if (!Directory.Exists(OutputPath))
        {
            Directory.CreateDirectory(OutputPath);
        }
    }


    /// <summary>
    /// ��ȡѡ�е���Դ·��
    /// </summary>
    public static List<string> GetSelectAssetNames()
    {
        List<string> selectedAssetNames = new List<string>();
        //for (int i = 0; i < AssetManagerConfig.CurrentSelectedAssets.Length; i++)
        //{
        //    if (AssetManagerConfig.CurrentSelectedAssets[i])
        //    {
        //        selectedAssetNames.Add(AssetManagerConfig.CurrentAllAssets[i]);
        //    }
        //}
        return selectedAssetNames;
    }

    /// <summary>
    /// ��ȡѡ����Դ����·��
    /// </summary>
    public static void GetAssetsFromPath()
    {
        List<string> selectedAssetNames = GetSelectAssetNames();
        string[] assetsDeps = AssetDatabase.GetDependencies(selectedAssetNames.ToArray(), true);
        foreach (var assetName in assetsDeps)
        {
            Debug.Log(assetName);
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    public static void BuildAssetBundleSets()
    {
        CheckOutputPath();
        //����Editorѡ��AssetΪSourceAsset
        List<string> selectedAssetNames = GetSelectAssetNames();
        //����SourceAsset����ΪDerivedAsset
        List<List<GUID>> selectedAssetDependencies = new List<List<GUID>>();
        //����SourceAsset������
        foreach (var selectedAssetName in selectedAssetNames)
        {
            string[] assetDeps = AssetDatabase.GetDependencies(selectedAssetName, true);
            List<GUID> assetGUIDs = new List<GUID>();
            //GUID assetGUID=AssetDatabase.GUIDFromAssetPath(selectedAssetName);
            //assetGUIDs.Add(assetGUID);
            foreach (var assetPath in assetDeps)
            {
                assetGUIDs.Add(AssetDatabase.GUIDFromAssetPath(assetPath));
            }
            selectedAssetDependencies.Add(assetGUIDs);
        }
        for (int i = 0; i < selectedAssetDependencies.Count; i++)
        {
            int nextCount = i + 1;
            if (nextCount >= selectedAssetDependencies.Count)
            {
                break;
            }
            for (int j = 0; j <= i; j++)
            {
                List<GUID> newDerivedAssets = ContrastDependenceFromGUID(selectedAssetDependencies[i], selectedAssetDependencies[nextCount]);
                if (newDerivedAssets.Count > 0)
                {
                    selectedAssetDependencies.Add(newDerivedAssets);
                }
            }
        }
        AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[selectedAssetDependencies.Count];
        for (int i = 0; i < assetBundleBuilds.Length; i++)
        {
            assetBundleBuilds[i].assetBundleName = i.ToString();
            string[] assetNames = new string[selectedAssetDependencies[i].Count];
            for (int j = 0; j < assetNames.Length; j++)
            {
                string assetName = AssetDatabase.GUIDToAssetPath(selectedAssetDependencies[i][j].ToString());
                assetNames[j] = assetName;
            }
            assetBundleBuilds[i].assetNames = assetNames;
        }
        BuildPipeline.BuildAssetBundles(OutputPath, assetBundleBuilds, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        AssetDatabase.Refresh();
    }

    /// <summary>
    ///��ȡdepsA��depB��ͬ��������ȥ������
    /// </summary>
    /// <param name="depsA"></param>
    /// <param name="depsB"></param>
    /// <returns>����ֵΪ��ͬ����GUID����</returns>
    public static List<GUID> ContrastDependenceFromGUID(List<GUID> depsA, List<GUID> depsB)
    {
        List<GUID> newDerivedAssets = new List<GUID>();
        foreach (GUID assetGUID in depsA)
        {
            if (depsB.Contains(assetGUID))
            {
                newDerivedAssets.Add(assetGUID);
            }
        }
        foreach (GUID item in newDerivedAssets)
        {
            if (depsA.Contains(item))
            {
                depsA.Remove(item);
            }
            if (depsB.Contains(item))
            {
                depsB.Remove(item);
            }
        }
        Debug.Log(newDerivedAssets.Count);
        return newDerivedAssets;
    }

    /// <summary>
    ///  Ƕ�׺���,����ʹ����������Node
    /// </summary>
    /// <param name="lastNode"></param>�ϲ�Node|���ӦΪSourceAsset
    /// <param name="currentAlINodes"></param>��ǰ���е�Node
    public static void GetNodesFromDependencies(AssetBundleNode lastNode, List<AssetBundleNode> currentAllNodes)
    {
        string[] assetNames = AssetDatabase.GetDependencies(lastNode.assetName, false);
        if (assetNames.Length > 0)
        {
            lastNode.OutEdge = new AssetBundleEdge();
        }
        foreach (string assetName in assetNames)
        {
            if (!IsValidExtentionName(assetName))
            {
                continue;
            }
            AssetBundleNode currentNode = null;
            foreach (AssetBundleNode existingNode in currentAllNodes)
            {
                if (existingNode.assetName == assetName)
                {
                    currentNode = existingNode;
                    break;
                }
            }
            if (currentNode == null)
            {
                currentNode = new AssetBundleNode();
                currentNode.assetName = assetName;
                currentNode.InEdge = new AssetBundleEdge();
                currentNode.SourceIndices = new List<int>();
                currentAllNodes.Add(currentNode);
            }
            currentNode.InEdge.Nodes.Add(lastNode);
            lastNode.OutEdge.Nodes.Add(currentNode);
            currentNode.PackageName = lastNode.PackageName;
            if (lastNode.SourceIndex >= 0)
            {
                currentNode.SourceIndices.Add(lastNode.SourceIndex);
            }
            else
            {
                foreach (int index in lastNode.SourceIndices)
                {
                    if (!currentNode.SourceIndices.Contains(index))
                    {
                        currentNode.SourceIndices.Add(index);
                    }
                }
            }
            lastNode.OutEdge.Nodes.Add(currentNode);
            GetNodesFromDependencies(currentNode, currentAllNodes);
        }
    }

    /// <summary>
    /// ����ͼ���
    /// </summary>
    public static void BuildAssetBundleFromDirectedGraph()
    {
        CheckOutputPath();

        List<AssetBundleNode> allNodes = new List<AssetBundleNode>();

        int sourceIndex = 0;

        Dictionary<string, PackageInfo> packageInfoDic = new Dictionary<string, PackageInfo>();

        for (int pakcageInfoIndex = 0; pakcageInfoIndex < AssetManagerConfig.EditorPackageInfos.Count; pakcageInfoIndex++)
        {
            PackageInfo info = new PackageInfo();

            PackageInfoEditor editorInfo = AssetManagerConfig.EditorPackageInfos[pakcageInfoIndex];

            info.IsSourcePackage = true;
            info.PackageName = editorInfo.PackageName;

            packageInfoDic.Add(editorInfo.PackageName, info);

            foreach (var asset in AssetManagerConfig.EditorPackageInfos[pakcageInfoIndex].Assets)
            {
                AssetBundleNode currentNode = null;
                string assetName = AssetDatabase.GetAssetPath(asset);
                //���һ��SourceAsset��֮ǰ�Ѿ�������DerivedAsset��ӹ�
                //��ôֱ��ʹ����ͬ��Node
                foreach (AssetBundleNode node in allNodes)
                {
                    if (node.assetName == assetName)
                    {
                        currentNode = node;
                        currentNode.PackageName = info.PackageName;
                        break;
                    }
                }
                //���򴴽��µ�Node
                if (currentNode == null)
                {
                    currentNode = new AssetBundleNode();

                    currentNode.PackageName = info.PackageName;
                    currentNode.PackageNames.Add(currentNode.PackageName);

                    currentNode.SourceIndex = sourceIndex;
                    currentNode.SourceIndices = new List<int>() { currentNode.SourceIndex };
                    currentNode.InEdge = new AssetBundleEdge();
                    currentNode.assetName = assetName;

                    allNodes.Add(currentNode);
                }

                GetNodesFromDependencies(currentNode, allNodes);
                sourceIndex++;
            }
        }



        Dictionary<List<int>, List<AssetBundleNode>> assetbundleNodeDic = new Dictionary<List<int>, List<AssetBundleNode>>();
        foreach (var node in allNodes)
        {
            StringBuilder packageNameString = new StringBuilder();

            //�������Ϊ�գ�������һ��DerivedAsset
            //��ô�ͱ��������˸�DerivedAsset��Package�������䵽һ���µİ���
            if (string.IsNullOrEmpty(node.PackageName))
            {
                //������������õ���Դ�����䵽һ���µİ���
                for (int i = 0; i < node.PackageNames.Count; i++)
                {
                    packageNameString.Append(node.PackageNames[i]);
                    if (i < node.PackageNames.Count - 1)
                    {
                        packageNameString.Append("_");
                    }
                }
                string packageName = packageNameString.ToString();
                node.PackageName = packageName;

                if (!packageInfoDic.Keys.Contains(packageName))
                {
                    PackageInfo dependInfo = new PackageInfo();
                    dependInfo.PackageName = packageName;
                    dependInfo.IsSourcePackage = false;
                    packageInfoDic.Add(dependInfo.PackageName, dependInfo);
                }
            }

            bool isSourceIndexEquals = false;
            List<int> keyList = new List<int>();
            foreach (List<int> key in assetbundleNodeDic.Keys)
            {
                isSourceIndexEquals = node.SourceIndices.Count == key.Count && node.SourceIndices.All(p => key.Any(k => k.Equals(p)));

                if (isSourceIndexEquals)
                {
                    keyList = key;
                }
            }
            if (!isSourceIndexEquals)
            {
                keyList = node.SourceIndices;
                assetbundleNodeDic.Add(keyList, new List<AssetBundleNode>());
            }
            assetbundleNodeDic[keyList].Add(node);
        }


        AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[assetbundleNodeDic.Count];

        int buildIndex = 0;
        foreach (var key in assetbundleNodeDic.Keys)
        {
            List<string> assetNames = new List<string>();

            foreach (AssetBundleNode node in assetbundleNodeDic[key])
            {
                assetNames.Add(node.assetName);

                //��Ϊ��ѭ����������е�Node
                //�������һ��node��PackageNames��Ψһ�����Ҳ���һ��SourcePackage����ΪPackageNames�еĶ�ӦPackage��ӶԸ�Package������
                foreach (string dependPackageName in node.PackageNames)
                {
                    if (packageInfoDic.Keys.Contains(dependPackageName))
                    {
                        if (!packageInfoDic[dependPackageName].BundleDependencies.Contains(node.PackageName) && !string.Equals(node.PackageName, packageInfoDic[dependPackageName].PackageName))
                        {
                            packageInfoDic[dependPackageName].BundleDependencies.Add(node.PackageName);
                        }
                    }
                }
            }

            assetBundleBuilds[buildIndex].assetBundleName = ComputeAssetSetSignature(assetNames);


            assetBundleBuilds[buildIndex].assetNames = assetNames.ToArray();

            foreach (AssetBundleNode node in assetbundleNodeDic[key])
            {
                AssetInfo info = new AssetInfo();
                info.AssetName = node.assetName;
                info.AssetBundleName = assetBundleBuilds[buildIndex].assetBundleName;
                packageInfoDic[node.PackageName].assetInfos.Add(info);
            }

            buildIndex++;
        }

        BuildPipeline.BuildAssetBundles(OutputPath, assetBundleBuilds, CheckIncrementalMode(),
                    BuildTarget.StandaloneWindows);

        string versionFilePath = Path.Combine(OutputPath, "BuildVersion.version");
        File.WriteAllText(versionFilePath, AssetManagerConfig.CurrentBuildVersion.ToString());

        BuildAssetBundleHashTable(assetBundleBuilds);

        BuildPackageTable(packageInfoDic);

        CopyAssetBundleToVersionFolder();

        //CreateBuildInfo();
        AssetManagerConfig.CurrentBuildVersion++;


        AssetDatabase.Refresh();
    }
    //public static void BuildAssetBundleFromDirectedGraph()
    //{
    //    AssetManagerConfig.CurrentBuildVersion++;
    //    CheckOutputPath();
    //    List<AssetBundleNode> allNodes = new List<AssetBundleNode>();
    //    int sourceIndex = 0;
    //    Dictionary<string, PackageInfo> packageInfos = new Dictionary<string, PackageInfo>();
    //    for (int pakcageInfoIndex = 0; pakcageInfoIndex < AssetManagerConfig.EditorPackageInfos.Count; pakcageInfoIndex++)
    //    {
    //        PackageInfo info = new PackageInfo();
    //        PackageInfoEditor editorInfo = AssetManagerConfig.EditorPackageInfos[pakcageInfoIndex];
    //        info.PackageName = editorInfo.PackageName;
    //        packageInfos.Add(editorInfo.PackageName, info);
    //        foreach (var asset in AssetManagerConfig.EditorPackageInfos[pakcageInfoIndex].Assets)
    //        {
    //            AssetBundleNode currentNode = new AssetBundleNode();
    //            currentNode.assetName = AssetDatabase.GetAssetPath(asset);
    //            currentNode.PackageName = info.PackageName;
    //            currentNode.SourceIndex = sourceIndex;
    //            currentNode.SourceIndices = new List<int>() { currentNode.SourceIndex };
    //            currentNode.InEdge = new AssetBundleEdge();
    //            allNodes.Add(currentNode);
    //            GetNodesFromDependencies(currentNode, allNodes);
    //            sourceIndex++;
    //        }
    //    }
    //    Dictionary<List<int>, List<AssetBundleNode>> assetbundleNodeDic = new Dictionary<List<int>, List<AssetBundleNode>>();
    //    foreach (var node in allNodes)
    //    {
    //        if (!assetbundleNodeDic.Keys.Contains(node.SourceIndices))
    //        {
    //            assetbundleNodeDic.Add(node.SourceIndices, new List<AssetBundleNode>());
    //        }
    //        if (assetbundleNodeDic.Keys.Contains(node.SourceIndices))
    //        {
    //            assetbundleNodeDic[node.SourceIndices].Add(node);
    //        }
    //    }
    //    AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[assetbundleNodeDic.Count];
    //    int buildIndex = 0;
    //    foreach (var key in assetbundleNodeDic.Keys)
    //    {
    //        List<string> assetNames = new List<string>();
    //        foreach (AssetBundleNode node in assetbundleNodeDic[key])
    //        {
    //            assetNames.Add(node.assetName);
    //        }
    //        assetBundleBuilds[buildIndex].assetBundleName = ComputeAssetSetSignature(assetNames);
    //        assetBundleBuilds[buildIndex].assetNames = assetNames.ToArray();
    //        foreach (AssetBundleNode node in assetbundleNodeDic[key])
    //        {
    //            if (assetNames.Contains(node.assetName))
    //            {
    //                AssetInfo info = new AssetInfo();
    //                info.AssetName = node.assetName;
    //                info.AssetBundleName = assetBundleBuilds[buildIndex].assetBundleName;
    //                packageInfos[node.PackageName].assetInfos.Add(info);
    //            }
    //        }
    //        buildIndex++;
    //    }




    //    BuildPipeline.BuildAssetBundles(OutputPath, assetBundleBuilds, CheckIncrementalMode(),
    //        BuildTarget.StandaloneWindows);
    //    BuildAssetBundleHashTable(assetBundleBuilds);
    //    BuildPackageTable(packageInfos);
    //    CopyAssetBundleToVersionFolder();
    //    AssetDatabase.Refresh();
    //}

    /// <summary>
    /// 
    /// </summary>
    //public static void BuildAssetBundleFromDirectedGraph()
    //{
    //    AssetManagerConfig.CurrentBuildVersion++;
    //    CheckOutputPath();
    //    List<string> selectedAssetNames = GetSelectAssetNames();
    //    List<AssetBundleNode> allNodes = new List<AssetBundleNode>();
    //    for (int i = 0; i < selectedAssetNames.Count; i++)
    //    {
    //        AssetBundleNode currentNode = new AssetBundleNode();
    //        currentNode.assetName = selectedAssetNames[i];
    //        currentNode.SourceIndex = i;
    //        currentNode.SourceIndices = new List<int>() { currentNode.SourceIndex };
    //        currentNode.InEdge = new AssetBundleEdge();
    //        allNodes.Add(currentNode);
    //        GetNodesFromDependencies(currentNode, allNodes);
    //    }
    //    Dictionary<List<int>, List<AssetBundleNode>> assetbundleNodeDic = new Dictionary<List<int>, List<AssetBundleNode>>();
    //    foreach (var node in allNodes)
    //    {
    //        if (!assetbundleNodeDic.Keys.Contains(node.SourceIndices))
    //        {
    //            assetbundleNodeDic.Add(node.SourceIndices, new List<AssetBundleNode>());
    //        }
    //        if (assetbundleNodeDic.Keys.Contains(node.SourceIndices))
    //        {
    //            assetbundleNodeDic[node.SourceIndices].Add(node);
    //        }
    //    }
    //    AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[assetbundleNodeDic.Count];
    //    int buildIndex = 0;
    //    foreach (var key in assetbundleNodeDic.Keys)
    //    {
    //        List<string> assetNames = new List<string>();
    //        foreach (AssetBundleNode node in assetbundleNodeDic[key])
    //        {
    //            assetNames.Add(node.assetName);
    //        }
    //        assetBundleBuilds[buildIndex].assetBundleName = ComputeAssetSetSignature(assetNames);
    //        assetBundleBuilds[buildIndex].assetNames = assetNames.ToArray();
    //        buildIndex++;
    //    }
    //    BuildPipeline.BuildAssetBundles(OutputPath, assetBundleBuilds, CheckIntIncrementalMode(), BuildTarget.StandaloneWindows);
    //    BuildAssetBundleHashTable(assetBundleBuilds);
    //    CopyAssetBundleToVersionFolder();
    //    AssetDatabase.Refresh();

    //}

    /// <summary>
    /// ����AB����ϣ��
    /// </summary>
    /// <param name="assetBundleBuilds"></param>
    /// <returns></returns>
    public static string[] BuildAssetBundleHashTable(AssetBundleBuild[] assetBundleBuilds)
    {
        CheckOutputPath();
        string[] assetBundleHashs = new string[assetBundleBuilds.Length];
        for (int i = 0; i < assetBundleBuilds.Length; i++)
        {
            string assetBundlePath = Path.Combine(OutputPath, assetBundleBuilds[i].assetBundleName);
            FileInfo fileInfo = new FileInfo(assetBundlePath);
            assetBundleHashs[i] = $"{fileInfo.Length}_{assetBundleBuilds[i].assetBundleName}";
        }
        string hashString = JsonConvert.SerializeObject(assetBundleHashs);
        string hashFilePath = Path.Combine(OutputPath, "AssetBundleHashs");
        File.WriteAllText(hashFilePath, hashString);
        return assetBundleHashs;
    }

    /// <summary>
    /// ������ʹ��
    /// </summary>
    /// <param name="assetBundlePath"></param>
    /// <returns></returns>
    static string ComputerAssetBundleSizeToMD5(string assetBundlePath)
    {
        MD5 md5 = MD5.Create();
        FileInfo filelnfo = new FileInfo(assetBundlePath);
        byte[] buffer = Encoding.ASCII.GetBytes(filelnfo.Length.ToString());
        md5.TransformFinalBlock(new byte[0], 0, 0);
        return BytesToHexString(md5.Hash);
    }

    /// <summary>
    /// ��ȡ�����asset��GUID,������ͨ��MD5�㷨������ЩGUID��hashֵ,��MD5 Hash ֵת��Ϊʮ�������ַ���������Ϊ����
    /// </summary>
    /// <param name="assetNames">asset</param>
    /// <returns>ʮ������MD5 Hashֵ(����)</returns>
    static string ComputeAssetSetSignature(IEnumerable<string> assetNames)
    {
        var assetGuids = assetNames.Select(AssetDatabase.AssetPathToGUID);
        MD5 md5 = MD5.Create();
        foreach (string assetGuid in assetGuids.OrderBy(x => x))
        {
            byte[] buffer = Encoding.ASCII.GetBytes(assetGuid);
            md5.TransformBlock(buffer, 0, buffer.Length, null, 0);
        }
        md5.TransformFinalBlock(new byte[0], 0, 0);
        return BytesToHexString(md5.Hash);
    }
    static string BytesToHexString(byte[] bytes)
    {
        StringBuilder byteString = new StringBuilder();
        foreach (byte aByte in bytes)
        {
            byteString.Append(aByte.ToString("x2"));
        }
        return byteString.ToString();
    }

    /// <summary>
    /// �Ƚ�AB����С��GUID
    /// </summary>
    /// <param name="oldHashTable"></param>
    /// <param name="newHashTable"></param>
    /// <returns>AssetBundleVersionDiffrence��,��¼������ɾ����AB��</returns>
    public static AssetBundleVersionDiffrence ContrastAssetBundleHashTable(string[] oldHashTable, string[] newHashTable)
    {
        AssetBundleVersionDiffrence diffrence = new AssetBundleVersionDiffrence();
        diffrence.AdditionAssetBundles = new List<string>();
        diffrence.ReducedAssetBundles = new List<string>();
        foreach (string assetHash in oldHashTable)
        {
            if (!newHashTable.Contains(assetHash))
            {
                diffrence.ReducedAssetBundles.Add(assetHash);
            }
        }
        foreach (string assetHash in newHashTable)
        {
            if (!oldHashTable.Contains(assetHash))
            {
                diffrence.AdditionAssetBundles.Add(assetHash);
            }
        }
        return diffrence;
    }

    /// <summary>
    /// �Ƚϵ�ǰ�汾����һ�汾������ɾ����AB��
    /// </summary>
    /// <param name="currentBuilds"></param>
    /// <returns></returns>
    public AssetBundleBuild[] GetVersionDiffrence(AssetBundleBuild[] currentBuilds)
    {
        string[] currentHashTable = BuildAssetBundleHashTable(currentBuilds);
        AssetManagerConfig.CurrentBuildVersion--;
        CheckOutputPath();
        string lastVersionHashTablePath = Path.Combine(OutputPath, "AssetBundleHashs");
        string lastVersionHashTablePathString = File.ReadAllText(lastVersionHashTablePath);
        string[] lastVersionHashTable = JsonConvert.DeserializeObject<string[]>(lastVersionHashTablePathString);
        AssetBundleVersionDiffrence diffrence = ContrastAssetBundleHashTable(lastVersionHashTable, currentHashTable);
        if (diffrence.AdditionAssetBundles.Count > 0)
        {
            foreach (var item in diffrence.AdditionAssetBundles)
            {
                Debug.Log(item);
            }
        }
        if (diffrence.ReducedAssetBundles.Count > 0)
        {
            foreach (var item in diffrence.ReducedAssetBundles)
            {
                Debug.Log(item);
            }
        }
        return currentBuilds;
    }

    /// <summary>
    /// ѡ���Ƿ��������
    /// </summary>
    /// <returns></returns>
    static BuildAssetBundleOptions CheckIncrementalMode()
    {
        BuildAssetBundleOptions option = BuildAssetBundleOptions.None;
        switch (AssetManagerConfig.BuildMode)
        {
            case IncrementalBuildMode.UselncrementalBuild:
                option = BuildAssetBundleOptions.DeterministicAssetBundle;
                break;
            case IncrementalBuildMode.ForcusRebuild:
                option = BuildAssetBundleOptions.ForceRebuildAssetBundle;
                break;
        }
        return option;
    }

    /// <summary>
    /// ���������ļ���OutputPath���Ƶ��汾�ļ���
    /// </summary>
    static void CopyAssetBundleToVersionFolder()
    {
        string versionString = AssetManagerConfig.CurrentBuildVersion.ToString();
        for (int i = versionString.Length - 1; i >= 1; i--)
        {
            versionString = versionString.Insert(i, ".");
        }
        string assetBundleVersionOutputPath = Path.Combine(Application.streamingAssetsPath, versionString);
        if (!Directory.Exists(assetBundleVersionOutputPath))
        {
            Directory.CreateDirectory(assetBundleVersionOutputPath);
        }
        string[] assetNames = ReadAssetBundleHashTable(OutputPath);
        //���ƹ�ϣ��
        string hashTableOriginPath = Path.Combine(OutputPath, "AssetBundleHashs");
        string hashTableVersionPath = Path.Combine(assetBundleVersionOutputPath, "AssetBundleHashs");
        File.Copy(hashTableOriginPath, hashTableVersionPath);
        //��������
        string mainBundleOriginPath = Path.Combine(OutputPath, "Local");
        string mainBundleVersionPath = Path.Combine(assetBundleVersionOutputPath, "Local");
        File.Copy(mainBundleOriginPath, mainBundleVersionPath);
        //����package
        string packageListOrginPath = Path.Combine(OutputPath, "Packages");
        string packageListVersionPath = Path.Combine(assetBundleVersionOutputPath, "Packages");
        File.Copy(packageListOrginPath, packageListVersionPath);
        foreach (var assetName in assetNames)
        {
            string assetHashName = assetName.Substring(assetName.IndexOf("_") + 1);
            string assetBundleOriginPath = Path.Combine(OutputPath, assetHashName);
            string assetBundleVersionPath = Path.Combine(assetBundleVersionOutputPath, assetHashName);
            File.Copy(assetBundleOriginPath, assetBundleVersionPath);
        }
    }
    /// <summary>
    /// ��ȡ��ϣ��
    /// </summary>
    /// <param name="OutputPath"></param>
    /// <returns></returns>
    static string[] ReadAssetBundleHashTable(string OutputPath)
    {
        string HashTablePath = Path.Combine(OutputPath, "AssetBundleHashs");
        string HashString = File.ReadAllText(HashTablePath);
        string[] HashTables = JsonConvert.DeserializeObject<string[]>(HashString);
        return HashTables;
    }
    static void BuildPackageTable(Dictionary<string, PackageInfo> packages)
    {
        string packagesJSON = JsonConvert.SerializeObject(packages);
        string packagesPath = Path.Combine(OutputPath, "Packages");
        File.WriteAllText(packagesPath, packagesJSON);
    }

    public static void LoadAssetManagerConfig(AssetManagerEditorWindow window)
    {
        if (AssetManagerConfig == null)
        {
            AssetManagerConfig = AssetDatabase.LoadAssetAtPath<AssetManagerConfigScirptableObject>("Assets/Editor/AssetManagerConfig.asset");
            window.VersionString = AssetManagerConfig.AssetBundleToolVersion.ToString();
            for (int i = window.VersionString.Length - 1; i >= 1; i--)
            {
                window.VersionString = window.VersionString.Insert(i, ".");
            }
            window.editorDirectory = AssetManagerConfig.AssetBundleDirectory;
        }

    }
    public static void LoadAssetManagerWindowConfig(AssetManagerEditorWindow window)
    {
        if (window.AssetManagerWindowConfig == null)
        {
            window.AssetManagerWindowConfig = AssetDatabase.LoadAssetAtPath<AssetManagerEditorWindowConfig>("Assets/Editor/AssetManagerEditorWindowConfig.asset");
        }
        window.AssetManagerWindowConfig.TitleLabelStyle.alignment = TextAnchor.MiddleCenter;
        window.AssetManagerWindowConfig.TitleLabelStyle.fontSize = 24;
        window.AssetManagerWindowConfig.TitleLabelStyle.normal.textColor = Color.red;

        window.AssetManagerWindowConfig.VersionLabelStyle.alignment = TextAnchor.LowerRight;
        window.AssetManagerWindowConfig.VersionLabelStyle.fontSize = 14;
        window.AssetManagerWindowConfig.VersionLabelStyle.normal.textColor = Color.green;

        window.AssetManagerWindowConfig.LogoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pictures/redflagpic.png");
        window.AssetManagerWindowConfig.LogoLabelStyle.alignment = TextAnchor.MiddleCenter;
        window.AssetManagerWindowConfig.LogoLabelStyle.fixedWidth = window.AssetManagerWindowConfig.LogoTexture.width / 8;
        window.AssetManagerWindowConfig.LogoLabelStyle.fixedHeight = window.AssetManagerWindowConfig.LogoTexture.height / 8;

    }

    /// <summary>
    /// �����ص�config����ΪJson��ʽ
    /// </summary>
    public static void SaveConfigToJSON()
    {
        if (AssetManagerConfig != null)
        {
            //string configString=JsonConvert.SerializeObject(AssetManagerConfig,Formatting.Indented,new JsonSerializerSettings {ReferenceLoopHandling=ReferenceLoopHandling.Ignore});
            string configString = JsonUtility.ToJson(AssetManagerConfig);
            string outputPath = Path.Combine(Application.dataPath, "Editor/AssetManagerConfig.json");
            File.WriteAllText(outputPath, configString);
            AssetDatabase.Refresh();
        }
    }
    /// <summary>
    /// ��json�ļ���ȡ������config
    /// </summary>
    public static void ReadConfigFromJSON()
    {
        string configPath = Path.Combine(Application.dataPath, "Editor/AssetManagerConfig.json");
        string configString = File.ReadAllText(configPath);
        JsonUtility.FromJsonOverwrite(configString, AssetManagerConfig);
    }
    public static void AddAssetBundleInfo()
    {
        AssetManagerConfig.EditorPackageInfos.Add(new PackageInfoEditor());
    }
    public static void RemoveAssetBundleInfo(PackageInfoEditor info)
    {
        if (AssetManagerConfig.EditorPackageInfos.Contains(info))
        {
            AssetManagerConfig.EditorPackageInfos.Remove(info);
        }
    }
    public static void AddAsset(PackageInfoEditor info)
    {
        info.Assets.Add(null);
    }
    public static void RemoveAsset(PackageInfoEditor info, UnityEngine.Object assetObject)
    {
        if (info.Assets.Contains(assetObject))
        {
            info.Assets.Remove(assetObject);
        }
    }

}




public class AssetBundleEdge
{
    //һ��Node�������ö��Node,���NodeҲ��������һ��Node
    public List<AssetBundleNode> Nodes = new List<AssetBundleNode>();

}
public class AssetBundleNode
{
    //ÿ��Node��Ӧһ��Asset
    public string assetName;
    ///����SourceAsset�����,�������SourceAsset��ΪĬ��ֵ
    public int SourceIndex = -1;
    /// ���������ø���Դ��SourceAsset���������
    public List<int> SourceIndices;
    //��ǰNode���õ�Nodes
    public AssetBundleEdge OutEdge;
    //���õ�ǰNode��Nodes
    public AssetBundleEdge InEdge;
    public string PackageName;
    public List<string> PackageNames = new List<string>();
}

