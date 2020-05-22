using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public enum StatusApplyMoment
{
    endTurn,
    add
}

[System.Serializable]
public enum StatusType
{
    ailment,
    support
}

[System.Serializable]
public enum StatusApplyType
{
    damage,
    disableCommands,
    disableMagic,
    modifier,
    addStatus
}

[CreateAssetMenu()]
public class Status : ScriptableObject
{
    public string id;
    public StatusType statusType;
    public int successRatePercent;
    public string description;
    public StatusApplyMoment applyMoment;
    public StatusApplyType applyType;
    public StatName statName;
    public int powerPercent;
    public int maxTurns;
    public List<Status> removesStatusesOnAdd;
    public List<Status> blockedByStatuses;

    public override string ToString()
    {
        return string.Format("[Status:{0} rate:{1}% moment:{2} type:{3} stat:{4} pow:{5}% maxTurns:{6}]", name, successRatePercent, applyMoment, applyType, statName, powerPercent, maxTurns);
    }

}


