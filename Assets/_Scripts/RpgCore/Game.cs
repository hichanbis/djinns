using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Game
{

    public static Game current;
    public Dictionary<int, Character> party;
    public string currentScene;
    public Vector3Serializer position;


    public Game()
    {
        party = new Dictionary<int, Character>();
        Stat hp = new Stat(StatName.hp, 300);
        Stat hpNow = new Stat(StatName.hpNow, 300);
        Stat mp = new Stat(StatName.mp, 35);
        Stat mpNow = new Stat(StatName.mpNow, 35);
        Stat strength = new Stat(StatName.strength, 10);
        Stat defense = new Stat(StatName.defense, 10);
        Stat intelligence = new Stat(StatName.intelligence, 10);
        Stat agility = new Stat(StatName.agility, 10);
        List<Stat> defaultStats = new List<Stat> { hp, hpNow, mp, mpNow, strength, defense, intelligence, agility };
        Character shamal = new Character(PlayerName.Shamal.ToString(), Element.Wind, AbilityCollection.Instance.abilities, defaultStats, false);
        party.Add(0, shamal);

        currentScene = "ExploTest";

        Vector3 pos = new Vector3(10f, 0f, 10f);
        position = new Vector3Serializer(pos + new Vector3(2f, 2f, 2f));
    }

    //Debug constructor for Dev only
    public Game(string scene)
    {
        
        for (int i=0; i < StatusCollection.Instance.statuses.Count; i++)
        {
            Debug.Log("Loaded: " + StatusCollection.Instance.statuses[i]);
        }

        Debug.Log("loading Abilities");

        for (int i=0; i < AbilityCollection.Instance.abilities.Count; i++)
        {
            Debug.Log("Loaded: " + AbilityCollection.Instance.abilities[i]);
        }

        Debug.Log("now matching refs");
        AbilityCollection.Instance.ReplaceWithRefs();

        Debug.Log("loading Abilities");

        for (int i=0; i < AbilityCollection.Instance.abilities.Count; i++)
        {
            Debug.Log("Loaded: " + AbilityCollection.Instance.abilities[i]);
        }


        party = new Dictionary<int, Character>();

        Character shamal = new Character(PlayerName.Shamal.ToString(), Element.Wind, AbilityCollection.Instance.abilities, GenerateDefaultStats(), false);
        Character tayar = new Character(PlayerName.Tayar.ToString(), Element.Wind, AbilityCollection.Instance.abilities, GenerateDefaultStats(), true);
        Character daeva = new Character(PlayerName.Daeva.ToString(), Element.Wind, AbilityCollection.Instance.abilities, GenerateDefaultStats(), true);
        Character afia = new Character(PlayerName.Afia.ToString(), Element.Wind, AbilityCollection.Instance.abilities, GenerateDefaultStats(), false);
        Character dushara = new Character(PlayerName.Dushara.ToString(), Element.Wind, AbilityCollection.Instance.abilities, GenerateDefaultStats(), false);
        Character firdowsi = new Character(PlayerName.Firdowsi.ToString(), Element.Wind, AbilityCollection.Instance.abilities, GenerateDefaultStats(), false);
        party.Add(0, shamal);
        party.Add(1, tayar);
        party.Add(2, daeva);
        party.Add(3, afia);
        party.Add(4, dushara);
        party.Add(5, firdowsi);

        currentScene = scene;
        GameObject spawnPoint = GameObject.Find("SpawnPoint");
        Vector3 pos = new Vector3(10f, 0f, 10f);
        if (spawnPoint != null)
            pos = spawnPoint.transform.position;
        position = new Vector3Serializer(pos + new Vector3(2f, 2f, 2f));

       
    }

    private List<Stat> GenerateDefaultStats()
    {
        Stat hp = new Stat(StatName.hp, 300);
        Stat hpNow = new Stat(StatName.hpNow, 300);
        Stat mp = new Stat(StatName.mp, 35);
        Stat mpNow = new Stat(StatName.mpNow, 35);
        Stat strength = new Stat(StatName.strength, 10);
        Stat defense = new Stat(StatName.defense, 10);
        Stat intelligence = new Stat(StatName.intelligence, 10);
        Stat agility = new Stat(StatName.agility, 10);
        return new List<Stat> { hp, hpNow, mp, mpNow, strength, defense, intelligence, agility };
    }

    public string getGameDesc()
    {
        string partyDesc = "";
        foreach (KeyValuePair<int, Character> entry in party)
        {
            partyDesc += entry.Value.name;
            if (entry.Key < party.Count)
                partyDesc += " / ";
        }
        return partyDesc;
    }

    public override string ToString()
    {
        string partyDesc = "";
        foreach (KeyValuePair<int, Character> entry in party)
        {
            partyDesc += entry.Value.name;
            partyDesc += " HpNow : " + entry.Value.GetStat(StatName.hpNow).baseValue;
            if (entry.Key < party.Count)
                partyDesc += " / ";
        }
        return partyDesc;
    }

}
