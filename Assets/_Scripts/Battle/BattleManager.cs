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

    public GameObject currentUnit;
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

        //mockup game
        if (Game.current == null)
        {
            Debug.Log("Dev mockup");
            Game.current = new Game("ExploTest");
        }
    }

    // Use this for initialization
    void Start()
    {
        //TransitionManager.Instance.isLoadingBattle = false;
        sceneController = FindObjectOfType<SceneController>();
        StartCoroutine(FSM());
    }

    IEnumerator FSM()
    {
        battleEnd = false;
        restartBattle = false;
        backToMainMenu = false;
        victoryAcknowledged = false;

        //Enter the coroutine after Start
        yield return null;
        
        currentState = BattleStates.InitBattle;


        while (!battleEnd)
        {
            yield return StartCoroutine(currentState.ToString());
        }

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
        playerUnits = BattleStart.InstantiatePlayerParty();
        EventManager.TriggerEvent("playerUnitsExist");

        monsterUnits = BattleStart.InstantiateMonsterParty();
        EventManager.TriggerEvent("monsterUnitsExist");

        yield return new WaitForSeconds(1f);
        currentState = BattleStates.ActionChoice;
    }

    IEnumerator ActionChoice()
    {
        EventManager.TriggerEvent("newTurn");
        yield return null;

        if (!currentState.Equals(BattleStates.ActionChoice))
            yield break;

        turnActions = new List<BattleAction>();
        List<GameObject> battlingUnits = playerUnits.Concat(monsterUnits).ToList();

        for (int i = 0; i < battlingUnits.Count; i++)
        {
            currentUnit = battlingUnits[i];
            currentUnitAction = new BattleAction();
            if (IsGameObjectAPlayer(currentUnit) && !currentUnit.GetComponent<BattleScript>().Dead)
            {
                currentUnitAction.fromUnit = currentUnit;
                EventManager.TriggerEvent("playerChoiceExpected");
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
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                turnActions.Add(currentUnitAction);
                IncrementPlayerIndex();
            }
            else if (!IsGameObjectAPlayer(currentUnit))
            {
                List<GameObject> alivePlayerUnits = playerUnits.Where(p => !p.GetComponent<BattleScript>().Dead).ToList();
                if (alivePlayerUnits.Count > 0)
                {
                    BattleAction action = new BattleAction(currentUnit, new List<GameObject>() { alivePlayerUnits[Random.Range(0, alivePlayerUnits.Count)] }, currentUnit.GetComponent<BattleScript>().Character.getAbility("Attack"));
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
            yield return null;

            if (AreAllPlayersDead() || AreAllEnemiesDead())
                break;

            //si unit source est détruite, dead ou paralysé par l'attaque précedente on skip sa turn action
            if (battleAction.fromUnit == null || battleAction.fromUnit.GetComponent<BattleScript>().Dead)
                continue;

            ReassignTargetIfNeeded(battleAction);
            Debug.Log(battleAction);
            foreach (GameObject target in battleAction.targets.ToList())
            {
                target.GetComponent<BattleScript>().ApplyActionImpact(battleAction.fromUnit, battleAction.ability);
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
            yield return null;
        }
			
        if (!AreAllPlayersDead() && !AreAllEnemiesDead())
        {
            EventManager.TriggerEvent("applyEndTurnStatus");
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
        EventManager.TriggerEvent("win");

        while (!victoryAcknowledged)
        {
            yield return new WaitForSeconds(0.1f);
        }

        battleEnd = true;
    }


    IEnumerator Failure()
    {
        yield return null;
        EventManager.TriggerEvent("lose");

        while (!restartBattle && !backToMainMenu)
        {
            yield return new WaitForSeconds(0.1f);
        }

        battleEnd = true;
    }

    void ImpactHpMpAfterVictory()
    {
        //Impact the hp to the game party ...
        for (int i = 0; i < playerUnits.Count; i++)
        {
            if (playerUnits[i].GetComponent<BattleScript>().Character.GetStat(StatName.hpNow).baseValue == 0)
                Game.current.party[i].GetStat(StatName.hpNow).baseValue = 1;
            else
            {
                Game.current.party[i].GetStat(StatName.hpNow).baseValue = playerUnits[i].GetComponent<BattleScript>().Character.GetStat(StatName.hpNow).baseValue;
                Game.current.party[i].GetStat(StatName.mpNow).baseValue = playerUnits[i].GetComponent<BattleScript>().Character.GetStat(StatName.mpNow).baseValue;
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

}
