using System.IO;
using UnityEngine;

public class GameProgressSaver : MonoBehaviour
{
    public GameProgress gameProgress;
    public AbilityCollection abilityCollection;
    public StatusCollection statusCollection;
    public CharacterDataCollection characterDataCollection;
    public AllConditions allConditions;

    void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void AddAbilities()
    {
        gameProgress.party[0].abilities.Add(abilityCollection.GetAbilityFromId("Poison"));
        gameProgress.party[0].abilities.Add(abilityCollection.GetAbilityFromId("Regen"));

    }

    void AddStatuses()
    {
        gameProgress.party[0].statuses.Add(statusCollection.GetStatusFromId("Poison"));

    }

    void QuickSaveGame()
    {
        string path = "quickSave" + Path.DirectorySeparatorChar + "gameprogress.json";
        SaveGame(path);
    }

    void SaveGame(string path)
    {
        //setter la position à partir du player transform...
        GameObject player = GameObject.Find("Player");
        if (player)
        {
            gameProgress.transform = player.transform;
        }

        GameProgressSaveData gameProgressSaveData = new GameProgressSaveData(gameProgress);
        SaveLoad.SaveToFile<GameProgressSaveData>(gameProgressSaveData, path);
    }

    void QuickLoadGame()
    {
        string path = "quickSave" + Path.DirectorySeparatorChar + "gameprogress.json";
        LoadGame(path);
    }

    void LoadGame(string path)
    {
        GameProgressSaveData gameProgressSaveData = SaveLoad.LoadFromFile<GameProgressSaveData>(path);

        //restore fields
        gameProgress.currentScene = gameProgressSaveData.currentScene;

        gameProgress.transform.position = gameProgressSaveData.position;
        gameProgress.transform.rotation = gameProgressSaveData.rotation;

        //restore player transform position...
        GameObject player = GameObject.Find("Player");
        if (player)
        {
            player.transform.position = gameProgress.transform.position;
            player.transform.rotation = gameProgress.transform.rotation;
        }

        //restore satisfied conditions
        gameProgress.satisfiedConditions.Clear();

        foreach (string satisfiedConditionName in gameProgressSaveData.satisfiedConditionNames)
        {
            gameProgress.satisfiedConditions.Add(allConditions.GetConditionFromName(satisfiedConditionName));
        }

        gameProgress.spawnPointIndexInScene = gameProgressSaveData.spawnPointIndexInScene;

        //restore party of characters
        gameProgress.party.Clear();
        for (int i = 0; i < gameProgressSaveData.party.Count; i++)
        {
            CharacterSaveData characterSaveData = gameProgressSaveData.party[i];
            gameProgress.party.Add(characterDataCollection.GetCharacterFromId(characterSaveData.id));

            gameProgress.party[i].abilities.Clear();
            for (int j = 0; j < characterSaveData.abilityIds.Count; j++)
            {
                gameProgress.party[i].abilities.Add(abilityCollection.GetAbilityFromId(characterSaveData.abilityIds[j]));
            }

            gameProgress.party[i].statuses.Clear();
            for (int j = 0; j < characterSaveData.statusIds.Count; j++)
            {
                gameProgress.party[i].statuses.Add(statusCollection.GetStatusFromId(characterSaveData.statusIds[j]));
            }
        }
    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Test"))
        {
            AddAbilities();
            AddStatuses();
        }

        if (Input.GetButtonDown("QuickSave"))
        {
            QuickSaveGame();
        }

        if (Input.GetButtonDown("QuickLoad"))
        {
            QuickLoadGame();
        }
    }
}
