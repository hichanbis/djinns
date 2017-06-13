using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public enum StatusApplyMoment
{
    endTurn,
    add
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
    [XmlAttribute("id")]
    public string id;
    public string name;
    public string type;
    public int successRatePercent;
    public string description;
    public StatusApplyMoment applyMoment;
    public StatusApplyType applyType;
    public StatName statName;
    public int powerPercent;

    public Status()
    {
    }

    public override string ToString()
    {
        return string.Format("[Status:{0} rate:{1}% moment:{2} type:{3} stat:{4} pow:{5}%]", id, successRatePercent, applyMoment, applyType, statName, powerPercent);
    }

}


