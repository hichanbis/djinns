using UnityEngine;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class AttackOtherOnCollide : MonoBehaviour
{
 
    public SaveData exploSaveData;
    public GameProgress gameProgress; 
    //public string uniqueIdentifier;             // A unique string set by a scene designer to identify what is being saved.
    //private string key;
    private bool canAttack;
    private string enemyName;
    private int nbTriggered = 0;
    private SceneController sceneController;
    private Vector3 playerPosition;
    
    // A string to identify what is being saved.  This should be set using information about the data as well as the uniqueIdentifier.


    void Start()
    {
        sceneController = FindObjectOfType<SceneController>();
        canAttack = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameObject.CompareTag("Player") && other.tag == "Enemy")
        {
            //nbHit to make sure I don't call the methods twice
            if (nbTriggered == 0)
            {
                playerPosition = transform.position;
                canAttack = true;
                enemyName = other.name;
                nbTriggered++;
            }
        }
        else if (gameObject.CompareTag("Enemy") && other.tag == "Player")
        {
            //nbHit to make sure I don't call the methods twice
            if (nbTriggered == 0)
            {
                playerPosition = other.transform.position;
                canAttack = true;
                enemyName = gameObject.name;
                nbTriggered++;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (gameObject.CompareTag("Player") && other.tag == "Enemy")
        {
            canAttack = false;
        }
        else if (gameObject.CompareTag("Enemy") && other.tag == "Player")
        {
            canAttack = false;
        }
    }

    void Update()
    {
        if (gameObject.CompareTag("Player") && canAttack && Input.GetButtonDown("Submit"))
        {
            canAttack = false;
            StartCoroutine(LaunchAttackOnOther(BattleAdvantage.Player, enemyName, transform.position));
        }
        else if (gameObject.CompareTag("Enemy") && canAttack)
        {
            canAttack = false;
            StartCoroutine(LaunchAttackOnOther(BattleAdvantage.Enemy, gameObject.name, playerPosition));
        }
    }

    IEnumerator LaunchAttackOnOther(BattleAdvantage battleAdvantage, String enemyName, Vector3 playerPosition)
    {
        if (battleAdvantage.Equals(BattleAdvantage.Enemy))
            yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 4f));

        if (ExploSaveData.Instance.Advantage.Equals(BattleAdvantage.Unset))
        {
            ExploSaveData.Instance.Advantage = battleAdvantage;
            exploSaveData.Save("advantage", battleAdvantage.ToString());

            String resultIndex = Regex.Match(enemyName, @"\d+$").Value;
            int index = Int32.Parse(resultIndex);
            ExploSaveData.Instance.EnemyKilledIndexes.Add(index);
            exploSaveData.Save("Enemy" + index + "Dead", true);

            gameProgress.position = playerPosition;
            sceneController.FadeAndLoadScene("BattleTest");
        } 
        yield return null;
    }
}
