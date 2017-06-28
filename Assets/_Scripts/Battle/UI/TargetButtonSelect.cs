using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TargetButtonSelect : MonoBehaviour, ISelectHandler
{
    private GameObject cursor;

    public void OnSelect(BaseEventData eventData)
    {
        BattleManager.Instance.SetCurrentTargetFromName(gameObject.name);
    }

}
