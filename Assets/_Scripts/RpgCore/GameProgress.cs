using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

[CreateAssetMenu()]
public class GameProgress : ScriptableObject
{
    public List<Character> party;
    public string currentScene;
    public Transform transform;
    public int spawnPointIndexInScene;
    public List<Condition> satisfiedConditions = new List<Condition>();

    public string getGameDesc()
    {
        string partyDesc = "";
        for (int i = 0; i < party.Count; i++)
        {
            partyDesc += party[i].id;
            if (i < party.Count - 1)
                partyDesc += " / ";
        }
        partyDesc += " / " + currentScene;
        return partyDesc;
    }

   
    public void RestoreCharactersAbilities(AbilityCollection abilityCollection)
    {
        foreach (Character character in party)
        {
            character.RestoreAbilities(abilityCollection);
        }
    }

    public void RestoreCharactersStatuses(StatusCollection statusCollection)
    {
        foreach (Character character in party)
        {
            character.RestoreStatuses(statusCollection);
        }
    }

    public void LoadFromStartGameProgress()
    {
        LoadFromGameProgressAsset("StartGameProgress");
    }

    public void LoadFromDebugGameProgress()
    {
        LoadFromGameProgressAsset("DebugGameProgress");
    }

    private void LoadFromGameProgressAsset(string name)
    {
        GameProgress gameProgress = Resources.Load<GameProgress>(name);

        string json = JsonUtility.ToJson(gameProgress, true);
        JsonUtility.FromJsonOverwrite(json, this);

        //SetConditionsToSavedStatusAfterLoad();
    }

    /*
    public void SetConditionsToSavedStatusAfterLoad()
    {
        Condition[] conditions = Resources.FindObjectsOfTypeAll(typeof(Condition)) as Condition[];
        foreach (string satisfiedConditionName in this.satisfiedConditionNames)
        {
            foreach (Condition condition in conditions)
            {
                if (condition.name.Equals(satisfiedConditionName))
                    condition.satisfied = true;
            }

        }
    }

    public void SetSatisfiedConditionsBeforeSave()
    {
        Condition[] conditions = Resources.FindObjectsOfTypeAll(typeof(Condition)) as Condition[];

        foreach (Condition condition in conditions)
        {
            if (!condition.name.Equals("") && condition.satisfied && !satisfiedConditionNames.Contains(condition.name))
                satisfiedConditionNames.Add(condition.name);
        }
    }*/

    public static string TryToGetGameDesc(int index)
    {
        string gameDesc = "";
        string path = Application.persistentDataPath + "/savedGame." + index + ".json";

        if (File.Exists(path))
        {
            GameProgress game = ScriptableObject.CreateInstance<GameProgress>();
            string json = File.ReadAllText(path); // loading all the text out of the file into a string, assuming the text is all JSON
            JsonUtility.FromJsonOverwrite(json, game);
            
            gameDesc = game.getGameDesc();
        }

        return gameDesc;
    }

}


