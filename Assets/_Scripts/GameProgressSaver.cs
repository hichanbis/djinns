using System.IO;
using UnityEngine;

public class GameProgressSaver : MonoBehaviour
{
    private static GameProgressSaver instance;
    public GameProgress gameProgress;
    public AbilityCollection abilityCollection;
    public StatusCollection statusCollection;

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

    void SaveGame()
    {
        for (int i = 0; i < gameProgress.party.Count; i++)
        {
            CharacterSaveData characterSaveData = new CharacterSaveData();
            string path = "slot10" + Path.DirectorySeparatorChar + gameProgress.party[i].name + ".data.json";

            for (int j = 0; j < gameProgress.party[i].abilities.Count; j++)
            {
                characterSaveData.abilityIds.Add(gameProgress.party[i].abilities[j].id);
            }

            for (int j = 0; j < gameProgress.party[i].statuses.Count; j++)
            {
                characterSaveData.statusIds.Add(gameProgress.party[i].statuses[j].id);
            }


            SaveLoad.SaveToFile<CharacterSaveData>(characterSaveData, path);
        }

        gameProgress.Save(10);
    }

    void LoadGame()
    {
        gameProgress.Load(10);

        for (int i = 0; i < gameProgress.party.Count; i++)
        {
            string path = "slot10" + Path.DirectorySeparatorChar + gameProgress.party[i].name + ".data.json";
            CharacterSaveData characterSaveData = SaveLoad.LoadFromFile<CharacterSaveData>(path);

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


            SaveLoad.SaveToFile<CharacterSaveData>(characterSaveData, path);
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
            SaveGame();
        }

        if (Input.GetButtonDown("QuickLoad"))
        {
            LoadGame();
        }
    }
}
