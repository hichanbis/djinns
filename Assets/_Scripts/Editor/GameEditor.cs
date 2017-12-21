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
            AbilityCollection abilityCollection = (AbilityCollection)AssetDatabase.LoadAssetAtPath("Assets/_Databases/StatusCollection", typeof(AbilityCollection));
        
            Stat hp = new Stat(StatName.hp, 300);
            Stat hpNow = new Stat(StatName.hpNow, 300);
            Stat mp = new Stat(StatName.mp, 35);
            Stat mpNow = new Stat(StatName.mpNow, 35);
            Stat strength = new Stat(StatName.strength, 10);
            Stat defense = new Stat(StatName.defense, 10);
            Stat intelligence = new Stat(StatName.intelligence, 10);
            Stat agility = new Stat(StatName.agility, 10);
            Stats defaultStats = new Stats(hp, hpNow, mp, mpNow, strength, defense, intelligence, agility);
            Character Cassim = new Character(PlayerName.Cassim.ToString(), Element.Wind, abilityCollection.abilities, defaultStats, false);
            game.party.Add(Cassim);

            UnityEditor.AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Load From Json"))
        {
            string json = File.ReadAllText(Application.persistentDataPath + "/savedGame.json"); // loading all the text out of the file into a string, assuming the text is all JSON
            JsonUtility.FromJsonOverwrite(json, game); 

            UnityEditor.AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Save as Json"))
        {
            string json = JsonUtility.ToJson(game, true);
            Debug.Log(json); 
            string path = Application.persistentDataPath + "/savedGame.json";
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


