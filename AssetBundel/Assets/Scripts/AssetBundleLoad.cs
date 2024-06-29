using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AssetBundleLoad : MonoBehaviour
{
    #region ��������
    public AssetBundlePattern LoadPattern;
    AssetBundle SphereAssetBundle;
    AssetBundle CubeAssetBundle;
    AssetBundle SampleAssetBundle;
    GameObject SampleObject;
    public Button LoadAssetBundleButton;
    public Button LoadAssetButton;
    public Button UnloadAssetButton;
    public Button UnloadAssetBundleButton;

    public string HTTPAddress = "http://10.24.6.35:8080/";
    string DownloadPath;
    string AssetBundleLoadPath;
    public static string AssetBundleName = "AssetBundles";
    public static string ObjectBundleName = "sampleassetbundle.ab";
    #endregion

    void Start()
    {
        AssetManagerRuntime.AssetManagerInit(LoadPattern);
        AssetPackage package = AssetManagerRuntime.instance.LoadPackage("A");
        GameObject obj = package.LoadAsset<GameObject>("Assets/Scenes/Sphere.prefab");
        Instantiate(obj);
        Debug.Log(package.PackageName);
        CheckAssetBundleLoadPath();
        LoadAssetBundleButton.onClick.AddListener(CheckAssetBundlePattern);
        LoadAssetButton.onClick.AddListener(LoadAsset);
        UnloadAssetButton.onClick.AddListener(UnloadAsset);
        UnloadAssetBundleButton.onClick.AddListener(UnloadAssetBundle);
    }
    void LoadAssetBundle()
    {
        CheckAssetBundleLoadPath();
        /*//����·��
        string assetBundleOutPath = "AssetBundles";
        string assetBundlePath = Path.Combine(Application.dataPath, assetBundleOutPath, assetBundleOutPath);
        //��������
        AssetBundle mainAssetBundle = AssetBundle.LoadFromFile(assetBundlePath);
        //��������Manifest
        AssetBundleManifest manifest = mainAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        //Ŀ�����
        //string ObjectAssetBundleName = "test";
        //������������������
        foreach (var depName in manifest.GetAllDependencies(ObjectBundleName))
        {
            assetBundlePath = Path.Combine(Application.streamingAssetsPath, assetBundleOutPath, depName);
            AssetBundle.LoadFromFile(assetBundlePath);
        }
        //����Ŀ���
        assetBundlePath = Path.Combine(Application.streamingAssetsPath, assetBundleOutPath, ObjectBundleName);
        SampleAssetBundle = AssetBundle.LoadFromFile(assetBundlePath);*/
        string assetBundlePath=Path.Combine(AssetBundleLoadPath, AssetBundleName);
        AssetBundle mainAssetBundle = AssetBundle.LoadFromFile(assetBundlePath);
        AssetBundleManifest manifest = mainAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        foreach (string depName in manifest.GetAllDependencies("0"))
        {
            assetBundlePath = Path.Combine(AssetBundleLoadPath, depName);
            AssetBundle.LoadFromFile(assetBundlePath);
        }
        assetBundlePath = Path.Combine(AssetBundleLoadPath, "0");
        CubeAssetBundle = AssetBundle.LoadFromFile(assetBundlePath);
        assetBundlePath = Path.Combine(AssetBundleLoadPath, "1");
        SphereAssetBundle = AssetBundle.LoadFromFile(assetBundlePath);


    }
    void LoadAsset()
    {
        GameObject prefab = CubeAssetBundle.LoadAsset<GameObject>("Cube");
        SampleObject = Instantiate(prefab);
        prefab = SphereAssetBundle.LoadAsset<GameObject>("Sphere");
        SampleObject = Instantiate(prefab);
    }
    void UnloadAsset()
    {
        SampleAssetBundle.Unload(false);
        Resources.UnloadAsset(SampleObject);

    }
    void UnloadAssetBundle()
    {
        Destroy(SampleObject);
        SampleAssetBundle.Unload(true);
        Resources.UnloadUnusedAssets();
    }
    void CheckAssetBundleLoadPath()
    {
        switch (LoadPattern)
        {
            case AssetBundlePattern.EditorSimulation:
                //AssetBundleLoadPath = Path.Combine(Application.persistentDataPath, AssetBundleName);
                break;
            case AssetBundlePattern.Local:
                AssetBundleLoadPath = Path.Combine(Application.streamingAssetsPath, AssetBundleName);
                break;
            case AssetBundlePattern.Remote:
                DownloadPath = Path.Combine(Application.persistentDataPath, "DownloadAssetBundle");
                AssetBundleLoadPath = Path.Combine(DownloadPath, AssetBundleName);
                break;
        }
    }
    void CheckAssetBundlePattern()
    {
        if (LoadPattern == AssetBundlePattern.Remote)
        {
            StartCoroutine(DownloadFile(ObjectBundleName, true, LoadAssetBundle));
        }
        else
        {
            LoadAssetBundle();
        }
    }
    IEnumerator DownloadFile(string fileName, bool isSaveFile, Action callback)
    {
        string remotePath = Path.Combine(HTTPAddress, AssetBundleName);
        string mainBundelPath = Path.Combine(remotePath, fileName);
        UnityWebRequest webRequest = UnityWebRequest.Get(mainBundelPath);
        webRequest.SendWebRequest();
        while (!webRequest.isDone)
        {
            Debug.Log(webRequest.downloadedBytes);
            Debug.Log(webRequest.downloadProgress);
            yield return null;
        }
        Debug.Log(webRequest.downloadHandler.data.Length);
        if (!Directory.Exists(AssetBundleLoadPath))
        {
            Directory.CreateDirectory(AssetBundleLoadPath);
        }
        if (isSaveFile)
        {
            string assetBundleDownloadPath = Path.Combine(AssetBundleLoadPath, fileName);
            StartCoroutine(SaveFile(assetBundleDownloadPath, webRequest.downloadHandler.data, callback));
        }
    }
    IEnumerator SaveFile(string filePath, byte[] bytes, Action callback)
    {
        FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate);
        yield return fileStream.WriteAsync(bytes, 0, bytes.Length);
        fileStream.Flush();
        fileStream.Close();
        fileStream.Dispose();

        Debug.Log($"{filePath}�������");
        callback?.Invoke();
        yield return null;
    }
}
