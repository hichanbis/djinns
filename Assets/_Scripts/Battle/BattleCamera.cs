using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class BattleCamera : MonoBehaviour
{
    public float xOffset = 0f;
    public float yOffset = 4f;
    public float zOffset = -5.5f;

    private BattleManager battleManager;
    private bool lookAtTarget = false;
    private bool lookAtActingUnit = false;
    private bool moveBehindPlayer = false;
    private bool lookAtEnemy = false;

    private UnityAction playerUnitsExistListener;
    private UnityAction playerChoiceExpectedListener;
    private UnityAction targetsPanelDisplayedListener;
    private UnityAction unitsLoadedListener;


    // Use this for initialization
    void Start()
    {
        battleManager = BattleManager.Instance;

        playerUnitsExistListener = new UnityAction(SetInitialCamPos);
        EventManager.StartListening(BattleEventMessages.unitsLoaded.ToString(), playerUnitsExistListener);

        playerChoiceExpectedListener = new UnityAction(MoveBehindPlayer);
        EventManager.StartListening(BattleEventMessages.playerChoiceExpected.ToString(), playerChoiceExpectedListener);

        targetsPanelDisplayedListener = new UnityAction(LookAtTarget);
        EventManager.StartListening(BattleEventMessages.targetsPanelDisplayed.ToString(), targetsPanelDisplayedListener);

        unitsLoadedListener = new UnityAction(RotateAnim);
        EventManager.StartListening(BattleEventMessages.beginFight.ToString(), unitsLoadedListener);
    }

    void SetInitialCamPos()
    {

    }

    void RotateAnim()
    {
        GetComponent<Animation>().Play();
    }

    void MoveBehindPlayer()
    {
        moveBehindPlayer = true;
        lookAtActingUnit = false;
        lookAtTarget = false;
        lookAtEnemy = true;
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

        if (moveBehindPlayer && battleManager.currentActingUnit != null)
        {
            Vector3 offset = new Vector3(xOffset, yOffset, zOffset);
            //float desiredAngle = battleManager.currentActingUnit.transform.eulerAngles.y;
            //Quaternion rotation = Quaternion.Euler(0, desiredAngle, 0);
            Quaternion rotation = Quaternion.Euler(0, 0, 0);
            transform.position = battleManager.currentActingUnit.transform.position + (rotation * offset);
            moveBehindPlayer = false;
        }
    }
}
