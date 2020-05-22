using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class EditorUtils
{
    private EditorUtils()
    {
    }

#if UNITY_EDITOR
    // Slightly modified version of this answer: http://answers.unity.com/answers/1216386/view.html
    public static List<T> FindAssetsByType<T>(params string[] folders) where T : Object
    {
        string type = typeof(T).Name;

        string[] guids;
        if (folders == null || folders.Length == 0)
        {
            guids = AssetDatabase.FindAssets("t:" + type);
        }
        else
        {
            guids = AssetDatabase.FindAssets("t:" + type, folders);
        }

        List<T> assets = new List<T>();

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            assets.Add(AssetDatabase.LoadAssetAtPath<T>(assetPath));
        }
        return assets;
    }
#endif
}
