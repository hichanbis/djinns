using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

[Serializable]
public class GameProgressSaveData
{
    public List<CharacterSaveData> party;
    public string currentScene;
    public Vector3 position;
    public Quaternion rotation;
    public int spawnPointIndexInScene;
    public List<string> satisfiedConditionNames;

    public GameProgressSaveData(GameProgress gameProgress)
    {
        party = new List<CharacterSaveData>();
        foreach (Character character in gameProgress.party)
        {
            party.Add(new CharacterSaveData(character));
        }

        currentScene = gameProgress.currentScene;
        position = gameProgress.transform.position;
        rotation = gameProgress.transform.rotation;
        spawnPointIndexInScene = gameProgress.spawnPointIndexInScene;

        satisfiedConditionNames = new List<string>();
        foreach (Condition condition in gameProgress.satisfiedConditions)
        {
            satisfiedConditionNames.Add(condition.name);
        }
    }


}


