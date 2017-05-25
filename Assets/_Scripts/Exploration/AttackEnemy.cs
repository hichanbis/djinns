using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AttackEnemy : MonoBehaviour
{

    private bool canAttack;
    private string enemyName;
    private int nbHit = 0;
    private SceneController sceneController;

    void Start()
    {
        sceneController = FindObjectOfType<SceneController>();
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

            //nbHit to make sur I don't call the methods twice
            if (nbHit == 0)
            {
                if (ExploSaveData.Instance.InitiateBattle(Advantage.Player, enemyName))
                {
                    Game.current.position = new Vector3Serializer(transform.position);
                    nbHit++;
                    sceneController.FadeAndLoadScene("BattleTest");   
                }
            }

        }
    }
}
