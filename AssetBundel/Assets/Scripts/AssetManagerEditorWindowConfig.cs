using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "AssetManagerEditorWindowConfig", menuName = "AssetManager/AssetManagerEditorWindowConfig", order = 1)]
public class AssetManagerEditorWindowConfig : ScriptableObject
{
    public Texture2D LogoTexture;
    public GUIStyle LogoLabelStyle = new GUIStyle();
    public GUIStyle TitleLabelStyle = new GUIStyle();
    public GUIStyle VersionLabelStyle = new GUIStyle();
}
