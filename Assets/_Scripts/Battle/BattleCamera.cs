using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class BattleCamera : MonoBehaviour
{
    

    private BattleManager battleManager;
    private bool lookAtTarget = false;
    private bool lookAtActingUnit = false;
    private bool cutToLeftPlayer = false;
    private bool lookAtEnemy = false;
    private bool lookAtPlayer = false;

    private UnityAction playerUnitsExistListener;
    private UnityAction playerChoiceExpectedListener;
    private UnityAction targetsPanelDisplayedListener;
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

    // VICTORY OU FAILURE
    // Battle Ended - cut devant players, lookAt Centre PlayerZone

    // Use this for initialization
    void Start()
    {
        battleManager = BattleManager.Instance;

        playerChoiceExpectedListener = new UnityAction(CutLeftToPlayer);
        EventManager.StartListening(BattleEventMessages.playerChoiceExpected.ToString(), playerChoiceExpectedListener);

        targetsPanelDisplayedListener = new UnityAction(LookAtTarget);
        EventManager.StartListening(BattleEventMessages.targetsPanelDisplayed.ToString(), targetsPanelDisplayedListener);

        unitsLoadedListener = new UnityAction(RotateAnim);
        EventManager.StartListening(BattleEventMessages.unitsLoaded.ToString(), unitsLoadedListener);
    }

    void SetInitialCamPos()
    {

    }

    void RotateAnim()
    {
        
    }

    void CutLeftToPlayer()
    {
        cutToLeftPlayer = true;
        lookAtActingUnit = false;
        lookAtPlayer = true;
        lookAtTarget = false;
        lookAtEnemy = false;
    }

    void LookAtTarget()
    {
        lookAtActingUnit = false;
        lookAtTarget = true;
        lookAtEnemy = false;
    }

    void LateUpdate()
    {
        if (lookAtTarget && battleManager.currentTargetUnit != null)
            transform.LookAt(battleManager.currentTargetUnit.transform);
        else if (lookAtActingUnit && battleManager.currentActingUnit != null)
            transform.LookAt(battleManager.currentActingUnit.transform);
        else if (lookAtEnemy && battleManager.GetCurrentEnemy() != null)
            transform.LookAt(battleManager.GetCurrentEnemy().transform);
        else if (lookAtPlayer && battleManager.GetCurrentPlayer() != null)
        {
            
            //Vector3 playerPos = battleManager.GetCurrentPlayer().transform.position;
            //Vector3 offset = new Vector3(0f, 1.5f, 0f);
            //transform.LookAt(new Vector3(playerPos.x, playerPos.y + 1, playerPos.z));
        }
        
        if (cutToLeftPlayer && battleManager.currentActingUnit != null)
        {
            Vector3 playerPos = battleManager.GetCurrentPlayer().transform.position;
            float playerVerticalSize = battleManager.GetCurrentPlayer().GetComponent<CapsuleCollider>().height;
            float playerChestHeight = playerVerticalSize - 0.5f;

            float leftPlayerXOffset = -0.7f;
            float leftPlayerYOffset = playerChestHeight;
            float leftPlayerZOffset = 0.5f;

            Vector3 offset = new Vector3(leftPlayerXOffset, leftPlayerYOffset, leftPlayerZOffset);
            transform.position = playerPos + offset;

            transform.LookAt(battleManager.GetCurrentPlayer().transform.position + new Vector3(0f, playerChestHeight, 0.3f));

            cutToLeftPlayer = false;
        }
    }
}
