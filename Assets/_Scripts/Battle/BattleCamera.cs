using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;


public enum LookAtType
{
    none,
    lookAtCenter,
    lookAtActingUnit,
    lookAtTarget,
    lookAtPlayerZone,
    lookAtMonsterZone
}

public enum LerpType
{
    none,
    InFrontPlayers,
    InFrontMonsters,
    InFrontOfTarget,
    SideOfPlayersToBehind,
    BehindPlayers,
    LeftFrontOfPlayerToLeftCenter,
    FollowActing,
    ToBCPMeleeView
}

public enum CameraPos
{
    BCPSideOfPlayers,
    BCPBehindPlayers,
    BCPMeleeView
}


public class BattleCamera : MonoBehaviour
{
 
    private BattleManager battleManager;
    private LookAtType currentLookAt;
    private LerpType lerpBehaviour;
    private Vector3 offset;
    //BattleCameraPositions object
    private Transform BCPSideOfPlayers;
    private Transform BCPBehindPlayers;
    private Transform BCPPlayerChoiceLeft;
    private Transform BCPPlayerRun;
    private Transform BCPMeleeView;

    private UnityAction unitsLoadedListener;
    private UnityAction playerChoiceExpectedListener;
    private UnityAction targetChoiceExpectedListener;
    private UnityAction targetChoiceAllPlayersListener;
    private UnityAction targetChoiceAllMonstersListener;
    private UnityAction actionChoicePhaseDoneListener;
    private UnityAction meleeAttackListener;

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
    // Melee vue d'en haut en diagonale de l'arène

    // Magic attack - Cut devant acting lookAtActing
    // Magic attack launched Single - cut devant target
    // Magic attack launched All - cut devant targets, lookAt Centre TargetZone

    // End Rage - Show all units

    // VICTORY OU FAILURE
    // Battle Ended - cut devant players, lookAt Centre PlayerZone

    // Use this for initialization
    void Start()
    {
        BCPSideOfPlayers = GameObject.Find("BCPSideOfPlayers").transform;
        BCPBehindPlayers = GameObject.Find("BCPBehindPlayers").transform;
        BCPMeleeView = GameObject.Find("BCPMeleeView").transform;

        //transform.position = new Vector3(20f, 2f, 0f);
        lerpBehaviour = LerpType.none;
        currentLookAt = LookAtType.none;

        battleManager = BattleManager.Instance;

        unitsLoadedListener = new UnityAction(CutFromSideToBehindPlayers);
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

        meleeAttackListener = new UnityAction(LerpToDiagonalView);
        EventManager.StartListening(BattleEventMessages.MeleeAttack.ToString(), meleeAttackListener);

        victoryListener = new UnityAction(CutInFrontPlayers);
        EventManager.StartListening(BattleEventMessages.Victory.ToString(), victoryListener);

        failureListener = new UnityAction(CutInFrontPlayers);
        EventManager.StartListening(BattleEventMessages.Failure.ToString(), failureListener);


    }

    void CutFromSideToBehindPlayers()
    {
        transform.position = BCPSideOfPlayers.position;
        transform.rotation = BCPSideOfPlayers.rotation;

        lerpBehaviour = LerpType.SideOfPlayersToBehind;
        currentLookAt = LookAtType.lookAtPlayerZone;
    }

    void CutBehindPlayerAndFollow()
    {
        BCPPlayerRun = battleManager.currentActingUnit.transform.Find("BCPPLayerRun").transform;

        transform.position = BCPPlayerRun.position;
        transform.rotation = BCPPlayerRun.rotation;

        offset = transform.position - battleManager.currentActingUnit.transform.position;

        lerpBehaviour = LerpType.FollowActing;
        currentLookAt = LookAtType.lookAtTarget;
    }

    void CutInFrontPlayers()
    {
        transform.position = new Vector3(0f, 0.5f, -0.5f);
        lerpBehaviour = LerpType.none;
        currentLookAt = LookAtType.lookAtPlayerZone;
    }

    void CutBehindPlayersLookAtCenter()
    {
        transform.position = new Vector3(-2f, 3.5f, -10f);
        lerpBehaviour = LerpType.none;
        currentLookAt = LookAtType.lookAtCenter;
    }

    void LerpToDiagonalView()
    {
        //transform.position = BCPBehindPlayers.position;
        //transform.rotation = BCPBehindPlayers.rotation;

        currentLookAt = LookAtType.lookAtCenter;
        lerpBehaviour = LerpType.ToBCPMeleeView;

    }

    void CutLeftToPlayerLookAtPlayer()
    {
        BCPPlayerChoiceLeft = battleManager.currentChoosingUnit.transform.Find("BCPPlayerChoiceLeft");
        transform.position = BCPPlayerChoiceLeft.position;
        transform.rotation = BCPPlayerChoiceLeft.rotation;

        lerpBehaviour = LerpType.LeftFrontOfPlayerToLeftCenter;
        currentLookAt = LookAtType.none;
      
    }

    void CutInFrontOfTargetLookAtTarget()
    {
        transform.position = new Vector3(battleManager.currentTargets[0].transform.position.x, 2f, 0f);
        lerpBehaviour = LerpType.none;
        currentLookAt = LookAtType.lookAtTarget;
    }

    void CutAndLookAtPlayerZone()
    {
        transform.position = new Vector3(0f, 2.5f, 2f);
        lerpBehaviour = LerpType.none;
        currentLookAt = LookAtType.lookAtPlayerZone;
    }

    void CutAndLookAtMonsterZone()
    {
        transform.position = new Vector3(0f, 2.5f, -2f);
        lerpBehaviour = LerpType.none;
        currentLookAt = LookAtType.lookAtMonsterZone;
    }

    void LateUpdate()
    {

        if (lerpBehaviour.Equals(LerpType.SideOfPlayersToBehind))
            transform.position = Vector3.Lerp(transform.position, BCPBehindPlayers.position, 0.8f * Time.deltaTime);
        else if (lerpBehaviour.Equals(LerpType.LeftFrontOfPlayerToLeftCenter) && battleManager.currentChoosingUnit)
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, transform.position.y, BCPPlayerChoiceLeft.position.z - 0.3f), 4f * Time.deltaTime);
        else if (lerpBehaviour.Equals(LerpType.FollowActing) && battleManager.currentActingUnit)
            transform.position = battleManager.currentActingUnit.transform.position + offset;
        else if (lerpBehaviour.Equals(LerpType.ToBCPMeleeView))
            transform.position = Vector3.Lerp(transform.position, BCPMeleeView.position, 0.8f * Time.deltaTime);

        if (currentLookAt.Equals(LookAtType.lookAtTarget) && battleManager.currentTargets != null && battleManager.currentTargets[0] != null)
            transform.LookAt(battleManager.currentTargets[0].transform);
        else if (currentLookAt.Equals(LookAtType.lookAtPlayerZone))
            transform.LookAt(new Vector3(0f, 1f, -5f));
        else if (currentLookAt.Equals(LookAtType.lookAtMonsterZone))
            transform.LookAt(new Vector3(0f, 1f, 5f));
        else if (currentLookAt.Equals(LookAtType.lookAtActingUnit) && battleManager.currentActingUnit)
            transform.LookAt(battleManager.currentActingUnit.transform);
        else if (currentLookAt.Equals(LookAtType.lookAtCenter))
            transform.LookAt(new Vector3(0f, 0f, 0f));
        


    }



}
