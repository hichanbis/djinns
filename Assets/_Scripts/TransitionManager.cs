using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;


public enum Advantage
{
    Player,
    Enemy
}

/*
 * Loads battle upon attack 
 * Loads exploration upon battle
 * Stores the battle initiator
 * Stores enemy killed indexes to avoid respawning them in the current explo scene
 * Persistent Object known by BattleManager and ExplorationManager
 */

public class TransitionManager : MonoBehaviour
{
	[SerializeField]
    private static TransitionManager instance;

	[SerializeField]
    private List<int> enemyIndexesToNotSpawn;

	[SerializeField]
    private Advantage advantage; //used in battle to give advantage to the player or enemy 

    public static TransitionManager Instance
    {
        get { return instance; }
    }

    public List<int> EnemyIndexesToNotSpawn
    {
        get
        {
            return enemyIndexesToNotSpawn;
        }

        set
        {
            enemyIndexesToNotSpawn = value;
        }
    }

    public void LoadBattle(Advantage advantage, Vector3 playerPosition, string enemyName)
    {
        DeclareBattlingEnemy(enemyName);
        this.advantage = advantage;
        Game.current.position = new Vector3Serializer(playerPosition);
        SceneManager.LoadScene("BattleTest");
    }

    //the enemy name ends with an int index corresponding to the spawn point index
    //Ex: Enemy12 corresponds to the spawn point 12
    public void DeclareBattlingEnemy(string enemyName)
    {
        //parse the name to get the index at the end
        String resultIndex = Regex.Match(enemyName, @"\d+$").Value;
        int index = Int32.Parse(resultIndex);
        enemyIndexesToNotSpawn.Add(index);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            enemyIndexesToNotSpawn = new List<int>();
        }
    }

}

