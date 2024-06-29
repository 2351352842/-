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
    #region 变量定义
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
    /// 从目标文件夹获取有效文件
    /// </summary>
    /// <param name="directoryPath">目标文件夹</param>
    /// <returns>目标文件夹有效文件链表</returns>
    static List<string> FindAllAssetFromFolder(string directoryPath)
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
    /// <summary>
    /// 判断文件扩展名是否有效
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
        //将多个Asset打包进一个AssetBundle
        //assetBundleName为打包包名
        builds[0].assetBundleName = AssetBundleName;
        //assetName实际为打包路径

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
        /*将多个Asset打包进一个AssetBundle
        //assetBundleName为打包包名
        builds[0].assetBundleName = AssetBundleLoad.ObjectBundleName;
        //assetName实际为打包路径
        builds[0].assetNames = bulidAssets.ToArray();
        BuildPipeline.BuildAssetBundles(OutPutPath, builds, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        AssetDatabase.Refresh();*/
        AssetBundleBuild[] builds = new AssetBundleBuild[buildAssets.Count];
        for (int i = 0; i < builds.Length; i++)
        {
            //将路径删去只留下文件名作为包名
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
    /// 获取选中的资源路径
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
    /// 获取选中资源依赖路径
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
    /// 零冗余打包
    /// </summary>
    public static void BuildAssetBundleSets()
    {
        CheckOutputPath();
        //所有Editor选择Asset为SourceAsset
        List<string> selectedAssetNames = GetSelectAssetNames();
        //所有SourceAsset依赖为DerivedAsset
        List<List<GUID>> selectedAssetDependencies = new List<List<GUID>>();
        //遍历SourceAsset的依赖
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
    ///提取depsA和depB相同的依赖并去除他们
    /// </summary>
    /// <param name="depsA"></param>
    /// <param name="depsB"></param>
    /// <returns>返回值为相同依赖GUID链表</returns>
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
    ///  嵌套函数,用于使用依赖创建Node
    /// </summary>
    /// <param name="lastNode"></param>上层Node|最顶层应为SourceAsset
    /// <param name="currentAlINodes"></param>当前所有的Node
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
    /// 有向图打包
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
                //如果一个SourceAsset在之前已经被当做DerivedAsset添加过
                //那么直接使用相同的Node
                foreach (AssetBundleNode node in allNodes)
                {
                    if (node.assetName == assetName)
                    {
                        currentNode = node;
                        currentNode.PackageName = info.PackageName;
                        break;
                    }
                }
                //否则创建新的Node
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

            //如果包名为空，代表是一个DerivedAsset
            //那么就遍历引用了该DerivedAsset的Package，并分配到一个新的包中
            if (string.IsNullOrEmpty(node.PackageName))
            {
                //将被多个包引用的资源，分配到一个新的包下
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

                //因为该循环会遍历所有的Node
                //所以如果一个node的PackageNames不唯一，并且不是一个SourcePackage，则为PackageNames中的对应Package添加对该Package的引用
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
    /// 建立AB包哈希表
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
    /// 不建议使用
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
    /// 获取传入的asset的GUID,遍历并通过MD5算法计算这些GUID的hash值,将MD5 Hash 值转化为十六进制字符串返回作为包名
    /// </summary>
    /// <param name="assetNames">asset</param>
    /// <returns>十六进制MD5 Hash值(包名)</returns>
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
    /// 比较AB包大小和GUID
    /// </summary>
    /// <param name="oldHashTable"></param>
    /// <param name="newHashTable"></param>
    /// <returns>AssetBundleVersionDiffrence类,记录新增和删除的AB包</returns>
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
    /// 比较当前版本和上一版本新增和删除的AB包
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
    /// 选择是否增量打包
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
    /// 将打包后的文件从OutputPath复制到版本文件夹
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
        //复制哈希表
        string hashTableOriginPath = Path.Combine(OutputPath, "AssetBundleHashs");
        string hashTableVersionPath = Path.Combine(assetBundleVersionOutputPath, "AssetBundleHashs");
        File.Copy(hashTableOriginPath, hashTableVersionPath);
        //复制主包
        string mainBundleOriginPath = Path.Combine(OutputPath, "Local");
        string mainBundleVersionPath = Path.Combine(assetBundleVersionOutputPath, "Local");
        File.Copy(mainBundleOriginPath, mainBundleVersionPath);
        //复制package
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
    /// 读取哈希表
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
    /// 将加载的config保存为Json格式
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
    /// 将json文件读取并覆盖config
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
    //一个Node可能引用多个Node,多个Node也可能引用一个Node
    public List<AssetBundleNode> Nodes = new List<AssetBundleNode>();

}
public class AssetBundleNode
{
    //每个Node对应一个Asset
    public string assetName;
    ///代表SourceAsset的序号,如果不是SourceAsset侧为默认值
    public int SourceIndex = -1;
    /// 代表引引用该资源的SourceAsset数量和序号
    public List<int> SourceIndices;
    //当前Node引用的Nodes
    public AssetBundleEdge OutEdge;
    //引用当前Node的Nodes
    public AssetBundleEdge InEdge;
    public string PackageName;
    public List<string> PackageNames = new List<string>();
}

