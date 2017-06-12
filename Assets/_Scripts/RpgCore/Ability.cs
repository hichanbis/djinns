using System.Collections.Generic;
using System.Xml;
using System;
using System.Xml.Serialization;

[System.Serializable]
public enum AbilityType
{
    Magic,
    Melee
}

[System.Serializable]
public enum Distance
{
    Close,
    Range
}

[System.Serializable]
public enum TargetType
{
    Self,
    Opposite,
    Same,
    AllSame,
    AllOpposite
}

[System.Serializable]
public class Ability
{
    [XmlAttribute("id")]
    public string id;
    public string name;
    public string description;
    public AbilityType abilityType;
    public Element element;
    public Distance distance;
    public TargetType targetType;
    public int mpCost;
    public int power;
    public List<Status> statuses;

    public Ability()
    {
    }

    public Ability(string id, string name, string description, AbilityType abilityType, Element element, Distance distance, TargetType targetType, int power, int mpCost, List<Status> statuses)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.abilityType = abilityType;
        this.element = element;
        this.distance = distance;
        this.targetType = targetType;
        this.mpCost = mpCost;
        this.power = power;
        this.statuses = statuses;
    }

    public override string ToString()
    {
        return string.Format("[Ability: id={0}, name={1}, power={2}, statuses={3}]", id, name, power, statuses.Count > 0 ? string.Join(" / ", statuses.ConvertAll(x => Convert.ToString(x)).ToArray()) : null);
    }

}


