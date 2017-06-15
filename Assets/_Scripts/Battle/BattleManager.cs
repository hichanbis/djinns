using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    private static BattleManager instance;

    public BattleStates currentState;
    public List<GameObject> playerUnits = new List<GameObject>();
    public List<GameObject> monsterUnits = new List<GameObject>();

    public GameObject currentActingUnit;
    public List<GameObject> currentTargets;
    public BattleAction currentUnitAction;
    public List<BattleAction> turnActions = new List<BattleAction>();
    public int currentPlayerIndex = 0;
    public int currentEnemyIndex = 0;
    public bool backToMainMenu;
    public bool restartBattle;
    public bool chosen;
    public bool victoryAcknowledged;
    private bool battleEnd;
    public bool targetImpactReached;

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
            SceneManager.LoadSceneAsync("Persistent", LoadSceneMode.Additive);
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
        currentTargets = new List<GameObject>(){ monsterUnits[0] };

        StartCoroutine(BattleStateMachine());
    }

    IEnumerator BattleStateMachine()
    {
        yield return null;

        //init UI and rotate anim and wait for it to finish
        EventManager.TriggerEvent(BattleEventMessages.UnitsLoaded.ToString());


        battleEnd = false;
        restartBattle = false;
        backToMainMenu = false;
        victoryAcknowledged = false;

        currentState = BattleStates.InitBattle;

        while (!battleEnd)
        {
            yield return StartCoroutine(currentState.ToString());

        }

        EventManager.TriggerEvent(BattleEventMessages.BattleEnded.ToString());
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
        //yield return new WaitForSeconds(3f);
        //EventManager.TriggerEvent(BattleEventMessages.Taunt.ToString());

        //wait for rotate and taunt to finish before action choice
        yield return new WaitForSeconds(1f);

        currentState = BattleStates.ActionChoice;
        yield return null;
    }


    IEnumerator ActionChoice()
    {
        yield return null;

        if (!currentState.Equals(BattleStates.ActionChoice))
            yield break;

        turnActions = new List<BattleAction>();
        List<GameObject> battlingUnits = GetAllUnits(); //should filter on dead and disabled

        for (int i = 0; i < battlingUnits.Count; i++)
        {
            currentActingUnit = battlingUnits[i];
            currentUnitAction = new BattleAction();
            if (IsGameObjectAPlayer(currentActingUnit) && !currentActingUnit.GetComponent<BattleScript>().dead)
            {
                currentUnitAction.fromUnit = currentActingUnit;
                EventManager.TriggerEvent(BattleEventMessages.PlayerChoiceExpected.ToString());
                bool choiceDone = false;
                while (!choiceDone)
                {
                    if (currentUnitAction.ability != null)
                    {
                        if (currentUnitAction.ability.targetType.Equals(TargetType.Self))
                            choiceDone = true;
                        else if (currentUnitAction.targets != null)
                            choiceDone = true;
                    }

                    //launch anim for attack preparation here (guard or magic focus)!

                    yield return null;
                }
                turnActions.Add(currentUnitAction);
                IncrementPlayerIndex();
            }
            else if (!IsGameObjectAPlayer(currentActingUnit))
            {
                List<GameObject> alivePlayerUnits = playerUnits.Where(p => !p.GetComponent<BattleScript>().dead).ToList();
                if (alivePlayerUnits.Count > 0)
                {
                    BattleAction action = new BattleAction(currentActingUnit, new List<GameObject>() { alivePlayerUnits[Random.Range(0, alivePlayerUnits.Count)] }, currentActingUnit.GetComponent<BattleScript>().character.getAbility("Attack"));
                    turnActions.Add(action);
                }
                IncrementEnemyIndex();
            }

            yield return null;
        }
        EventManager.TriggerEvent(BattleEventMessages.ActionChoicePhaseDone.ToString());
        yield return new WaitForSeconds(1f);
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
            if (battleAction.fromUnit == null || battleAction.fromUnit.GetComponent<BattleScript>().dead)
                continue;

            ReassignTargetIfNeeded(battleAction);

            Vector3 initPos = battleAction.fromUnit.transform.position;
            Quaternion initRot = battleAction.fromUnit.transform.rotation;
            if (battleAction.ability.distance.Equals(Distance.Close))
                yield return StartCoroutine(battleAction.fromUnit.GetComponent<BattleScript>().RunToTarget(battleAction.targets[0]));

            targetImpactReached = false;

            //ne pas se mettre en attente car ensuite on attend l'anim event qui set targetImpactReached a true
            StartCoroutine(battleAction.fromUnit.GetComponent<BattleScript>().LaunchAbilityAnim(battleAction.ability.id));

            //battleManager calculates the damage and send it to targets who withstand the impact
            CoroutineJoin coroutineJoinTakeDamage = new CoroutineJoin(this);
            foreach (GameObject target in battleAction.targets.ToList())
            {
                int dmg = CalculateDamage(battleAction.fromUnit, battleAction.ability, target);

                //targetImpactReached est setté par un animEvent
                while (!targetImpactReached)
                    yield return null;

                //if there are dmg! if poison or guard c un autre delire
                target.GetComponent<BattleScript>().damageTaken = false;
                coroutineJoinTakeDamage.StartSubtask(target.GetComponent<BattleScript>().TakeDamage(dmg));
            }

            battleAction.fromUnit.GetComponent<BattleScript>().removeMp(battleAction.ability);

            //Wait for all takeDamage End
            yield return coroutineJoinTakeDamage.WaitForAll();

            CoroutineJoin coroutineJoinStatuses = new CoroutineJoin(this);
            foreach (GameObject target in battleAction.targets.ToList())
            {
                foreach (Status status in battleAction.ability.statuses)
                {
                    coroutineJoinTakeDamage.StartSubtask(target.GetComponent<BattleScript>().TryAddStatus(status));
                }

                yield return null;
                target.GetComponent<BattleScript>().DestroyIfDead();

            }
            yield return coroutineJoinStatuses.WaitForAll();
        

            //wait for attack anim to finish before moving on
            //voir si on peut pas utiliser normalizedTime < 1
            while (battleAction.fromUnit.GetComponent<BattleScript>().anim.GetCurrentAnimatorStateInfo(0).IsName(battleAction.ability.id))
            {
                yield return null;
            }

            if (battleAction.ability.distance.Equals(Distance.Close))
                battleAction.fromUnit.transform.position = initPos;
            
            battleAction.fromUnit.transform.rotation = initRot;

        }

        //Fin d'exec de la battleAction
    



        if (!AreAllPlayersDead() && !AreAllEnemiesDead())
        {
            //will reinit DoneApplied for each script
            // à remplacer par une joint coroutine wait for all
            EventManager.TriggerEvent(BattleEventMessages.EndRageStatusTime.ToString());

            CoroutineJoin coroutineJoinEndRage = new CoroutineJoin(this);
            foreach (GameObject unit in GetAllUnits())
            {
                coroutineJoinEndRage.StartSubtask(unit.GetComponent<BattleScript>().ApplyEndRageStatusEffects());
            }
      
            coroutineJoinEndRage.WaitForAll();
            yield return new WaitForSeconds(5f);
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

    private int CalculateDamage(GameObject fromUnit, Ability ability, GameObject target)
    {
        int rawDmg = 0;
        int dmg = 0;
        int multiplyingStat = 0;
        if (ability.abilityType.Equals(AbilityType.Magic))
            multiplyingStat = fromUnit.GetComponent<BattleScript>().character.GetStat(StatName.intelligence).GetValue();
        else
            multiplyingStat = fromUnit.GetComponent<BattleScript>().character.GetStat(StatName.strength).GetValue();
        rawDmg = ability.power * multiplyingStat * 6;
        if (ability.targetType.Equals(TargetType.Self) || ability.targetType.Equals(TargetType.Same) || ability.targetType.Equals(TargetType.AllSame))
            dmg = Mathf.CeilToInt(rawDmg / 10); // or reduce magic power....
        else
            //if opposite dmg = rawDmg with defense reduction
            dmg = Mathf.CeilToInt(rawDmg / (target.GetComponent<BattleScript>().character.GetStat(StatName.defense).GetValue() * 2));
        return dmg;
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
        else if (battleAction.ability.targetType.Equals(TargetType.Opposite) && (battleAction.targets == null || battleAction.targets[0] == null || battleAction.targets[0].GetComponent<BattleScript>().dead))
        {
            if (IsGameObjectAPlayer(battleAction.fromUnit))
                battleAction.targets = new List<GameObject>() { monsterUnits.First() };
            else
                battleAction.targets = new List<GameObject>() { playerUnits.Where(p => !p.GetComponent<BattleScript>().dead).ToList().First() }; //should be random
        }
        else if (battleAction.ability.targetType.Equals(TargetType.Same) && (battleAction.targets == null || battleAction.targets[0] == null || battleAction.targets[0].GetComponent<BattleScript>().dead))
        {
            if (IsGameObjectAPlayer(battleAction.fromUnit))
            {
                if (!battleAction.ability.id.Equals("Revive"))
                    battleAction.targets = new List<GameObject>() { playerUnits.Where(p => !p.GetComponent<BattleScript>().dead).ToList().First() };

            }
            else
                battleAction.targets = new List<GameObject>() { monsterUnits.First() };
        }
    }

    IEnumerator Victory()
    {
        yield return null;
        EventManager.TriggerEvent(BattleEventMessages.Victory.ToString());

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
        EventManager.TriggerEvent(BattleEventMessages.Failure.ToString());

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
            Character player = playerUnits[i].GetComponent<BattleScript>().character;


            if (player.GetStat(StatName.hpNow).baseValue == 0)
                Game.current.party[i].GetStat(StatName.hpNow).baseValue = 1;
            else
            {
                Game.current.party[i].GetStat(StatName.hpNow).baseValue = player.GetStat(StatName.hpNow).baseValue;
                Game.current.party[i].GetStat(StatName.mpNow).baseValue = player.GetStat(StatName.mpNow).baseValue;
            }
        }

    }

    public bool IsGameObjectAPlayer(GameObject unit)
    {
        return (System.Enum.IsDefined(typeof(PlayerName), unit.name));
    }

    public bool IsCurrentActingUnitAPlayer()
    {
        return (System.Enum.IsDefined(typeof(PlayerName), currentActingUnit.name));
    }

    bool AreAllPlayersDead()
    {
        int nbPlayersDead = 0;
        for (int i = 0; i < playerUnits.Count; i++)
        {
            if (playerUnits[i].GetComponent<BattleScript>().dead)
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
        if (!targetName.Equals("All"))
            currentTargets = new List<GameObject>() { GetAllUnits().Find(u => u.name.Equals(targetName)) };
        else if (targetName.Equals("All Players"))
            currentTargets = playerUnits;
        else if (targetName.Equals("All Enemies"))
            currentTargets = monsterUnits;
    }

    public List<GameObject> GetAllUnits()
    {
        return playerUnits.Concat(monsterUnits).ToList();
    }
}
