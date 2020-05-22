﻿using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(AbilityCollection))]
public class AbilityCollectionEditor : Editor
{
    private AbilityCollection abilityCollection;

    private void OnEnable()
    {
        // Cache the reference to the target.
        abilityCollection = (AbilityCollection) target;

    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        /* seulement si Status est un SO
         * if (GUILayout.Button("Add all Statuses SO to Collection"))
        {
            statusCollectionSO.statuses.Clear();
            string[] guids = UnityEditor.AssetDatabase.FindAssets ("t:" + typeof(StatusSO).Name);
            for(int i = 0; i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath (guids [i]);
                Debug.Log(guids [i] + " / " +path);
                StatusSO status = UnityEditor.AssetDatabase.LoadAssetAtPath<StatusSO> (path);
                statusCollectionSO.statuses.Add(status);
                //UnityEditor.EditorUtility.SetDirty(this);
            }

            UnityEditor.AssetDatabase.Refresh();
        }*/

        if (GUILayout.Button("Load Collection From Json"))
        {
            string json = File.ReadAllText(Application.dataPath + "/_Databases/Abilities/AbilityCollection.json"); // loading all the text out of the file into a string, assuming the text is all JSON
            JsonUtility.FromJsonOverwrite(json, abilityCollection); 

            UnityEditor.AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Save Collection as Json"))
        {
            string json = JsonUtility.ToJson(abilityCollection, true);
            Debug.Log(json); 
            string path = Application.dataPath + "/_Databases/Abilities/AbilityCollection.json";
            Debug.Log(path);

            //Create Directory if it does not exist
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            try
            {
                StreamWriter sw = File.CreateText(path); // if file doesnt exist, make the file in the specified path
                sw.Close();
                File.WriteAllText(path, json);
                Debug.Log("Saved Data to: " + path.Replace("/", "\\"));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed To Save Data to: " + path.Replace("/", "\\"));
                Debug.LogWarning("Error: " + e.Message);
            }

            UnityEditor.AssetDatabase.Refresh();

        }

    }



}


