using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    private static BattleManager instance;

    public GameProgress gameProgress;
    public StatusCollection statusCollection;
    public AbilityCollection abilityCollection;

    public BattleStates currentState;
    public List<GameObject> playerUnits = new List<GameObject>();
    public List<GameObject> monsterUnits = new List<GameObject>();

    public GameObject currentChoosingUnit;
    public GameObject currentActingUnit;
    public List<GameObject> currentTargets;
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

        if (FindObjectOfType(typeof(GameManager)) == null)
        {
            Debug.Log("No GameManager found, it is likely the persistent scene is unloaded so it is debug mode");
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


        currentChoosingUnit = null;
        currentActingUnit = null;

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
        playerUnits = InstantiatePlayerParty();
        monsterUnits = InstantiateMonsterParty();
        currentTargets = new List<GameObject>();

        yield return new WaitForSeconds(1f);

        EventManager.TriggerEvent(BattleEventMessages.InitBattle.ToString()); //starts anim rotation and taunt
        yield return new WaitForSeconds(3f);

        currentState = BattleStates.ActionChoice;
        yield return null;
    }

    private List<GameObject> InstantiatePlayerParty()
    {
        List<GameObject> players = new List<GameObject>();
        int nbPlayers = gameProgress.party.Count;
        if (nbPlayers > 3)
            nbPlayers = 3;
        float spaceBetweenPlayers = 3.5f;
        float xPos = -spaceBetweenPlayers / 2 * (nbPlayers - 1);
        float zPos = -5f;

        Debug.Log(nbPlayers);

        for (int i = 0; i < nbPlayers; i++)
        {
            
            Character character = gameProgress.party[i];
            Vector3 spawnPosition = new Vector3(xPos, 0f, zPos);
            Quaternion rotation = Quaternion.LookRotation(new Vector3(xPos, 0, 0) - spawnPosition);
            GameObject unitPlayer = Instantiate(Resources.Load("Player") as GameObject, spawnPosition, rotation) as GameObject;
            unitPlayer.GetComponent<Movement>().enabled = false;
            unitPlayer.GetComponent<Cinemachine.Examples.CharacterMovement>().enabled = false;
            unitPlayer.GetComponent<AttackOtherOnCollide>().enabled = false;

            unitPlayer.name = character.name;
            unitPlayer.GetComponent<BattleScript>().character = ObjectCopier.Clone<Character>(character);
            unitPlayer.GetComponent<BattleScript>().enabled = true;
           
            players.Add(unitPlayer);

            xPos += spaceBetweenPlayers;
        }
        return players;
    }

    private List<GameObject> InstantiateMonsterParty()
    {
        List<GameObject> enemies = new List<GameObject>();
        int nbEnemies = Random.Range(2, 5);
        //int nbEnemies = 1;
        float spaceBetweenEnemies = 4;
        float xPos = -spaceBetweenEnemies / 2 * (nbEnemies - 1);
        float zPos = 5f;

        for (int i = 0; i < nbEnemies; i++)
        {
            Vector3 spawnPosition = new Vector3(xPos, 0f, zPos);
            Quaternion rotation = Quaternion.LookRotation(new Vector3(xPos, 0, 0) - spawnPosition);
            GameObject enemy = Instantiate(Resources.Load("Enemy") as GameObject, spawnPosition, rotation) as GameObject;
            enemy.GetComponent<AttackOtherOnCollide>().enabled = false;
            enemy.name = "Enemy" + i;
            List<Ability> basicAbs = new List<Ability>();
            basicAbs.Add(abilityCollection.GetAbilityFromId("Attack"));
            Stat hp = new Stat(100);
            Stat hpNow = new Stat(100);
            Stat mp = new Stat(35);
            Stat mpNow = new Stat(35);
            Stat strength = new Stat(20);
            Stat defense = new Stat(10);
            Stat intelligence = new Stat(10);
            Stat agility = new Stat(10);
            Stats defaultStats = new Stats(hp, hpNow, mp, mpNow, strength, defense, intelligence, agility);
            Character character = new Character(enemy.name, Element.Fire, basicAbs, defaultStats, false);
            enemy.GetComponent<BattleScript>().character = character;
            enemies.Add(enemy);
            xPos += spaceBetweenEnemies;
        }
        return enemies;
    }

    IEnumerator ActionChoice()
    {
        yield return null;

        if (!currentState.Equals(BattleStates.ActionChoice))
            yield break;

        turnActions = new List<BattleAction>();
        currentTargets = new List<GameObject>();
        List<GameObject> battlingUnits = GetAllBattleAbleUnits(); //should filter on dead and disabled

        for (int i = 0; i < battlingUnits.Count; i++)
        {
            currentChoosingUnit = battlingUnits[i];
            currentChoosingUnit.GetComponent<BattleScript>().anim.SetTrigger("Idle");
            currentUnitAction = new BattleAction();
            currentUnitAction.ability = null;
            if (IsGameObjectAPlayer(currentChoosingUnit))
            {
                StartCoroutine(currentChoosingUnit.GetComponent<BattleScript>().LaunchChoiceAnim());
                currentUnitAction.fromUnit = currentChoosingUnit;
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
                        else if (currentUnitAction.targets != null)
                        {
                            choiceDone = true;
                        }
                    }
                    //launch anim for attack preparation here (guard or magic focus)!

                    yield return null;
                }

                if (currentUnitAction.ability.id.Equals("Guard"))
                    currentChoosingUnit.GetComponent<BattleScript>().anim.SetTrigger("Guard");
                else if (currentUnitAction.ability.abilityType.Equals(AbilityType.Melee))
                    currentChoosingUnit.GetComponent<BattleScript>().anim.SetTrigger("MeleeReady");
                else if (currentUnitAction.ability.abilityType.Equals(AbilityType.Magic))
                    currentChoosingUnit.GetComponent<BattleScript>().anim.SetTrigger("MagicReady");
                

                turnActions.Add(currentUnitAction);

            }
            else if (!IsGameObjectAPlayer(currentChoosingUnit))
            {
                List<GameObject> alivePlayerUnits = GetAlivePlayerUnits();
                if (alivePlayerUnits.Count > 0)
                {
                    BattleAction action = new BattleAction(currentChoosingUnit, new List<GameObject>() { alivePlayerUnits[Random.Range(0, alivePlayerUnits.Count)] }, currentChoosingUnit.GetComponent<BattleScript>().character.GetAbility("Attack"));
                    turnActions.Add(action);
                    currentChoosingUnit.GetComponent<BattleScript>().anim.SetTrigger("MeleeReady");
                }

            }

            yield return null;

        }

        EventManager.TriggerEvent(BattleEventMessages.ActionChoicePhaseDone.ToString());
        currentChoosingUnit = null;
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

            currentActingUnit = battleAction.fromUnit;

            //si unit source est d�truite, dead ou paralys� par l'attaque pr�cedente on skip sa turn action
            if (battleAction.fromUnit == null || battleAction.fromUnit.GetComponent<BattleScript>().dead)
                continue;

            ReassignTargetIfNeeded(battleAction);

            Debug.Log("avant launch");

            if (!battleAction.ability.id.Equals("Guard"))
            {
                targetImpactReached = false;
                StartCoroutine(battleAction.fromUnit.GetComponent<BattleScript>().LaunchAbilityWithAnim(battleAction));
           
                //targetImpactReached est sett� par un animEvent
                while (!targetImpactReached)
                    yield return null;
            }

            Debug.Log("avant dmg taken");

            yield return StartCoroutine(WaitForAllDamageToBeTaken(battleAction));
            yield return StartCoroutine(WaitForAllStatusToBeAdded(battleAction));
        
            if (!battleAction.ability.id.Equals("Guard"))
            {
                while (!battleAction.fromUnit.GetComponent<BattleScript>().anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    Debug.Log("j'attends idle");
                    yield return null;
                }
            }

        }

        currentActingUnit = null;

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

        foreach (GameObject target in battleAction.targets.ToList())
        {
            int dmg = CalculateDamage(battleAction.fromUnit, battleAction.ability, target);

            //if there are dmg! if poison or guard c un autre delire
            if (dmg != 0)
                coroutineJoinTakeDamage.StartSubtask(target.GetComponent<BattleScript>().TakeDamage(dmg));
        }
        //Wait for all takeDamage End
        yield return coroutineJoinTakeDamage.WaitForAll();
    }

    private IEnumerator WaitForAllStatusToBeAdded(BattleAction battleAction)
    {
        CoroutineJoin coroutineJoinStatuses = new CoroutineJoin(this);
        foreach (GameObject target in battleAction.targets.ToList())
        {
            foreach (string statusName in battleAction.ability.statusIds)
            {
                Debug.Log(statusName);
                Status status = statusCollection.FindStatusFromId(statusName);
                if (target.GetComponent<BattleScript>().CanAddStatus(status))
                    coroutineJoinStatuses.StartSubtask(target.GetComponent<BattleScript>().AddStatus(status));
            }
        }
        yield return coroutineJoinStatuses.WaitForAll();
    }

    private IEnumerator WaitForAllEndRageStatusToBeApplied()
    {
        
        CoroutineJoin coroutineJoinEndRage = new CoroutineJoin(this);
        foreach (GameObject unit in GetAllUnits())
        {
            coroutineJoinEndRage.StartSubtask(unit.GetComponent<BattleScript>().ApplyEndRageStatusEffects());
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

    private int CalculateDamage(GameObject fromUnit, Ability ability, GameObject target)
    {
        int rawDmg = 0;
        int dmg = 0;
        int multiplyingStat = 0;
        if (ability.abilityType.Equals(AbilityType.Magic))
            multiplyingStat = fromUnit.GetComponent<BattleScript>().character.stats.intelligence.GetValue();
        else
            multiplyingStat = fromUnit.GetComponent<BattleScript>().character.stats.strength.GetValue();
        rawDmg = ability.power * multiplyingStat * 6;
        if (ability.targetType.Equals(TargetType.Self) || ability.targetType.Equals(TargetType.Same) || ability.targetType.Equals(TargetType.AllSame))
            dmg = Mathf.CeilToInt(rawDmg / 10); // or reduce magic power....
        else
            //if opposite dmg = rawDmg with defense reduction
            dmg = Mathf.CeilToInt(rawDmg / (target.GetComponent<BattleScript>().character.stats.defense.GetValue() * 2));
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
                battleAction.targets = new List<GameObject>() { GetAlivePlayerUnits().First() }; //should be random
        }
        else if (battleAction.ability.targetType.Equals(TargetType.Same) && (battleAction.targets == null || battleAction.targets[0] == null || battleAction.targets[0].GetComponent<BattleScript>().dead))
        {
            if (IsGameObjectAPlayer(battleAction.fromUnit))
            {
                if (!battleAction.ability.id.Equals("Revive"))
                    battleAction.targets = new List<GameObject>() { GetAlivePlayerUnits().First() };

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

        // + impact status que l'on veut conserver apr�s bataille (poison au minimum)

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


            if (player.stats.hpNow.baseValue == 0)
                gameProgress.party[i].stats.hpNow.baseValue = 1;
            else
            {
                gameProgress.party[i].stats.hpNow.baseValue = player.stats.hpNow.baseValue;
                gameProgress.party[i].stats.mpNow.baseValue = player.stats.mpNow.baseValue;
            }
        }

    }

    public bool IsGameObjectAPlayer(GameObject unit)
    {
        return (System.Enum.IsDefined(typeof(PlayerName), unit.name));
    }

    public bool IsCurrentChoosingUnitAPlayer()
    {
        return (System.Enum.IsDefined(typeof(PlayerName), currentChoosingUnit.name));
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

    public void SetCurrentTargetFromName(string targetName)
    {
        foreach (GameObject target in currentTargets)
        {
            TargetCircle tc = target.GetComponent<TargetCircle>();
            if (tc)
                tc.HideCircle();
        }


        if (targetName.Equals("All Players"))
        {
            currentTargets = playerUnits;
            EventManager.TriggerEvent(BattleEventMessages.TargetChoiceAllPlayers.ToString());
        }
        else if (targetName.Equals("All Enemies"))
        {
            currentTargets = monsterUnits;
            EventManager.TriggerEvent(BattleEventMessages.TargetChoiceAllMonsters.ToString());
        }
        else
        {
            currentTargets = new List<GameObject>() { GetAllUnits().Find(u => u.name.Equals(targetName)) };
            EventManager.TriggerEvent(BattleEventMessages.TargetChoiceExpected.ToString());
        }

        foreach (GameObject target in currentTargets)
        {
            TargetCircle tc = target.GetComponent<TargetCircle>();
            if (tc)
                tc.DisplayCircle();
        }

    }

    public List<GameObject> GetAlivePlayerUnits()
    {
        return playerUnits.Where(u => !u.GetComponent<BattleScript>().dead).ToList();
    }

    public List<GameObject> GetAliveMonsterUnits()
    {
        return monsterUnits.Where(u => !u.GetComponent<BattleScript>().dead).ToList();
    }

    public List<GameObject> GetAllUnits()
    {
        return playerUnits.Concat(monsterUnits).ToList();
    }

    public List<GameObject> GetAllBattleAbleUnits()
    {
        return playerUnits.Concat(monsterUnits).ToList().Where(u => !u.GetComponent<BattleScript>().dead && u.GetComponent<BattleScript>().canAct).ToList();

    }
}
