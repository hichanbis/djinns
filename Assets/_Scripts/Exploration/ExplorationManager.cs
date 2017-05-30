﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ExplorationManager : MonoBehaviour
{
    public SaveData exploSaveData;
    private GameObject player;

    void Awake()
    {
        //Debug code during dev 
        if (Game.current == null)
        {
            Debug.Log("MockupGame");
            Game.current = new Game("ExploTest");
        }
        else
        {
            Debug.Log(Game.current);
        }

    }

    void Start()
    {
        Character character;
        if (Game.current.party.TryGetValue(0, out character))
        {
            this.player = Instantiate(Resources.Load("Player") as GameObject, Game.current.position.V3, Quaternion.identity) as GameObject;
            this.player.name = character.name;
        }

        List<GameObject> enemySpawnpoints = new List<GameObject>(GameObject.FindGameObjectsWithTag("EnemySpawnPoint"));
        for (int i = 0; i < enemySpawnpoints.Count; i++)
        {
            bool enemyDead = false;
            if (!exploSaveData.Load("Enemy" + i + "Dead", ref enemyDead))
            //if (!ExploSaveData.Instance.EnemyKilledIndexes.Contains(i))
            {
                GameObject enemy = Instantiate(Resources.Load("Enemy") as GameObject, enemySpawnpoints[i].transform.position, Quaternion.identity) as GameObject;
                enemy.name = "enemy" + i;
            }
        }





    }

    public GameObject Player()
    {
        return player;
    }
}
