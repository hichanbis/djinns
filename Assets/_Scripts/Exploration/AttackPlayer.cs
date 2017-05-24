using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AttackPlayer : MonoBehaviour {

	bool isCollidingWithPlayer = false;

	void OnTriggerEnter(Collider other) {
		if (other.tag == "Player")
		{
			if(isCollidingWithPlayer) 
				return;

			isCollidingWithPlayer = true;
			StartCoroutine(LaunchAttackOnPlayer(other.transform.position));
		}
	}
    
    IEnumerator LaunchAttackOnPlayer(Vector3 playerPosition)
    {
		yield return null;
        yield return new WaitForSeconds(Random.Range(1f,4f));
        TransitionManager.Instance.LoadBattle(Advantage.Enemy, playerPosition, gameObject.name);
    }

	void Update(){
		isCollidingWithPlayer = false;
	}
}
