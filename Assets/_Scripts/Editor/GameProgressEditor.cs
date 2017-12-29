using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(GameProgress))]
public class GameProgressEditor : Editor
{
    private GameProgress game;

    private void OnEnable()
    {
        // Cache the reference to the target.
        game = (GameProgress)target;

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
            string path = Application.persistentDataPath + "/currentGame.json";
            game.Load(path);

            UnityEditor.AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Save as Json"))
        {
            string path = Application.persistentDataPath + "/currentGame.json";
            game.Save(path);
                
            UnityEditor.AssetDatabase.Refresh();
          
        }

    }



}


