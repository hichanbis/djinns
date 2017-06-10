using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class BattleCamera : MonoBehaviour
{
    

    private BattleManager battleManager;
    private bool cutToSidePlayers = false;
    private bool cutBehindPlayers = false;
    private bool cutToLeftPlayer = false;
    private bool lookAtTarget = false;
    private bool lookAtPlayerZone = false;
    private bool lookAtMonsterZone = false;
    private bool lookAtActingUnit = false;
    private bool lookAtCenter = false;
    private bool lookAtEnemy = false;
    private bool lookAtPlayer = false;

    private UnityAction playerUnitsExistListener;
    private UnityAction playerChoiceExpectedListener;
    private UnityAction targetChoiceExpectedListener;
    private UnityAction targetChoiceAllPlayersListener;
    private UnityAction targetChoiceAllMonstersListener;
    private UnityAction actionChoicePhaseDoneListener;
    private UnityAction unitsLoadedListener;


    // EXPECTED BEHAVIOUR related to Battle MESSAGES

    // Start Camera default anim voir décor

    // INIT
    // Units Loaded - (Après init anim) cut droite players et lerp juska derriere PlayerZone pour voir ennemis

    // ACTIONCHOICE
    // Player Choice Expected - cut gauche player, lookAtPlayer
    // Target Choice Expected - cut devant target, lookAtTarget
    // Target Choice All Players - cut centre, lookAtPlayerZone
    // Target Choice All Monsters - cut centre, lookAtEnemyZone
    // Action Choice Phase Done - cut derriere les joueurs, lookAtEnemyZone

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



    }

    void MoveFromSideToBehindPlayers()
    {
        cutToSidePlayers = true;

        cutBehindPlayers = false;
        cutToLeftPlayer = false;
    }

    void CutBehindPlayers()
    {
        cutBehindPlayers = true;

        cutToSidePlayers = false;
        cutToLeftPlayer = false;
        LookAtCenter();
    }

    void CutLeftToPlayer()
    {
        cutToLeftPlayer = true;

        cutToSidePlayers = false;
        cutBehindPlayers = false;

        LookAtPlayer();
      
    }

    void LookAtCenter()
    {
        lookAtCenter = true;

        lookAtPlayer = false;
        lookAtTarget = false;
        lookAtEnemy = false;
        lookAtPlayerZone = false;
        lookAtMonsterZone = false;
        lookAtActingUnit = false;
    }

    void LookAtPlayer()
    {
        lookAtPlayer = true;

        lookAtTarget = false;
        lookAtEnemy = false;
        lookAtPlayerZone = false;
        lookAtMonsterZone = false;
        lookAtActingUnit = false;
        lookAtCenter = false;
    }

    void LookAtTarget()
    {
        lookAtTarget = true;

        lookAtEnemy = false;
        lookAtPlayer = false;
        lookAtPlayerZone = false;
        lookAtMonsterZone = false;
        lookAtActingUnit = false;
        lookAtCenter = false;
    }

    void LookAtPlayerZone()
    {
        lookAtPlayerZone = true;

        lookAtEnemy = false;
        lookAtPlayer = false;
        lookAtTarget = false;
        lookAtMonsterZone = false;
        lookAtActingUnit = false;
        lookAtCenter = false;
    }

    void LookAtMonsterZone()
    {
        lookAtMonsterZone = true;

        lookAtEnemy = false;
        lookAtPlayer = false;
        lookAtTarget = false;
        lookAtPlayerZone = false;
        lookAtActingUnit = false;
        lookAtCenter = false;
    }

    void LateUpdate()
    {
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
            transform.LookAt(new Vector3(0f, 0f, 0f));
        }

        if (lookAtTarget && battleManager.currentTargets != null && battleManager.currentTargets[0] != null)
        {
            transform.position = new Vector3(battleManager.currentTargets[0].transform.position.x, 2f, 0f);
            transform.LookAt(battleManager.currentTargets[0].transform);
        }
        else if (lookAtPlayerZone)
        {
            transform.position = new Vector3(0f, 2.5f, 2f);
            transform.LookAt(new Vector3(0f, 1f, -5f));
        }
        else if (lookAtMonsterZone)
        {
            transform.position = new Vector3(0f, 2.5f, -2f);
            transform.LookAt(new Vector3(0f, 1f, 5f));
        }
        else if (lookAtActingUnit && battleManager.currentActingUnit != null)
            transform.LookAt(battleManager.currentActingUnit.transform);
        else if (lookAtEnemy && battleManager.GetCurrentEnemy() != null)
            transform.LookAt(battleManager.GetCurrentEnemy().transform);
        else if (lookAtPlayer && battleManager.GetCurrentPlayer() != null)
        {
            float playerVerticalSize = battleManager.GetCurrentPlayer().GetComponent<CapsuleCollider>().height;
            float playerChestHeight = playerVerticalSize - 0.5f;

            transform.LookAt(battleManager.GetCurrentPlayer().transform.position + new Vector3(0f, playerChestHeight, 0.5f));
        }
        
        cutToLeftPlayer = false;

    }
}
