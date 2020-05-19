using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class Spawner : MonoBehaviour
{
    public GameProgress gameProgress;
    public GameObject vcam;
    public SaveData exploSaveData;

    void Awake()
    {
        if (FindObjectOfType(typeof(GameManager)) == null)
        {
            Debug.Log("No GameManager found, it is likely the persistent scene is unloaded so it is debug mode");
            StartCoroutine(LoadDebugPersistentScene());
            Debug.Log("Ok persistent scene loaded go debug");
        }

    }

    IEnumerator LoadDebugPersistentScene()
    {
        yield return SceneManager.LoadSceneAsync("Persistent", LoadSceneMode.Additive);
    }

    void Start()
    {
        Character character = gameProgress.party[0];

        //if loading from a battle on spawn a l'endroit du battle précedent

        //solution à trouver pour le spawn post battle!
        GameObject spawnPoint = GameObject.Find("SpawnPoint" + gameProgress.spawnPointIndexInScene);
        if (spawnPoint == null)
            Debug.LogError("pas de SpawnPoint trouvé à l'index: " + gameProgress.spawnPointIndexInScene);

        GameObject player = Instantiate(Resources.Load("Player") as GameObject, spawnPoint.transform.position, Quaternion.identity) as GameObject;
        player.name = "Player";

        //indiquer ici à la vcam de follow et lookAt le player
        vcam.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = player.transform;
        vcam.GetComponent<Cinemachine.CinemachineVirtualCamera>().LookAt = player.transform;


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

}
