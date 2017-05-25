using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Events;

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

public class ExploSaveData : MonoBehaviour
{
    private static ExploSaveData instance;
    private SceneController sceneController;
    private UnityAction battleLoadedListener;
    // Reference to the SceneController to actually do the loading and unloading of scenes.


    [SerializeField]
    public List<int> enemyIndexesToNotSpawn;

    [SerializeField]
    public Advantage advantage;
    //used in battle to give advantage to the player or enemy

    [SerializeField]
    public bool advantageGiven = false;

    public static ExploSaveData Instance
    {
        get { return instance; }
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

    protected void Start()
    {
        battleLoadedListener = new UnityAction(ResetAdvantage);
        EventManager.StartListening(BattleEventMessages.battleLoaded.ToString(), battleLoadedListener);
    }

    void ResetAdvantage(){
        advantageGiven = false;
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

    public bool InitiateBattle(Advantage advantage, string enemyName)
    {
        if (advantageGiven)
            return false;
        
        advantageGiven = true;
        DeclareBattlingEnemy(enemyName);
        this.advantage = advantage;
        return true;
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

    

}

