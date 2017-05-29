using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public enum BattleAdvantage
{
    Unset,
    Player,
    Enemy
}

/*
 * Stores the battle initiator
 * Stores enemy killed indexes to avoid respawning them in the current explo scene
 * Persistent Object known by BattleManager and ExplorationManager
 */

public class ExploSaveData
{
    private static ExploSaveData instance;
    private UnityAction battleEndedListener;

    [SerializeField]
    private List<int> enemyKilledIndexes;

    [SerializeField]
    private BattleAdvantage advantage;
    //used in battle to give advantage to the player or enemy

    private ExploSaveData()
    {
    }

    public static ExploSaveData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ExploSaveData();   
                instance.Init();
            }
            return instance;
        }
    }

    void Init()
    {
        ResetFull();
        battleEndedListener = new UnityAction(ResetPostBattle);
        EventManager.StartListening(BattleEventMessages.battleEnded.ToString(), battleEndedListener);
    }

 
    private void ResetFull()
    {
        advantage = BattleAdvantage.Unset;
        enemyKilledIndexes = new List<int>();
    }

    private void ResetPostBattle()
    {
        advantage = BattleAdvantage.Unset;
    }

    public BattleAdvantage Advantage
    {
        get
        { 
            return advantage;
        }
        set
        { 
            advantage = value;
        }
    }

    public List<int> EnemyKilledIndexes
    {
        get
        {
            return enemyKilledIndexes;
        }

        set
        {
            enemyKilledIndexes = value;
        }
    }

}

