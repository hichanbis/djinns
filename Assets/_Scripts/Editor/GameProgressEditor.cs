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

        if (GUILayout.Button("Add Cassim character to game"))
        {
            AbilityCollection abilityCollection = (AbilityCollection)AssetDatabase.LoadAssetAtPath("Assets/_Databases/Abilities/AbilityCollection.asset", typeof(AbilityCollection));
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
            Character Cassim = new Character(PlayerName.Cassim.ToString(), Element.Wind, abilityCollection.abilities, defaultStats);
            game.party = new List<Character>();
            game.party.Add(Cassim);

            UnityEditor.AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Load From StartGameProgress"))
        {
            game.LoadFromStartGameProgress();
            UnityEditor.AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Load From DebugGameProgress"))
        {
            game.LoadFromDebugGameProgress();
            UnityEditor.AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Load From currentGame.Json"))
        {
            game.Load(10);

            UnityEditor.AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Save as currentGame.Json"))
        {
            game.Save(10);
                
            UnityEditor.AssetDatabase.Refresh();
          
        }

        if (GUILayout.Button("Reset values"))
        {
            //game.Reset();
            game.party = new List<Character>();
            game.currentScene = null;
            game.position = new Vector3();
            game.spawnPointIndexInScene = new int();
            game.satisfiedConditionNames = new List<string>();
            UnityEditor.AssetDatabase.Refresh();

        }



    }



}


