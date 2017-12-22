using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

[CreateAssetMenu()]
public class Game : ScriptableObject
{
    public List<Character> party;
    public string currentScene;
    public Vector3 position;

    public string getGameDesc()
    {
        string partyDesc = "";
        for (int i = 0; i < party.Count; i++)
        {
            partyDesc += party[i].name;
            if (i < party.Count - 1)
                partyDesc += " / ";
        }
        partyDesc += " / " + currentScene;
        return partyDesc;
    }

    public void Save(int index)
    {
        string json = JsonUtility.ToJson(this, true);
        Debug.Log(json); 
        string path = Application.persistentDataPath + "/savedGame." + index + ".json";
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
    }

    public void Load(int index)
    {
        string path = Application.persistentDataPath + "/savedGame." + index + ".json";

        string json = File.ReadAllText(path); // loading all the text out of the file into a string, assuming the text is all JSON
        JsonUtility.FromJsonOverwrite(json, this); 
        Debug.Log("json : " + json + " loaded");

    }

    public static Game TryToLoad(int index)
    {
        Game game = null;
        string path = Application.persistentDataPath + "/savedGame." + index + ".json";

        if (File.Exists(path))
        {
            game = ScriptableObject.CreateInstance<Game>();
            string json = File.ReadAllText(path); // loading all the text out of the file into a string, assuming the text is all JSON
            JsonUtility.FromJsonOverwrite(json, game); 
            Debug.Log("json : " + json + " loaded");
        }

        return game;
    }

}


