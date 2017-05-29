using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class BattleCamera : MonoBehaviour
{
    public float xOffset;
    public float yOffset;
    public float zOffset;

    private BattleManager battleManager;
    private bool lookAtTarget = false;
    private bool lookAtActingUnit = false;
    private bool moveBehindPlayer = false;

    private UnityAction playerUnitsExistListener;
    private UnityAction playerMustChooseAbilityListener;
    private UnityAction targetsPanelDisplayedListener;

    // Use this for initialization
    void Start()
    {
        battleManager = BattleManager.Instance;

        playerUnitsExistListener = new UnityAction(SetInitialCamPos);
        EventManager.StartListening(BattleEventMessages.playerUnitsExist.ToString(), playerUnitsExistListener);

        playerMustChooseAbilityListener = new UnityAction(MoveBehindPlayer);
        EventManager.StartListening(BattleEventMessages.playerChoiceExpected.ToString(), playerMustChooseAbilityListener);

        targetsPanelDisplayedListener = new UnityAction(LookAtTarget);
        EventManager.StartListening(BattleEventMessages.targetsPanelDisplayed.ToString(), targetsPanelDisplayedListener);
    }

    void SetInitialCamPos()
    {

    }

    void MoveBehindPlayer()
    {
        moveBehindPlayer = true;
        lookAtActingUnit = true;
        lookAtTarget = false;
    }

    void LookAtTarget()
    {
        lookAtActingUnit = false;
        lookAtTarget = true;
    }

    void LateUpdate()
    {
        if (lookAtTarget && battleManager.currentTargetUnit != null)
            transform.LookAt(battleManager.currentTargetUnit.transform);
        else if (lookAtActingUnit && battleManager.currentActingUnit != null)
            transform.LookAt(battleManager.currentActingUnit.transform);

        if (moveBehindPlayer && battleManager.currentActingUnit != null)
        {
            Vector3 offset = new Vector3(xOffset, yOffset, zOffset);
            float desiredAngle = battleManager.currentActingUnit.transform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, desiredAngle, 0);
            transform.position = battleManager.currentActingUnit.transform.position + (rotation * offset);
        }
    }
}
