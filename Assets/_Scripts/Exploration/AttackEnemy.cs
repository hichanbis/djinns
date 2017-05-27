using UnityEngine;
using System;
using System.Collections;
using System.Text.RegularExpressions;
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
                if (ExploSaveData.Instance.Advantage.Equals(BattleAdvantage.Unset))
                {
                    ExploSaveData.Instance.Advantage = BattleAdvantage.Player;

                    String resultIndex = Regex.Match(enemyName, @"\d+$").Value;
                    int index = Int32.Parse(resultIndex);
                    ExploSaveData.Instance.EnemyKilledIndexes.Add(index);

                    //This should be in ExploSaveData also!
                    Game.current.position = new Vector3Serializer(transform.position);
                    nbHit++;
                    sceneController.FadeAndLoadScene("BattleTest");   
                }
            }

        }
    }
}
