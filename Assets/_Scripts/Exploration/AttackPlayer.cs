using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AttackPlayer : MonoBehaviour
{

    int nbTriggered = 0;

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
        TransitionManager.Instance.LoadBattle(Advantage.Enemy, playerPosition, gameObject.name);
    }

    void Update()
    {
    }
}
