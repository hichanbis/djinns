using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;


public enum LookAtType
{
    lookAtCenter,
    lookAtPlayer,
    lookAtTarget,
    lookAtEnemy,
    lookAtPlayerZone,
    lookAtMonsterZone,
    lookAtActingUnit
}


public class BattleCamera : MonoBehaviour
{
    

    private BattleManager battleManager;
    private LookAtType currentLookAt;
    private bool cutInFrontPlayers = false;
    private bool cutToSidePlayers = false;
    private bool cutBehindPlayers = false;
    private bool cutToLeftPlayer = false;
   
    private UnityAction playerUnitsExistListener;
    private UnityAction playerChoiceExpectedListener;
    private UnityAction targetChoiceExpectedListener;
    private UnityAction targetChoiceAllPlayersListener;
    private UnityAction targetChoiceAllMonstersListener;
    private UnityAction actionChoicePhaseDoneListener;
    private UnityAction unitsLoadedListener;
    private UnityAction victoryListener;
    private UnityAction failureListener;


    // BEHAVIOUR related to Battle MESSAGES

    // INIT
    // Units Loaded - (Après init anim) cut droite players et lerp juska derriere PlayerZone pour voir ennemis

    // ACTIONCHOICE
    // Player Choice Expected - cut gauche player, lookAtPlayer
    // Target Choice Expected - cut devant target, lookAtTarget
    // Target Choice All Players - cut centre, lookAtPlayerZone
    // Target Choice All Monsters - cut centre, lookAtEnemyZone
    // Action Choice Phase Done - cut derriere les joueurs, lookAtCenter

    // RAGE
    // Melee Player Run - Cut derriere player dans la direction Player -> Enemy, lookAtTarget, follow
    // Melee Player Attack -  look atTarget, Stop follow player
    // Melee Enemy Run - Cut devant enemy dans la direction Enemy -> Player, lookAtActing, follow
    // Melee Enemy Attack - Cut droite ennemi, lookatTarget

    // Magic attack - Cut devant acting lookAtActing
    // Magic attack launched Single - cut devant target
    // Magic attack launched All - cut devant targets, lookAt Centre TargetZone

    // End Rage - Show all units

    // VICTORY OU FAILURE
    // Battle Ended - cut devant players, lookAt Centre PlayerZone

    // Use this for initialization
    void Start()
    {
        battleManager = BattleManager.Instance;

        unitsLoadedListener = new UnityAction(MoveFromSideToBehindPlayers);
        EventManager.StartListening(BattleEventMessages.UnitsLoaded.ToString(), unitsLoadedListener);

        playerChoiceExpectedListener = new UnityAction(CutLeftToPlayer);
        EventManager.StartListening(BattleEventMessages.PlayerChoiceExpected.ToString(), playerChoiceExpectedListener);

        targetChoiceExpectedListener = new UnityAction(LookAtTarget);
        EventManager.StartListening(BattleEventMessages.TargetChoiceExpected.ToString(), targetChoiceExpectedListener);

        targetChoiceAllPlayersListener = new UnityAction(LookAtPlayerZone);
        EventManager.StartListening(BattleEventMessages.TargetChoiceAllPlayers.ToString(), targetChoiceAllPlayersListener);

        targetChoiceAllMonstersListener = new UnityAction(LookAtMonsterZone);
        EventManager.StartListening(BattleEventMessages.TargetChoiceAllMonsters.ToString(), targetChoiceAllMonstersListener);

        actionChoicePhaseDoneListener = new UnityAction(CutBehindPlayers);
        EventManager.StartListening(BattleEventMessages.ActionChoicePhaseDone.ToString(), actionChoicePhaseDoneListener);

        victoryListener = new UnityAction(CutInFrontPlayers);
        EventManager.StartListening(BattleEventMessages.Victory.ToString(), victoryListener);

        failureListener = new UnityAction(CutInFrontPlayers);
        EventManager.StartListening(BattleEventMessages.Failure.ToString(), failureListener);


    }

    void MoveFromSideToBehindPlayers()
    {
        cutToSidePlayers = true;

        cutInFrontPlayers = false;
        cutBehindPlayers = false;
        cutToLeftPlayer = false;
    }

