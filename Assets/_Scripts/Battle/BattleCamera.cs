using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;


public enum LookAtType
{
    none,
    lookAtCenter,
    lookAtPlayer,
    lookAtTarget,
    lookAtEnemy,
    lookAtPlayerZone,
    lookAtMonsterZone,
    lookAtActingUnit
}

public enum LerpType
{
    none,
    InFrontPlayers,
    InFrontMonsters,
    InFrontOfTarget,
    SideOfPlayersToBehind,
    BehindPlayers,
    LeftOfPlayer
}


public class BattleCamera : MonoBehaviour
{
    

    private BattleManager battleManager;
    private LookAtType currentLookAt;
    private LerpType lerpBehaviour;
  
   
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
        //transform.position = new Vector3(20f, 2f, 0f);
        lerpBehaviour = LerpType.none;
        currentLookAt = LookAtType.none;

        battleManager = BattleManager.Instance;

        unitsLoadedListener = new UnityAction(MoveFromSideToBehindPlayers);
        EventManager.StartListening(BattleEventMessages.InitBattle.ToString(), unitsLoadedListener);

        playerChoiceExpectedListener = new UnityAction(CutLeftToPlayerLookAtPlayer);
        EventManager.StartListening(BattleEventMessages.PlayerChoiceExpected.ToString(), playerChoiceExpectedListener);

        targetChoiceExpectedListener = new UnityAction(CutInFrontOfTargetLookAtTarget);
        EventManager.StartListening(BattleEventMessages.TargetChoiceExpected.ToString(), targetChoiceExpectedListener);

        targetChoiceAllPlayersListener = new UnityAction(CutAndLookAtPlayerZone);
        EventManager.StartListening(BattleEventMessages.TargetChoiceAllPlayers.ToString(), targetChoiceAllPlayersListener);

        targetChoiceAllMonstersListener = new UnityAction(CutAndLookAtMonsterZone);
        EventManager.StartListening(BattleEventMessages.TargetChoiceAllMonsters.ToString(), targetChoiceAllMonstersListener);

        actionChoicePhaseDoneListener = new UnityAction(CutBehindPlayersLookAtCenter);
        EventManager.StartListening(BattleEventMessages.ActionChoicePhaseDone.ToString(), actionChoicePhaseDoneListener);

        victoryListener = new UnityAction(CutInFrontPlayers);
        EventManager.StartListening(BattleEventMessages.Victory.ToString(), victoryListener);

        failureListener = new UnityAction(CutInFrontPlayers);
        EventManager.StartListening(BattleEventMessages.Failure.ToString(), failureListener);


    }

    void MoveFromSideToBehindPlayers()
    {
        //no need so set transform.pos because it is the initial pos
        lerpBehaviour = LerpType.SideOfPlayersToBehind;
        currentLookAt = LookAtType.lookAtPlayerZone;
    }

    void CutInFrontPlayers()
    {
        transform.position = new Vector3(0f, 0.5f, -0.5f);
        lerpBehaviour = LerpType.InFrontPlayers;
        currentLookAt = LookAtType.lookAtPlayerZone;
    }

    void CutBehindPlayersLookAtCenter()
    {
        transform.position = new Vector3(-2f, 3.5f, -10f);
        lerpBehaviour = LerpType.BehindPlayers;
        currentLookAt = LookAtType.lookAtCenter;
    }

    void CutLeftToPlayerLookAtPlayer()
    {
        Vector3 playerPos = battleManager.GetCurrentPlayer().transform.position;
        float playerVerticalSize = battleManager.GetCurrentPlayer().GetComponent<CapsuleCollider>().height;
        float playerChestHeight = playerVerticalSize - 0.5f;

        float leftPlayerXOffset = -1f;
        float leftPlayerYOffset = playerChestHeight;
        float leftPlayerZOffset = 1.5f;

        Vector3 offset = new Vector3(leftPlayerXOffset, leftPlayerYOffset, leftPlayerZOffset);
        transform.position = playerPos + offset;

        transform.LookAt(battleManager.GetCurrentPlayer().transform.position + new Vector3(0f, playerChestHeight, 1.5f));

        lerpBehaviour = LerpType.LeftOfPlayer;
        currentLookAt = LookAtType.none;
      
    }

    void CutInFrontOfTargetLookAtTarget()
    {
        transform.position = new Vector3(battleManager.currentTargets[0].transform.position.x, 2f, 0f);
        lerpBehaviour = LerpType.InFrontOfTarget;
        currentLookAt = LookAtType.lookAtTarget;
    }

    void CutAndLookAtPlayerZone()
    {
        transform.position = new Vector3(0f, 2.5f, 2f);
        lerpBehaviour = LerpType.InFrontPlayers;
        currentLookAt = LookAtType.lookAtPlayerZone;
    }

    void CutAndLookAtMonsterZone()
    {
        transform.position = new Vector3(0f, 2.5f, -2f);
        lerpBehaviour = LerpType.InFrontMonsters;
        currentLookAt = LookAtType.lookAtMonsterZone;
    }

    void LateUpdate()
    {

        if (lerpBehaviour.Equals(LerpType.SideOfPlayersToBehind))
            transform.position = Vector3.Lerp(transform.position, new Vector3(0f, 2.5f, -10f), 0.8f * Time.deltaTime);
        else if (lerpBehaviour.Equals(LerpType.InFrontPlayers))
        {
           
        }
        else if (lerpBehaviour.Equals(LerpType.LeftOfPlayer) && battleManager.currentChoosingUnit != null)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, transform.position.y, -4.5f), 4f * Time.deltaTime);

            //only after lerp fully done

        }
        else if (lerpBehaviour.Equals(LerpType.BehindPlayers))
        {
            
        }
        else if (lerpBehaviour.Equals(LerpType.InFrontOfTarget))
        {
            //trouver une façon de le faire plus haut
            //if (battleManager.currentTargets != null && battleManager.currentTargets[0] != null)
                
        }

        if (currentLookAt.Equals(LookAtType.lookAtTarget) && battleManager.currentTargets != null && battleManager.currentTargets[0] != null)
            transform.LookAt(battleManager.currentTargets[0].transform);
        else if (currentLookAt.Equals(LookAtType.lookAtPlayerZone))
            transform.LookAt(new Vector3(0f, 1f, -5f));
        else if (currentLookAt.Equals(LookAtType.lookAtMonsterZone))
            transform.LookAt(new Vector3(0f, 1f, 5f));
        else if (currentLookAt.Equals(LookAtType.lookAtActingUnit) && battleManager.currentChoosingUnit != null)
            transform.LookAt(battleManager.currentChoosingUnit.transform);
        else if (currentLookAt.Equals(LookAtType.lookAtEnemy) && battleManager.GetCurrentEnemy() != null)
            transform.LookAt(battleManager.GetCurrentEnemy().transform);
        else if (currentLookAt.Equals(LookAtType.lookAtPlayer) && battleManager.GetCurrentPlayer() != null)
        {
            
        }
        else if (currentLookAt.Equals(LookAtType.lookAtCenter))
            transform.LookAt(new Vector3(0f, 0f, 0f));
        


    }
}
