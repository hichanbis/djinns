using System.Collections.Generic;
using System;
using UnityEngine;

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

[CreateAssetMenu()]
public class Ability : ScriptableObject
{
    public string id;
    public string description;
    public AbilityType abilityType;
    public Element element;
    public Distance distance;
    public TargetType targetType;
    public int mpCost;
    public int power;
    public List<Status> statuses = new List<Status>();

    public override string ToString()
    {
        return string.Format("[Ability: id={0}, power={1}, statuses={2}]", id, power, statuses.Count > 0 ? string.Join(" / ", statuses.ConvertAll(x => Convert.ToString(x)).ToArray()) : null);
    }

}


