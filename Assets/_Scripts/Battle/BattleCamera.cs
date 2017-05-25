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

    private UnityAction playerUnitsExistListener;
    private UnityAction playerMustChooseAbilityListener;

    // Use this for initialization
    void Start()
    {
        battleManager = BattleManager.Instance;

        playerUnitsExistListener = new UnityAction(SetInitialCamPos);
        EventManager.StartListening(BattleEventMessages.playerUnitsExist.ToString(), playerUnitsExistListener);

        playerMustChooseAbilityListener = new UnityAction(MoveBehindPlayer);
        EventManager.StartListening(BattleEventMessages.playerChoiceExpected.ToString(), playerMustChooseAbilityListener);
    }

    void SetInitialCamPos()
    {

    }

    void MoveBehindPlayer()
    {
        Vector3 offset = new Vector3(xOffset, yOffset, zOffset);
        float desiredAngle = battleManager.currentUnit.transform.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, desiredAngle, 0);
        transform.position = battleManager.currentUnit.transform.position + (rotation * offset);
        transform.LookAt(battleManager.currentUnit.transform);
    }
}
