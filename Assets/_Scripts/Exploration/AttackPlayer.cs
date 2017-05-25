using UnityEngine;
using System.Collections;
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
        yield return new WaitForSeconds(Random.Range(1f, 4f));
        if (ExploSaveData.Instance.InitiateBattle(Advantage.Enemy, gameObject.name))
        {
            Game.current.position = new Vector3Serializer(playerPosition);
            sceneController.FadeAndLoadScene("BattleTest");
        } 
    }

    void Update()
    {
    }
}
