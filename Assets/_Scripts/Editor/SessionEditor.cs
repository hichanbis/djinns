using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(Session))]
public class SessionEditor : Editor
{
    private Session session;

    private void OnEnable()
    {
        // Cache the reference to the target.
        session = (Session)target;

    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        if (GUILayout.Button("Load From Json"))
        {
            string json = File.ReadAllText(Application.persistentDataPath + "/savedGames.json"); // loading all the text out of the file into a string, assuming the text is all JSON
            JsonUtility.FromJsonOverwrite(json, session); 

            UnityEditor.AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Save as Json"))
        {
            string json = EditorJsonUtility.ToJson(session, true);
            Debug.Log(json); 
            string path = Application.persistentDataPath + "/savedGames.json";
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


