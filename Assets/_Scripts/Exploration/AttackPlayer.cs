using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AttackPlayer : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Player")
		{
			StartCoroutine(LaunchAttackOnPlayer(other.transform.position));
		}
	}
    
    IEnumerator LaunchAttackOnPlayer(Vector3 playerPosition)
    {
        yield return new WaitForSeconds(Random.Range(1f,4f));
        TransitionManager.Instance.LoadBattle(Advantage.Enemy, playerPosition, gameObject.name);
    }	 
}
