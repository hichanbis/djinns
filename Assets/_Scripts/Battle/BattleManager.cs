using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    private static BattleManager instance;

    public BattleStates currentState;
    public List<GameObject> playerUnits;
    public List<GameObject> monsterUnits;

    public GameObject currentActingUnit;
    public GameObject currentTargetUnit;
    public BattleAction currentUnitAction;
    private List<BattleAction> turnActions;
    public int currentPlayerIndex = 0;
    public int currentEnemyIndex = 0;
    public bool backToMainMenu;
    public bool restartBattle;
    public bool chosen;
    public bool victoryAcknowledged;
    private bool battleEnd;

    private SceneController sceneController;
    // Reference to the SceneController to actually do the loading and unloading of scenes.

    public static BattleManager Instance
    {
        get
        {
            return instance;
        }
    }


    public enum BattleStates
    {
        InitBattle,
        ActionChoice,
        Rage,
        Victory,
        Failure
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        if (FindObjectOfType(typeof(EventManager)) == null)
        {
            Debug.Log("No EventManager found, it is likely the persistent scene is unloaded so it is debug mode");
            StartCoroutine(LoadDebugPersistentScene());
            Debug.Log("Ok persistent scene loaded go debug");
        }


        //mockup game
        if (Game.current == null)
        {
            Debug.Log("Dev mockup");
            Game.current = new Game("ExploTest");
        }
        else
        {
            Debug.Log(Game.current);
        }
    }

    IEnumerator LoadDebugPersistentScene()
    {
        yield return SceneManager.LoadSceneAsync("Persistent", LoadSceneMode.Additive);
    }

    // Use this for initialization
    void Start()
    {
        sceneController = FindObjectOfType<SceneController>();

        playerUnits = BattleStart.InstantiatePlayerParty();
        currentActingUnit = playerUnits[0];
        monsterUnits = BattleStart.InstantiateMonsterParty();
        currentTargetUnit = monsterUnits[0];

        StartCoroutine(BattleStateMachine());
    }

    IEnumerator BattleStateMachine()
    {
        yield return null;

        //init UI and rotate anim and wait for it to finish
        EventManager.TriggerEvent(BattleEventMessages.unitsLoaded.ToString());
       

        battleEnd = false;
        restartBattle = false;
        backToMainMenu = false;
        victoryAcknowledged = false;

        currentState = BattleStates.InitBattle;

        while (!battleEnd)
        {
            yield return StartCoroutine(currentState.ToString());
        }

        EventManager.TriggerEvent(BattleEventMessages.battleEnded.ToString());
        yield return null;

        if (victoryAcknowledged)
            sceneController.FadeAndLoadScene("ExploTest");
        else if (backToMainMenu)
            sceneController.FadeAndLoadScene("MainMenu");
        else if (restartBattle)
            sceneController.FadeAndLoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator InitBattle()
    {
        //wait 3 sec before taunt (during rotate anim) 
        yield return new WaitForSeconds(3f);
        EventManager.TriggerEvent(BattleEventMessages.taunt.ToString());

        //wait for rotate and taunt to finish before action choice
        yield return new WaitForSeconds(3f);

        currentState = BattleStates.ActionChoice;
    }


    IEnumerator ActionChoice()
    {
        EventManager.TriggerEvent(BattleEventMessages.newTurn.ToString());
        yield return null;

        if (!currentState.Equals(BattleStates.ActionChoice))
            yield break;

        turnActions = new List<BattleAction>();
        List<GameObject> battlingUnits = playerUnits.Concat(monsterUnits).ToList();

        for (int i = 0; i < battlingUnits.Count; i++)
        {
            currentActingUnit = battlingUnits[i];
            currentUnitAction = new BattleAction();
            if (IsGameObjectAPlayer(currentActingUnit) && !currentActingUnit.GetComponent<BattleScript>().Dead)
            {
                currentUnitAction.fromUnit = currentActingUnit;
                EventManager.TriggerEvent(BattleEventMessages.playerChoiceExpected.ToString());
                bool choiceDone = false;
                while (!choiceDone)
                {
                    if (currentUnitAction.ability != null)
                    {
                        if (currentUnitAction.ability.targetType.Equals(TargetType.Self) || currentUnitAction.ability.targetType.Equals(TargetType.AllSame) || currentUnitAction.ability.targetType.Equals(TargetType.AllOpposite))
                            choiceDone = true;
                        else if (currentUnitAction.ability.targetType.Equals(TargetType.Same) || currentUnitAction.ability.targetType.Equals(TargetType.Opposite))
                        {
                            if (currentUnitAction.targets != null)
                                choiceDone = true;
                        }

                        //launch anim for attack preparation here (guard or magic focus)!
                    }
                    yield return null;
                }
                turnActions.Add(currentUnitAction);
                IncrementPlayerIndex();
            }
            else if (!IsGameObjectAPlayer(currentActingUnit))
            {
                List<GameObject> alivePlayerUnits = playerUnits.Where(p => !p.GetComponent<BattleScript>().Dead).ToList();
                if (alivePlayerUnits.Count > 0)
                {
                    BattleAction action = new BattleAction(currentActingUnit, new List<GameObject>() { alivePlayerUnits[Random.Range(0, alivePlayerUnits.Count)] }, currentActingUnit.GetComponent<BattleScript>().Character.getAbility("Attack"));
                    turnActions.Add(action);
                } 
                IncrementEnemyIndex();
            }

            yield return null;
        }
        currentState = BattleStates.Rage;
    }

    IEnumerator Rage()
    {
        yield return null;
        //turnActions.OrderByDescending(u => u.GetComponent<BattleCharacter>().Character.stats.speed).ToList();
        foreach (BattleAction battleAction in turnActions)
        {
            if (AreAllPlayersDead() || AreAllEnemiesDead())
                break;

            //si unit source est détruite, dead ou paralysé par l'attaque précedente on skip sa turn action
            if (battleAction.fromUnit == null || battleAction.fromUnit.GetComponent<BattleScript>().Dead)
                continue;

            ReassignTargetIfNeeded(battleAction);
            Debug.Log(battleAction);

            Vector3 initPos = battleAction.fromUnit.transform.position;
            if (battleAction.ability.distance.Equals(Distance.Close))
                yield return StartCoroutine(battleAction.fromUnit.GetComponent<BattleScript>().RunToTarget(battleAction.targets[0]));


            yield return StartCoroutine(battleAction.fromUnit.GetComponent<BattleScript>().LaunchAndWaitAnim(battleAction.ability.id));

            //battleManager calculates the damage and send it to targets who withstand the impact

            foreach (GameObject target in battleAction.targets.ToList())
            {
                Ability ab = battleAction.ability;
                int rawDmg = 0;
                int dmg = 0;

                int multiplyingStat = 0;
                if (ab.abilityType.Equals(AbilityType.Magic))
                    multiplyingStat = battleAction.fromUnit.GetComponent<BattleScript>().Character.GetStat(StatName.intelligence).GetValue();
                else
                    multiplyingStat = battleAction.fromUnit.GetComponent<BattleScript>().Character.GetStat(StatName.strength).GetValue();

                rawDmg = ab.power * multiplyingStat * 6;

                if (ab.targetType.Equals(TargetType.Self) || ab.targetType.Equals(TargetType.Same) || ab.targetType.Equals(TargetType.AllSame))
                    dmg = rawDmg;
                else //if opposite dmg = rawDmg with defense reduction
                    dmg = Mathf.CeilToInt(rawDmg / (target.GetComponent<BattleScript>().Character.GetStat(StatName.defense).GetValue() * 2));

                target.GetComponent<BattleScript>().TakeDamage(dmg);

                foreach (string statusClass in battleAction.ability.status)
                {
                    Status status = (Status)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(statusClass);
                    if (status == null)
                        throw new System.ExecutionEngineException("Status class " + statusClass + " doesn't exist");
                    target.GetComponent<BattleScript>().TryAddStatus(status);
                }
                yield return null;
            }

            battleAction.fromUnit.GetComponent<BattleScript>().removeMp(battleAction.ability);


            if (battleAction.ability.distance.Equals(Distance.Close))
                battleAction.fromUnit.transform.position = initPos; 


            yield return new WaitForSeconds(1f);
        }
			
        if (!AreAllPlayersDead() && !AreAllEnemiesDead())
        {
            EventManager.TriggerEvent(BattleEventMessages.applyEndTurnStatus.ToString());
            yield return null;
        }

        SetBattleStateAfterDamage();
        yield return null;
    }

    private void SetBattleStateAfterDamage()
    {
        if (AreAllPlayersDead())
            currentState = BattleStates.Failure;
        else if (AreAllEnemiesDead())
            currentState = BattleStates.Victory;
        else
            currentState = BattleStates.ActionChoice;
    }

    private void ReassignTargetIfNeeded(BattleAction battleAction)
    {
        if (battleAction.ability.targetType.Equals(TargetType.Self))
            battleAction.targets = new List<GameObject>() { battleAction.fromUnit };
        else if (battleAction.ability.targetType.Equals(TargetType.AllSame))
        {
            if (IsGameObjectAPlayer(battleAction.fromUnit))
                battleAction.targets = playerUnits;
            else
                battleAction.targets = monsterUnits;
        }
        else if (battleAction.ability.targetType.Equals(TargetType.AllOpposite))
        {
            if (IsGameObjectAPlayer(battleAction.fromUnit))
                battleAction.targets = monsterUnits;
            else
                battleAction.targets = playerUnits;
        }
        else if (battleAction.ability.targetType.Equals(TargetType.Opposite) && (battleAction.targets == null || battleAction.targets[0] == null || battleAction.targets[0].GetComponent<BattleScript>().Dead))
        {
            if (IsGameObjectAPlayer(battleAction.fromUnit))
                battleAction.targets = new List<GameObject>() { monsterUnits.First() };
            else
                battleAction.targets = new List<GameObject>() { playerUnits.First() };
        }
        else if (battleAction.ability.targetType.Equals(TargetType.Same) && (battleAction.targets == null || battleAction.targets[0] == null || battleAction.targets[0].GetComponent<BattleScript>().Dead))
        {
            if (IsGameObjectAPlayer(battleAction.fromUnit))
            {
                if (!battleAction.ability.id.Equals("Revive"))
                    battleAction.targets = new List<GameObject>() { playerUnits.First() };
            }
            else
                battleAction.targets = new List<GameObject>() { monsterUnits.First() };
        }
    }

    IEnumerator Victory()
    {
        yield return null;
        EventManager.TriggerEvent(BattleEventMessages.win.ToString());

        while (!victoryAcknowledged)
        {
            yield return null;
        }

        ImpactHpMpAfterVictory();

        battleEnd = true;
    }


    IEnumerator Failure()
    {
        yield return null;
        EventManager.TriggerEvent(BattleEventMessages.lose.ToString());

        while (!restartBattle && !backToMainMenu)
        {
            yield return null;
        }

        battleEnd = true;
    }

    //Impact the hp to the game party ...
    void ImpactHpMpAfterVictory()
    {
        for (int i = 0; i < playerUnits.Count; i++)
        {
            Character player = playerUnits[i].GetComponent<BattleScript>().Character;


            if (player.GetStat(StatName.hpNow).baseValue == 0)
                Game.current.party[i].GetStat(StatName.hpNow).baseValue = 1;
            else
            {
                Game.current.party[i].GetStat(StatName.hpNow).baseValue = player.GetStat(StatName.hpNow).baseValue;
                Game.current.party[i].GetStat(StatName.mpNow).baseValue = player.GetStat(StatName.mpNow).baseValue;
            }
        }

    }

    bool IsGameObjectAPlayer(GameObject unit)
    {
        return (System.Enum.IsDefined(typeof(PlayerName), unit.name));
    }

    bool AreAllPlayersDead()
    {
        int nbPlayersDead = 0;
        for (int i = 0; i < playerUnits.Count; i++)
        {
            if (playerUnits[i].GetComponent<BattleScript>().Dead)
                nbPlayersDead++;
        }

        if (nbPlayersDead == playerUnits.Count)
            return true;
        else
            return false;

    }

    bool AreAllEnemiesDead()
    {
        return monsterUnits.Count == 0;
    }


    // Update is called once per frame
    void Update()
    {
    }

    void CaptureSwitchPlayerInput()
    {
        if (Input.GetButtonDown("L2"))
        {
            DecrementPlayerIndex();
        }

        if (Input.GetButtonDown("R2"))
        {
            IncrementPlayerIndex();
        }
    }

    public void IncrementEnemyIndex()
    {
        if (currentEnemyIndex != monsterUnits.Count - 1)
            currentEnemyIndex++;
        else
            currentEnemyIndex = 0;
    }

    public void DecrementEnemyIndex()
    {
        if (currentEnemyIndex != 0)
            currentEnemyIndex--;
        else
            currentEnemyIndex = monsterUnits.Count - 1;
    }


    public void IncrementPlayerIndex()
    {

        if (currentPlayerIndex != playerUnits.Count - 1)
            currentPlayerIndex++;
        else
            currentPlayerIndex = 0;
    }

    public void DecrementPlayerIndex()
    {

        if (currentPlayerIndex != 0)
            currentPlayerIndex--;
        else
            currentPlayerIndex = playerUnits.Count - 1;
    }

    public GameObject GetCurrentPlayer()
    {
        return playerUnits[currentPlayerIndex];
    }

    public GameObject GetCurrentEnemy()
    {
        return monsterUnits[currentEnemyIndex];
    }

    public void SetCurrentTargetFromName(string targetName)
    {
        List<GameObject> allUnits = playerUnits.Concat(monsterUnits).ToList();
        currentTargetUnit = allUnits.Find(u => u.name.Equals(targetName));
    }

}
