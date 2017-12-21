using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEditor;

[CreateAssetMenu()]
public class Session : ScriptableObject
{

    public Game[] gameSlots = new Game[10];
    public Game currentGame;

    public void Save(int index)
    {
        
        //Game game = UnityEngine.Object.Instantiate(currentGame) as Game;
        Game game = gameSlots[index];
        game.currentScene = currentGame.currentScene;
        game.party = currentGame.party;
        game.position = currentGame.position;
        gameSlots[index] = game;

        string json = EditorJsonUtility.ToJson(this, true);
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

            
    }

	
    public void Load()
    {
        
        string json = File.ReadAllText(Application.persistentDataPath + "/savedGames.json"); // loading all the text out of the file into a string, assuming the text is all JSON
        EditorJsonUtility.FromJsonOverwrite(json, this); 
        Debug.Log("json : " + json + " loaded");
    }
       
}

