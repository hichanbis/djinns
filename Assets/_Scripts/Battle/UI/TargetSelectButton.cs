using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TargetSelectButton : MonoBehaviour, ISelectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        BattleManager.Instance.SetCurrentTargetFromName(gameObject.name);
    }

}
