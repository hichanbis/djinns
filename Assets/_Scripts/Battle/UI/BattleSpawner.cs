using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleSpawner : MonoBehaviour
{
    public BattleUnits battleUnits;
    public GameProgress gameProgress;
    public Character enemyCharacterData;
    // Start is called before the first frame update
    void Start()
    {
        battleUnits.SetPlayerUnits(InstantiatePlayerParty());
        battleUnits.SetEnemyUnits(InstantiateEnemyUnits());
        battleUnits.SetTargetUnit(battleUnits.enemyUnits.Last<BattleScript>());
    }

    private List<BattleScript> InstantiatePlayerParty()
    {
        List<BattleScript> players = new List<BattleScript>();
        int nbPlayers = gameProgress.party.Count;
        if (nbPlayers > 3)
            nbPlayers = 3;
        float spaceBetweenPlayers = 3.5f;
        float xPos = -spaceBetweenPlayers / 2 * (nbPlayers - 1);
        float zPos = -4f;

        Debug.Log(nbPlayers);

        for (int i = 0; i < nbPlayers; i++)
        {

            Character character = ScriptableObject.Instantiate<Character>(gameProgress.party[i]);
            Vector3 spawnPosition = new Vector3(xPos, 0f, zPos);
            Quaternion rotation = Quaternion.LookRotation(new Vector3(xPos, 0, 0) - spawnPosition);
            GameObject unitPlayer = Instantiate(Resources.Load("Player") as GameObject, spawnPosition, rotation) as GameObject;
            unitPlayer.name = character.id;
            unitPlayer.GetComponent<Movement>().enabled = false;
            unitPlayer.GetComponent<Cinemachine.Examples.CharacterMovement>().enabled = false;
            unitPlayer.GetComponent<AttackOtherOnCollide>().enabled = false;

            
            unitPlayer.GetComponent<BattleScript>().SetCharacter(character);
            unitPlayer.GetComponent<BattleScript>().enabled = true;

            players.Add(unitPlayer.GetComponent<BattleScript>());

            xPos += spaceBetweenPlayers;
        }
        return players;
    }

    private List<BattleScript> InstantiateEnemyUnits()
    {
        List<BattleScript> enemies = new List<BattleScript>();
        int nbEnemies = Random.Range(2, 5);
        //int nbEnemies = 1;
        float spaceBetweenEnemies = 4;
        float xPos = -spaceBetweenEnemies / 2 * (nbEnemies - 1);
        float zPos = 4f;

        for (int i = 0; i < nbEnemies; i++)
        {
            string enemyName = "Enemy " + i;

            Character character = ScriptableObject.Instantiate<Character>(enemyCharacterData);
            character.name = enemyName;
            Vector3 spawnPosition = new Vector3(xPos, 0f, zPos);
            Quaternion rotation = Quaternion.LookRotation(new Vector3(xPos, 0, 0) - spawnPosition);
            GameObject enemy = Instantiate(Resources.Load("Enemy") as GameObject, spawnPosition, rotation) as GameObject;
            enemy.name = enemyName;
            enemy.GetComponent<AttackOtherOnCollide>().enabled = false;
            enemy.GetComponent<BattleScript>().SetCharacter(character);
            enemies.Add(enemy.GetComponent<BattleScript>());
            xPos += spaceBetweenEnemies;
        }
        return enemies;
    }
}
