using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(Game))]
public class GameEditor : Editor
{
    private Game game;

    private void OnEnable()
    {
        // Cache the reference to the target.
        game = (Game)target;

    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Add character to game"))
        {
            AbilityCollection abilityCollection = (AbilityCollection)AssetDatabase.LoadAssetAtPath("Assets/_Databases/AbilityCollection.asset", typeof(AbilityCollection));
            Debug.Log(abilityCollection);

            Stat hp = new Stat(300);
            Stat hpNow = new Stat(300);
            Stat mp = new Stat(35);
            Stat mpNow = new Stat(35);
            Stat strength = new Stat(10);
            Stat defense = new Stat(10);
            Stat intelligence = new Stat(10);
            Stat agility = new Stat(10);
            Stats defaultStats = new Stats(hp, hpNow, mp, mpNow, strength, defense, intelligence, agility);
            Character Cassim = new Character(PlayerName.Cassim.ToString(), Element.Wind, abilityCollection.abilities, defaultStats, true);
            game.party.Add(Cassim);

            UnityEditor.AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Load From Json"))
        {
            string json = File.ReadAllText(Application.persistentDataPath + "/currentGame.json"); // loading all the text out of the file into a string, assuming the text is all JSON
            JsonUtility.FromJsonOverwrite(json, game); 

            UnityEditor.AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Save as Json"))
        {
            string json = JsonUtility.ToJson(game, true);
            Debug.Log(json); 
            string path = Application.persistentDataPath + "/currentGame.json";
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


