using UnityEngine;
using System.Collections.Generic;
using System;


[System.Serializable]
public class Character
{
    public string name;
    public Element element;
    public List<Ability> abilities; // changer pour liste string ids
    public Stats stats;
    public List<Status> statuses; // changer pour liste string ids

    public Character(string name, Element element, List<Ability> abilities, Stats stats)
    {
        this.name = name;
        this.element = element;
        this.abilities = abilities;
        this.stats = stats;
        this.statuses = new List<Status>() { };
    }

    public Ability GetAbility(string name)
    {
        return this.abilities.Find(a => a.name.Equals(name));
    }

    public Stat GetStat(StatName name)
    {
        if (name.Equals(StatName.hp))
            return this.stats.hp;
        else if (name.Equals(StatName.hpNow))
            return this.stats.hpNow;
        else if (name.Equals(StatName.mp))
            return this.stats.mp;
        else if (name.Equals(StatName.mpNow))
            return this.stats.mpNow;
        else if (name.Equals(StatName.strength))
            return this.stats.strength;
        else if (name.Equals(StatName.defense))
            return this.stats.defense;
        else if (name.Equals(StatName.agility))
            return this.stats.agility;
        else if (name.Equals(StatName.intelligence))
            return this.stats.intelligence;
        else
            return null;
        
    }

    internal void RestoreStatuses(StatusCollection statusCollection)
    {
        for (int i = 0; i < statuses.Count; i++)
        {
            statuses[i] = statusCollection.GetStatusFromId(statuses[i].id);
        }
    }

    public void RestoreAbilities(AbilityCollection abilityCollection)
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            abilities[i] = abilityCollection.GetAbilityFromId(abilities[i].id);
        }
    }


    public override string ToString()
    {
        return string.Format("[Character: name={0}, statuses={1}]", name, statuses.Count > 0 ? string.Join(" / ", statuses.ConvertAll(x => Convert.ToString(x)).ToArray()) : null);
    }

}
