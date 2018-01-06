using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public enum StatusApplyMoment
{
    endTurn,
    add
}

public enum StatusType
{
    ailment,
    support
}

public enum StatusApplyType
{
    damage,
    disableCommands,
    disableMagic,
    modifier,
    addStatus
}

[System.Serializable]
public class Status
{
    public string id;
    public string name;
    public StatusType statusType;
    public int successRatePercent;
    public string description;
    public StatusApplyMoment applyMoment;
    public StatusApplyType applyType;
    public StatName statName;
    public int powerPercent;
    public int maxTurns;
    public List<string> removesStatusesOnAdd;
    public List<string> blockedByStatuses;

    public Status()
    {
    }

    public override string ToString()
    {
        return string.Format("[Status:{0} rate:{1}% moment:{2} type:{3} stat:{4} pow:{5}% maxTurns:{6}]", id, successRatePercent, applyMoment, applyType, statName, powerPercent, maxTurns);
    }

}


