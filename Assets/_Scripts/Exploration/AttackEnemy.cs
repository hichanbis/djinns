using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AttackEnemy : MonoBehaviour
{

    private bool canAttack;
    private string enemyName;
    private int nbHit = 0;

    void Start()
    {
        canAttack = false;
    }

    void OnTriggerEnter(Collider other)
    {
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
            canAttack = false;

            if (nbHit == 0)
                TransitionManager.Instance.LoadBattle(Advantage.Player, transform.position, enemyName);
            nbHit++;

        }
    }
}
