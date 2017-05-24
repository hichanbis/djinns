using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleStart : MonoBehaviour
{
    public static List<GameObject> InstantiatePlayerParty()
    {
        List<GameObject> players = new List<GameObject>();
        int nbPlayers = Game.current.party.Count;
        if (nbPlayers > 3)
            nbPlayers = 3;
        float spaceBetweenPlayers = 10f;
        float xPos = -spaceBetweenPlayers / 2 * (nbPlayers - 1);
        float zPos = -10f;

        for (int i = 0; i < nbPlayers; i++)
        {
            Character character;
            if (Game.current.party.TryGetValue(i, out character))
            {
                GameObject unitPlayer = Instantiate(Resources.Load("Battle" + character.name) as GameObject, new Vector3(xPos, 0.5f, zPos), Quaternion.identity) as GameObject;
                unitPlayer.name = character.name;
                unitPlayer.GetComponent<BattleScript>().Character = ObjectCopier.Clone<Character>(character);
                players.Add(unitPlayer);
            }
            xPos += 10f;
        }
        return players;
    }

    public static List<GameObject> InstantiateMonsterParty()
    {
        List<GameObject> enemies = new List<GameObject>();
        int nbEnemies = Random.Range(2, 6);
        float spaceBetweenEnemies = 10f;
        float xPos = -spaceBetweenEnemies / 2 * (nbEnemies - 1);
        float zPos = 10f;

        for (int i = 0; i < nbEnemies; i++)
        {
            GameObject enemy = Instantiate(Resources.Load("BattleEnemy") as GameObject, new Vector3(xPos, 0.5f, zPos), Quaternion.identity) as GameObject;
            enemy.name = "Enemy" + i;
            List<Ability> basicAbs = new List<Ability>();
            basicAbs.Add(AbilityCollection.Instance.FindAbilityFromId("Attack"));
            Stat hp = new Stat(StatName.hp, 100);
            Stat hpNow = new Stat(StatName.hpNow, 100);
            Stat mp = new Stat(StatName.mp, 35);
            Stat mpNow = new Stat(StatName.mpNow, 35);
            Stat strength = new Stat(StatName.strength, 20);
            Stat defense = new Stat(StatName.defense, 20);
            Stat intelligence = new Stat(StatName.intelligence, 10);
            Stat agility = new Stat(StatName.agility, 10);
            List<Stat> defaultStats = new List<Stat> { hp, hpNow, mp, mpNow, strength, defense, intelligence, agility };
            Character character = new Character(enemy.name, Element.Fire, basicAbs, defaultStats, false);
            enemy.GetComponent<BattleScript>().Character = character;
            enemies.Add(enemy);
            xPos += spaceBetweenEnemies;
        }
        return enemies;
    }

}


