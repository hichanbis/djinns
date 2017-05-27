using UnityEngine;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class AttackPlayer : MonoBehaviour
{

    int nbTriggered = 0;
    private SceneController sceneController;

    void Start(){
        sceneController = FindObjectOfType<SceneController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (nbTriggered == 0)
            {
                nbTriggered++;
                StartCoroutine(LaunchAttackOnPlayer(other.transform.position));
            }
        }
    }

    IEnumerator LaunchAttackOnPlayer(Vector3 playerPosition)
    {
        yield return null;
        yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 4f));
        if (ExploSaveData.Instance.Advantage.Equals(BattleAdvantage.Unset))
        {
            ExploSaveData.Instance.Advantage = BattleAdvantage.Enemy;

            String resultIndex = Regex.Match(gameObject.name, @"\d+$").Value;
            int index = Int32.Parse(resultIndex);
            ExploSaveData.Instance.EnemyKilledIndexes.Add(index);

            Game.current.position = new Vector3Serializer(playerPosition);
            sceneController.FadeAndLoadScene("BattleTest");
        } 
    }

    void Update()
    {
    }
}