    void CutInFrontPlayers()
    {
        cutInFrontPlayers = true;

        cutBehindPlayers = false;
        cutToSidePlayers = false;
        cutToLeftPlayer = false;
        currentLookAt = LookAtType.lookAtPlayerZone;
    }

    void CutBehindPlayers()
    {
        cutBehindPlayers = true;

        cutInFrontPlayers = false;
        cutToSidePlayers = false;
        cutToLeftPlayer = false;
        currentLookAt = LookAtType.lookAtCenter;
    }

    void CutLeftToPlayer()
    {
        cutToLeftPlayer = true;

        cutInFrontPlayers = false;
        cutToSidePlayers = false;
        cutBehindPlayers = false;
        currentLookAt = LookAtType.lookAtPlayer;
      
    }

    void LookAtCenter()
    {
        currentLookAt = LookAtType.lookAtCenter;
    }

    void LookAtPlayer()
    {
        currentLookAt = LookAtType.lookAtPlayer;
    }

    void LookAtTarget()
    {
        currentLookAt = LookAtType.lookAtTarget;
    }

    void LookAtPlayerZone()
    {
        currentLookAt = LookAtType.lookAtPlayerZone;
    }

    void LookAtMonsterZone()
    {
        currentLookAt = LookAtType.lookAtMonsterZone;
    }

    void LateUpdate()
    {

        if (cutInFrontPlayers)
        {
            transform.position = new Vector3(-2f, 3.5f, -10f);
        }

        if (cutToLeftPlayer && battleManager.currentActingUnit != null)
        {
            Vector3 playerPos = battleManager.GetCurrentPlayer().transform.position;
            float playerVerticalSize = battleManager.GetCurrentPlayer().GetComponent<CapsuleCollider>().height;
            float playerChestHeight = playerVerticalSize - 0.5f;

            float leftPlayerXOffset = -1f;
            float leftPlayerYOffset = playerChestHeight;
            float leftPlayerZOffset = 0.5f;

            Vector3 offset = new Vector3(leftPlayerXOffset, leftPlayerYOffset, leftPlayerZOffset);
            transform.position = playerPos + offset;
        }

        if (cutBehindPlayers)
        {
            transform.position = new Vector3(-2f, 3.5f, -10f);

        }

        if (currentLookAt.Equals(LookAtType.lookAtTarget) && battleManager.currentTargets != null && battleManager.currentTargets[0] != null)
        {
            transform.position = new Vector3(battleManager.currentTargets[0].transform.position.x, 2f, 0f);
            transform.LookAt(battleManager.currentTargets[0].transform);
        }
        else if (currentLookAt.Equals(LookAtType.lookAtPlayerZone))
        {
            transform.position = new Vector3(0f, 2.5f, 2f);
            transform.LookAt(new Vector3(0f, 1f, -5f));
        }
        else if (currentLookAt.Equals(LookAtType.lookAtMonsterZone))
        {
            transform.position = new Vector3(0f, 2.5f, -2f);
            transform.LookAt(new Vector3(0f, 1f, 5f));
        }
        else if (currentLookAt.Equals(LookAtType.lookAtActingUnit) && battleManager.currentActingUnit != null)
            transform.LookAt(battleManager.currentActingUnit.transform);
        else if (currentLookAt.Equals(LookAtType.lookAtEnemy) && battleManager.GetCurrentEnemy() != null)
            transform.LookAt(battleManager.GetCurrentEnemy().transform);
        else if (currentLookAt.Equals(LookAtType.lookAtPlayer) && battleManager.GetCurrentPlayer() != null)
        {
            float playerVerticalSize = battleManager.GetCurrentPlayer().GetComponent<CapsuleCollider>().height;
            float playerChestHeight = playerVerticalSize - 0.5f;

            transform.LookAt(battleManager.GetCurrentPlayer().transform.position + new Vector3(0f, playerChestHeight, 0.5f));
        }
        else if (currentLookAt.Equals(LookAtType.lookAtCenter))
            transform.LookAt(new Vector3(0f, 0f, 0f));
        
        cutToLeftPlayer = false;

    }
}
