using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AttackEnemy : MonoBehaviour {

    private bool canAttack;
    private string enemyName;

    void Start()
    {
        canAttack = false;
    }

	void OnTriggerEnter(Collider other) {
		if (other.tag == "Enemy")
		{
            canAttack = true;
            enemyName = other.name;
		}
	}

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            canAttack = false;
        }
    }

    void Update()
    {
        if (canAttack && Input.GetButtonDown("Submit"))
        {
            TransitionManager.Instance.LoadBattle(Advantage.Player, transform.position, enemyName);
        }
    }	 
}
