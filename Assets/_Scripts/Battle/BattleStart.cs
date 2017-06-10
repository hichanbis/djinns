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
        float spaceBetweenPlayers = 5f;
        float xPos = -spaceBetweenPlayers / 2 * (nbPlayers - 1);
        float zPos = -5f;

        for (int i = 0; i < nbPlayers; i++)
        {
            Character character;
            if (Game.current.party.TryGetValue(i, out character))
            {
                Vector3 spawnPosition = new Vector3(xPos, 0f, zPos);
                Quaternion rotation = Quaternion.LookRotation(new Vector3(xPos, 0, 0) - spawnPosition);
                GameObject unitPlayer = Instantiate(Resources.Load("Player") as GameObject, spawnPosition, rotation) as GameObject;
                unitPlayer.GetComponent<Movement>().enabled = false;
                unitPlayer.GetComponent<AttackOtherOnCollide>().enabled = false;

                unitPlayer.name = character.name;
                unitPlayer.GetComponent<BattleScript>().Character = ObjectCopier.Clone<Character>(character);
                unitPlayer.GetComponent<BattleScript>().enabled = true;
                
                players.Add(unitPlayer);
            }
            xPos += spaceBetweenPlayers;
        }
        return players;
    }

    public static List<GameObject> InstantiateMonsterParty()
    {
        List<GameObject> enemies = new List<GameObject>();
        int nbEnemies = Random.Range(2, 6);
        //int nbEnemies = 5;
        float spaceBetweenEnemies = 5;
        float xPos = -spaceBetweenEnemies / 2 * (nbEnemies - 1);
        float zPos = 5f;

        for (int i = 0; i < nbEnemies; i++)
        {
            Vector3 spawnPosition = new Vector3(xPos, 0f, zPos);
            Quaternion rotation = Quaternion.LookRotation(new Vector3(xPos, 0, 0) - spawnPosition);
            GameObject enemy = Instantiate(Resources.Load("Enemy") as GameObject, spawnPosition, rotation) as GameObject;
            enemy.GetComponent<AttackOtherOnCollide>().enabled = false;
            enemy.name = "Enemy" + i;
            List<Ability> basicAbs = new List<Ability>();
            basicAbs.Add(AbilityCollection.Instance.FindAbilityFromId("Attack"));
            Stat hp = new Stat(StatName.hp, 100);
            Stat hpNow = new Stat(StatName.hpNow, 100);
            Stat mp = new Stat(StatName.mp, 35);
            Stat mpNow = new Stat(StatName.mpNow, 35);
            Stat strength = new Stat(StatName.strength, 20);
            Stat defense = new Stat(StatName.defense, 10);
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


