using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetManagerEditorWindow : EditorWindow
{
    public DefaultAsset editorDirectory = null;
    public AssetManagerEditorWindowConfig AssetManagerWindowConfig;
    public string VersionString = "";

    void Awake()
    {
        AssetManagerEditor.LoadAssetManagerConfig(this);
        AssetManagerEditor.LoadAssetManagerWindowConfig(this);
    }
    void OnFocus()
    {
        AssetManagerEditor.LoadAssetManagerWindowConfig(this);
        AssetManagerEditor.LoadAssetManagerConfig(this);
    }
    void OnEnable()
    {
        AssetManagerEditor.AssetManagerConfig.GetFolderAllAssets();
    }
    void OnValidate()
    {
        AssetManagerEditor.LoadAssetManagerWindowConfig(this);
        AssetManagerEditor.LoadAssetManagerConfig(this);
    }
    void OnInspectorUpdate()
    {
        AssetManagerEditor.LoadAssetManagerWindowConfig(this);
        AssetManagerEditor.LoadAssetManagerConfig(this);
    }
    private void OnGUI()
    {
        #region 标题图
        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(AssetManagerWindowConfig.LogoTexture, AssetManagerWindowConfig.LogoLabelStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        #endregion
        #region 标题
        GUILayout.Space(20);
        GUILayout.Label(AssetManagerEditor.AssetManagerConfig.ManagerTitle, AssetManagerWindowConfig.TitleLabelStyle);
        #endregion
        #region 版本号
        GUILayout.Space(10);
        GUILayout.Label(VersionString.ToString(), AssetManagerWindowConfig.VersionLabelStyle);
        #endregion

        #region 打包文件夹
        GUILayout.Space(10);
        for (int i = 0; i < AssetManagerEditor.AssetManagerConfig.EditorPackageInfos.Count; i++)
        {
            GUILayout.BeginVertical("frameBox");
            PackageInfoEditor info = AssetManagerEditor.AssetManagerConfig.EditorPackageInfos[i];
            GUILayout.BeginHorizontal();
            info.PackageName = EditorGUILayout.TextField("AssetBundleName", info.PackageName);
            if (GUILayout.Button("Remove"))
            {
                AssetManagerEditor.RemoveAssetBundleInfo(info);
            }
            GUILayout.EndHorizontal();
            if (info.Assets.Count > 0)
            {
                GUILayout.BeginVertical("frameBox");
                for (int j = 0; j < info.Assets.Count; j++)
                {
                    GUILayout.BeginHorizontal();
                    info.Assets[j] = EditorGUILayout.ObjectField(info.Assets[j], typeof(UnityEngine.Object), true) as UnityEngine.Object;
                    if (GUILayout.Button("Remove"))
                    {
                        AssetManagerEditor.RemoveAsset(info, info.Assets[j]);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            if (GUILayout.Button("新增Asset"))
            {
                AssetManagerEditor.AddAsset(info);
            }
            GUILayout.EndVertical();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("新增Package"))
        {
            AssetManagerEditor.AddAssetBundleInfo();
            GUILayout.EndVertical();
        }
        //editorDirectory = EditorGUILayout.ObjectField(editorDirectory, typeof(DefaultAsset), true) as DefaultAsset;
        //if (AssetManagerEditor.AssetManagerConfig.AssetBundleDirectory != editorDirectory)
        //{
        //    if (editorDirectory == null)
        //    {
        //        AssetManagerEditor.AssetManagerConfig.CurrentAllAssets.Clear();
        //    }
        //    AssetManagerEditor.AssetManagerConfig.AssetBundleDirectory = editorDirectory;
        //    AssetManagerEditor.AssetManagerConfig.GetFolderAllAssets();
        //}
        #endregion

        #region 打包路径选择
        GUILayout.Space(10);
        AssetManagerEditor.LoadPattern = (AssetBundlePattern)EditorGUILayout.EnumPopup("打包路径", AssetManagerEditor.LoadPattern);
        //if (AssetManagerEditor.AssetManagerConfig.CurrentAllAssets != null)
        //{
        //    for (int i = 0; i < AssetManagerEditor.AssetManagerConfig.CurrentAllAssets.Count; i++)
        //    {
        //        AssetManagerEditor.AssetManagerConfig.CurrentSelectedAssets[i] = EditorGUILayout.ToggleLeft(AssetManagerEditor.AssetManagerConfig.CurrentAllAssets[i], AssetManagerEditor.AssetManagerConfig.CurrentSelectedAssets[i]);
        //    }
        //}
        #endregion
        #region 增量打包选择
        GUILayout.Space(10);
        AssetManagerEditor.AssetManagerConfig.BuildMode = (IncrementalBuildMode)EditorGUILayout.EnumPopup("增量打包模式", AssetManagerEditor.AssetManagerConfig.BuildMode);
        #endregion

        #region 打包按钮
        GUILayout.Space(20);
        if (GUILayout.Button("打包AssetBundle"))
        {
            //AssetManagerEditor.BuildAssetBundleFromFolder();
            //AssetManagerEditor.BuildAssetBundleFromEditorWindow();
            //AssetManagerEditor.GetAssetsFromPath();
            AssetManagerEditor.BuildAssetBundleFromDirectedGraph();
        }
        #endregion

        #region 保存config按钮
        GUILayout.Space(10);
        if (GUILayout.Button("保存Config"))
        {
            AssetManagerEditor.SaveConfigToJSON();
        }
        #endregion
        #region 读取config.json按钮
        GUILayout.Space(10);
        if (GUILayout.Button("读取config.json"))
        {
            AssetManagerEditor.ReadConfigFromJSON();
        }
        #endregion


    }

}
