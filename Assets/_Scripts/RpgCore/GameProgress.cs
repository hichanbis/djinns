using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

[CreateAssetMenu()]
public class GameProgress : ScriptableObject
{
    private static GameProgress instance;
    // The singleton instance.

    public List<Character> party;
    public string currentScene;
    public Vector3 position;
    public int spawnPointIndexInScene;
    public List<string> satisfiedConditionNames;

    private const string loadPath = "GameProgress";
    // The path within the Resources folder that

    public static GameProgress Instance                // The public accessor for the singleton instance.
    {
        get
        {
            // If the instance is currently null, try to find an AllConditions instance already in memory.
            if (!instance)
                instance = FindObjectOfType<GameProgress>();
            // If the instance is still null, try to load it from the Resources folder.
            if (!instance)
                instance = Resources.Load<GameProgress>(loadPath);
            // If the instance is still null, report that it has not been created yet.
            if (!instance)
                Debug.LogError("GameProgress has not been created yet or is not in Resources");
            return instance;
        }
        set { instance = value; }
    }


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
        string path = Application.persistentDataPath + "/savedGame." + index + ".json";
        Save(path);
    }

    public void Load(int index)
    {
        string path = Application.persistentDataPath + "/savedGame." + index + ".json";
        Load(path);
    }

    public void Save(string path)
    {
        Condition[] conditions = Resources.FindObjectsOfTypeAll(typeof(Condition))as Condition[];
        foreach (Condition condition in conditions)
        {
            if (!condition.name.Equals("") && condition.satisfied && !satisfiedConditionNames.Contains(condition.name))
                satisfiedConditionNames.Add(condition.name);
        }

        string json = JsonUtility.ToJson(this, true);

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

    public void Load(string path)
    {
        string json = File.ReadAllText(path);
        JsonUtility.FromJsonOverwrite(json, this); 

        Condition[] conditions = Resources.FindObjectsOfTypeAll(typeof(Condition))as Condition[];
        foreach (string satisfiedConditionName in this.satisfiedConditionNames)
        {
            foreach (Condition condition in conditions)
            {
                if (condition.name.Equals(satisfiedConditionName))
                    condition.satisfied = true;
            }

        }
    }

    /*
     * Must be improved by just parsing json for required fields 
     * instead of instantiating new game just for that (which is not logic as it is a singleton)
     * */
    public static string TryToGetGameDesc(int index)
    {
        string gameDesc = null;
        string path = Application.persistentDataPath + "/savedGame." + index + ".json";

        if (File.Exists(path))
        {
            GameProgress game = ScriptableObject.CreateInstance<GameProgress>();
            string json = File.ReadAllText(path); // loading all the text out of the file into a string, assuming the text is all JSON
            JsonUtility.FromJsonOverwrite(json, game); 
            Debug.Log("json : " + json + " loaded");
            gameDesc = game.getGameDesc();
        }

        return gameDesc;
    }

}


