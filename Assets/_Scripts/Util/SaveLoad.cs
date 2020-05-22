using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using Object = UnityEngine.Object;

public class SaveLoad
{
    private SaveLoad()
    { }

    public static string basePath = Application.persistentDataPath + Path.DirectorySeparatorChar;

    public static void SaveToFile<T>(T o, string path)
    {
        path = basePath + path;
        string json = JsonUtility.ToJson(o, true);

        //Create Directonry if it does not exist
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        try
        {
            StreamWriter sw = File.CreateText(path); // if file doesnt exist, make the file in the specified path
            sw.Close();
            File.WriteAllText(path, json);
            Debug.Log("Saved Data to: " + path);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed To Save Data to: " + path);
            Debug.LogWarning("Error: " + e.Message);
        }
    }

    public static T LoadFromFile<T>(string path)
    {
        var o = default(T);
        path = basePath + path;

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            o = JsonUtility.FromJson<T>(json);
            Debug.Log("Loaded Data from: " + path);

        }

        return o;

    }

    
    public static void LoadOverwriteFromFile<T>(T o, string path)
    {
       path = basePath + path;

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(json, o);
            Debug.Log("Loaded Data from: " + path);
            
        }

    }

}
