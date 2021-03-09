using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    private static BattleManager instance;

    public GameProgress gameProgress;
    public BattleUnits battleUnits;
    public StatusCollection statusCollection;
    public AbilityCollection abilityCollection;


    public BattleStates currentState;
    
    public BattleAction currentUnitAction;
    public List<BattleAction> turnActions = new List<BattleAction>();

    public bool backToMainMenu;
    public bool restartBattle;
    public bool victoryAcknowledged;
    private bool battleEnd;
    public bool targetImpactReached;
    private bool interrupt;

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

        if (FindObjectOfType(typeof(GameProgressSaver)) == null)
        {
            Debug.Log("No GameProgressSaver found, it is likely the persistent scene is unloaded so it is debug mode");
            StartCoroutine(LoadDebugPersistentScene());
            Debug.Log("Ok persistent scene loaded go debug");
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

        StartCoroutine(BattleStateMachine());
    }

    IEnumerator BattleStateMachine()
    {
        yield return null;


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
            sceneController.FadeAndLoadScene(gameProgress.currentScene);
        else if (backToMainMenu)
            sceneController.FadeAndLoadScene("MainMenu");
        else if (restartBattle)
            sceneController.FadeAndLoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator InitBattle()
    {
 
        

        yield return new WaitForSeconds(1f);

        EventManager.TriggerEvent(BattleEventMessages.InitBattle.ToString()); //starts anim rotation and taunt
        yield return new WaitForSeconds(3f);

        currentState = BattleStates.ActionChoice;
        yield return null;
    }

    

    IEnumerator ActionChoice()
    {
        yield return null;

        if (!currentState.Equals(BattleStates.ActionChoice))
            yield break;

        turnActions = new List<BattleAction>();
        battleUnits.SetTargetUnit(battleUnits.enemyUnits.Last<BattleScript>());
        battleUnits.targetUnits[0].GetComponent<Outline>().enabled = true;

        List<BattleScript> battlingUnits = GetAllBattleAbleUnits(); //should filter on dead and disabled

        for (int i = 0; i < battlingUnits.Count; i++)
        {
            battleUnits.currentChoosingUnit = battlingUnits[i];
            battleUnits.currentChoosingUnit.GetComponent<BattleScript>().anim.SetTrigger("Idle");
            currentUnitAction = new BattleAction();
            currentUnitAction.ability = null;
            if (IsGameObjectAPlayer(battleUnits.currentChoosingUnit))
            {
                StartCoroutine(battleUnits.currentChoosingUnit.GetComponent<BattleScript>().NewTurn());
                StartCoroutine(battleUnits.currentChoosingUnit.GetComponent<BattleScript>().LaunchChoiceAnim());
                currentUnitAction.fromUnit = battleUnits.currentChoosingUnit;
                EventManager.TriggerEvent(BattleEventMessages.PlayerChoiceExpected.ToString());
                bool choiceDone = false;
                while (!choiceDone)
                {
                    if (currentUnitAction.ability != null)
                    {
                        if (currentUnitAction.ability.targetType.Equals(TargetType.Self))
                        {
                            Debug.Log("choiceDone");
                            choiceDone = true;
                        }
                        else if (currentUnitAction.ability.id.Equals("Attack"))
                        {
                            choiceDone = true;
                        }
                        else if (currentUnitAction.targets != null)
                        {
                            choiceDone = true;
                        }
                    }
                    //launch anim for attack preparation here (guard or magic focus)!

                    yield return null;
                }

                if (currentUnitAction.ability.name.Equals("Guard"))
                    battleUnits.currentChoosingUnit.anim.SetTrigger("Guard");
                else if (currentUnitAction.ability.abilityType.Equals(AbilityType.Melee))
                    battleUnits.currentChoosingUnit.anim.SetTrigger("MeleeReady");
                else if (currentUnitAction.ability.abilityType.Equals(AbilityType.Magic))
                    battleUnits.currentChoosingUnit.anim.SetTrigger("MagicReady");


                turnActions.Add(currentUnitAction);

            }
            else if (!IsGameObjectAPlayer(battleUnits.currentChoosingUnit))
            {
                List<BattleScript> alivePlayerUnits = GetAlivePlayerUnits();
                if (alivePlayerUnits.Count > 0)
                {
                    BattleAction action = new BattleAction(battleUnits.currentChoosingUnit, new List<BattleScript>() { alivePlayerUnits[Random.Range(0, alivePlayerUnits.Count)] }, battleUnits.currentChoosingUnit.GetComponent<BattleScript>().character.GetAbility("Attack"));
                    turnActions.Add(action);
                    battleUnits.currentChoosingUnit.anim.SetTrigger("MeleeReady");
                }

            }

            yield return null;

        }

        EventManager.TriggerEvent(BattleEventMessages.ActionChoicePhaseDone.ToString());
        battleUnits.currentChoosingUnit = null;
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

            battleUnits.currentActingUnit = battleAction.fromUnit;

            //si unit source est d?truite, dead ou paralys? par l'attaque pr?cedente on skip sa turn action
            if (battleAction.fromUnit == null || battleAction.fromUnit.dead)
                continue;

            ReassignTargetIfNeeded(battleAction);

            //Debug.Log("avant launch");

            if (!battleAction.ability.name.Equals("Guard"))
            {
                targetImpactReached = false;
                StartCoroutine(battleAction.fromUnit.LaunchAbilityWithAnim(battleAction));

                //targetImpactReached est sett? par un animEvent
                while (!targetImpactReached)
                    yield return null;
            }

            //Debug.Log("avant dmg taken");

            yield return StartCoroutine(WaitForAllDamageToBeTaken(battleAction));
            yield return StartCoroutine(WaitForAllStatusToBeAdded(battleAction));

            if (!battleAction.ability.name.Equals("Guard"))
            {
                while (!battleAction.fromUnit.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    //Debug.Log("j'attends idle");
                    yield return null;
                }
            }

        }

        battleUnits.currentActingUnit = null;

        //Fin d'exec des battleActions

        if (!AreAllPlayersDead() && !AreAllEnemiesDead())
        {
            EventManager.TriggerEvent(BattleEventMessages.EndRageStatusTime.ToString());
            yield return StartCoroutine(WaitForAllEndRageStatusToBeApplied());

        }

        SetBattleStateAfterDamage();
        yield return null;
    }

    private IEnumerator WaitForAllDamageToBeTaken(BattleAction battleAction)
    {
        //battleManager calculates the damage and send it to targets who withstand the impact
        CoroutineJoin coroutineJoinTakeDamage = new CoroutineJoin(this);

        foreach (BattleScript target in battleAction.targets.ToList())
        {
            int dmg = CalculateDamage(battleAction.fromUnit, battleAction.ability, target);
            Debug.Log("CALCULATED DMG" + dmg);
            if (dmg != 0)
            {
                target.GetComponent<BattleScript>().character.stats.hpNow.SetValue(Mathf.Clamp(target.GetComponent<BattleScript>().character.stats.hpNow.GetValue() + dmg, 0, target.GetComponent<BattleScript>().character.stats.hp.GetValue()));
                //coroutineJoinTakeDamage.StartSubtask(target.GetComponent<BattleScript>().TakeDamage(dmg));
            }
        }
        //Wait for all takeDamage End
        yield return coroutineJoinTakeDamage.WaitForAll();
    }

    private IEnumerator WaitForAllStatusToBeAdded(BattleAction battleAction)
    {
        CoroutineJoin coroutineJoinStatuses = new CoroutineJoin(this);
        foreach (BattleScript target in battleAction.targets.ToList())
        {
            foreach (Status status in battleAction.ability.statuses.ToList<Status>())
            {
                Debug.Log(status);
                coroutineJoinStatuses.StartSubtask(target.GetComponent<BattleScript>().AddStatus(ScriptableObject.Instantiate<Status>(status)));
            }
        }
        yield return coroutineJoinStatuses.WaitForAll();
    }

    //To change with new turn call on acting unit
    private IEnumerator WaitForAllEndRageStatusToBeApplied()
    {

        CoroutineJoin coroutineJoinEndRage = new CoroutineJoin(this);
        foreach (BattleScript unit in GetAllUnits())
        {

        }

        yield return coroutineJoinEndRage.WaitForAll();
    }

    public void CancelTarget()
    {
        EventManager.TriggerEvent(BattleEventMessages.CancelTarget.ToString());
        EventManager.TriggerEvent(BattleEventMessages.PlayerChoiceExpected.ToString());
        if (currentUnitAction.ability.abilityType.Equals(AbilityType.Magic))
            EventManager.TriggerEvent(BattleEventMessages.DisplayMagicsPanel.ToString());
        currentUnitAction.ability = null;
    }

    public void CancelPreviousChoice()
    {

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

    private int CalculateDamage(BattleScript fromUnit, Ability ability, BattleScript target)
    {
        int rawDmg = 0;
        int dmg = 0;
        int multiplyingStat = 0;
        if (ability.abilityType.Equals(AbilityType.Magic))
            multiplyingStat = fromUnit.character.stats.intelligence.GetValue();
        else
            multiplyingStat = fromUnit.character.stats.strength.GetValue();
        rawDmg = ability.power * multiplyingStat * 6;
        if (ability.targetType.Equals(TargetType.Self) || ability.targetType.Equals(TargetType.Same) || ability.targetType.Equals(TargetType.AllSame))
            dmg = Mathf.CeilToInt(rawDmg / 10); // or reduce magic power....
        else
            //if opposite dmg = rawDmg with defense reduction
            dmg = Mathf.CeilToInt(rawDmg / (target.character.stats.defense.GetValue() * 2));
        return dmg;
    }

    private void ReassignTargetIfNeeded(BattleAction battleAction)
    {
        if (battleAction.ability.targetType.Equals(TargetType.Self))
            battleAction.targets = new List<BattleScript>() { battleAction.fromUnit };
        else if (battleAction.ability.targetType.Equals(TargetType.AllSame))
        {
            if (IsGameObjectAPlayer(battleAction.fromUnit))
                battleAction.targets = battleUnits.playerUnits;
            else
                battleAction.targets = battleUnits.enemyUnits;
        }
        else if (battleAction.ability.targetType.Equals(TargetType.AllOpposite))
        {
            if (IsGameObjectAPlayer(battleAction.fromUnit))
                battleAction.targets = battleUnits.enemyUnits;
            else
                battleAction.targets = battleUnits.playerUnits;
        }
        else if (battleAction.ability.targetType.Equals(TargetType.Opposite) && (battleAction.targets == null || battleAction.targets[0] == null || battleAction.targets[0].dead))
        {
            if (IsGameObjectAPlayer(battleAction.fromUnit))
                battleAction.targets = new List<BattleScript>() { battleUnits.enemyUnits.First() };
            else
                battleAction.targets = new List<BattleScript>() { GetAlivePlayerUnits().First() }; //should be random
        }
        else if (battleAction.ability.targetType.Equals(TargetType.Same) && (battleAction.targets == null || battleAction.targets[0] == null || battleAction.targets[0].GetComponent<BattleScript>().dead))
        {
            if (IsGameObjectAPlayer(battleAction.fromUnit))
            {
                if (!battleAction.ability.name.Equals("Revive"))
                    battleAction.targets = new List<BattleScript>() { GetAlivePlayerUnits().First() };

            }
            else
                battleAction.targets = new List<BattleScript>() { battleUnits.enemyUnits.First() };
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

        // + impact status que l'on veut conserver apr?s bataille (poison au minimum)

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
        for (int i = 0; i < battleUnits.playerUnits.Count; i++)
        {
            Character player = battleUnits.playerUnits[i].GetComponent<BattleScript>().character;


            if (player.stats.hpNow.GetValue() == 0)
                gameProgress.party[i].stats.hpNow.SetValue(1);
            else
            {
                gameProgress.party[i].stats.hpNow.SetValue(player.stats.hpNow.GetValue());
                gameProgress.party[i].stats.mpNow.SetValue(player.stats.mpNow.GetValue());
            }
        }

    }

    public bool IsGameObjectAPlayer(BattleScript unit)
    {
        return (System.Enum.IsDefined(typeof(PlayerName), unit.name));
    }

    public bool IsCurrentChoosingUnitAPlayer()
    {
        return (System.Enum.IsDefined(typeof(PlayerName), battleUnits.currentChoosingUnit.name));
    }

    bool AreAllPlayersDead()
    {
        int nbPlayersDead = 0;
        for (int i = 0; i < battleUnits.playerUnits.Count; i++)
        {
            if (battleUnits.playerUnits[i].GetComponent<BattleScript>().dead)
                nbPlayersDead++;
        }

        if (nbPlayersDead == battleUnits.playerUnits.Count)
            return true;
        else
            return false;

    }

    bool AreAllEnemiesDead()
    {
        return battleUnits.enemyUnits.Count == 0;
    }


    // Update is called once per frame
    void Update()
    {
    }

    public void SetCurrentTargetFromName(string targetName)
    {
        foreach (BattleScript target in battleUnits.targetUnits)
        {
            TargetCircle tc = target.GetComponent<TargetCircle>();
            if (tc)
                tc.HideCircle();
        }


        if (targetName.Equals("All Players"))
        {
            battleUnits.targetUnits = battleUnits.playerUnits;
            EventManager.TriggerEvent(BattleEventMessages.TargetChoiceAllPlayers.ToString());
        }
        else if (targetName.Equals("All Enemies"))
        {
            battleUnits.targetUnits = battleUnits.enemyUnits;
            EventManager.TriggerEvent(BattleEventMessages.TargetChoiceAllMonsters.ToString());
        }
        else
        {
            battleUnits.targetUnits = new List<BattleScript>() { GetAllUnits().Find(u => u.name.Equals(targetName)) };
            EventManager.TriggerEvent(BattleEventMessages.TargetChoiceExpected.ToString());
        }

        foreach (BattleScript target in battleUnits.targetUnits)
        {
            TargetCircle tc = target.GetComponent<TargetCircle>();
            if (tc)
                tc.DisplayCircle();
        }

    }

    public List<BattleScript> GetAlivePlayerUnits()
    {
        return battleUnits.playerUnits.Where(u => !u.GetComponent<BattleScript>().dead).ToList();
    }

    public List<BattleScript> GetAliveEnemyUnits()
    {
        return battleUnits.enemyUnits.Where(u => !u.GetComponent<BattleScript>().dead).ToList();
    }

    public List<BattleScript> GetAllUnits()
    {
        return battleUnits.playerUnits.Concat(battleUnits.enemyUnits).ToList();
    }

    public List<BattleScript> GetAllBattleAbleUnits()
    {
        return battleUnits.playerUnits.Concat(battleUnits.enemyUnits).ToList().Where(u => !u.GetComponent<BattleScript>().dead && u.GetComponent<BattleScript>().canAct).ToList();

    }
}
