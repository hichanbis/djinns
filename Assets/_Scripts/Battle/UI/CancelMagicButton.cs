using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CancelMagicButton : MonoBehaviour, ICancelHandler
{
    //only work on button?
    public void OnCancel(BaseEventData eventData)
    {
        GetComponentInParent<BattleUI>().CancelMagic();
    }


}
