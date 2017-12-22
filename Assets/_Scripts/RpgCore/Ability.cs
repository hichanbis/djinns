using System.Collections.Generic;
using System;


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
    Undefined,
    Self,
    Opposite,
    Same,
    AllSame,
    AllOpposite
}

[System.Serializable]
public class Ability
{
    public string id;
    public string name;
    public string description;
    public AbilityType abilityType;
    public Element element;
    public Distance distance;
    public TargetType targetType;
    public int mpCost;
    public int power;
    public List<string> statusIds;

    public Ability()
    {
    }

    public Ability(string id, string name, string description, AbilityType abilityType, Element element, Distance distance, TargetType targetType, int power, int mpCost, List<string> statusIds)
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
        this.statusIds = statusIds;
    }

    public override string ToString()
    {
        return string.Format("[Ability: id={0}, name={1}, power={2}, statuses={3}]", id, name, power, statusIds.Count > 0 ? string.Join(" / ", statusIds.ConvertAll(x => Convert.ToString(x)).ToArray()) : null);
    }

}


