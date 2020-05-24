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
            //game.Load(10);

            UnityEditor.AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Save as currentGame.Json"))
        {
            //game.Save(10);
                
            UnityEditor.AssetDatabase.Refresh();
          
        }

        if (GUILayout.Button("Reset values"))
        {
            game.party = new List<Character>();
            game.currentScene = null;
            game.transform = null;
            game.spawnPointIndexInScene = new int();
            game.satisfiedConditions = new List<Condition>();
            UnityEditor.AssetDatabase.Refresh();

        }



    }



}


