using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


public class CharacterIcon : ScriptableObject
{
    [MenuItem("Assets/Create/Character/CharacterIcon", false, 1)]
    private static void CreateNewAsset()
    {
        ProjectWindowUtil.CreateAssetWithContent(
            "Default Name.extension",
            string.Empty);
        CreatePrefabInProject("MyPrefab");
    }

    public static string CurrentProjectFolderPath
    {
        get
        {
            Type projectWindowUtilType = typeof(ProjectWindowUtil);
            MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            object obj = getActiveFolderPath.Invoke(null, new object[0]);
            return obj.ToString();
        }
    }

    private static void CreatePrefabInProject(string prefabName)
    {
        var prefab = UnityEngine.Resources.Load(prefabName);
        string targetPath = $"{CurrentProjectFolderPath}/{prefabName}.prefab";
        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(prefab), targetPath);
    }

}
