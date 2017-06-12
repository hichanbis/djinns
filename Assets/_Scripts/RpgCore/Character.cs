using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Character
{
	public string name;
	public Element element;
	public List<Ability> abilities;
	public List<Stat> stats;
    public bool canSummon;
    public List<Status> statuses;
    
    public Character (string name, Element element, List<Ability> abilities, List<Stat> stats, bool canSummon)
	{
		this.name = name;
		this.element = element;
		this.abilities = abilities;
		this.stats = stats;
        this.canSummon = canSummon;
        this.statuses = new List<Status>() {};
    }

     public List<Ability> Abilities {
		get {
			return this.abilities;
		}
	}

	public Ability getAbility (string name)
	{
		return this.abilities.Find (a => a.name.Equals (name));
	}

    public Stat GetStat(StatName name)
    {
        return this.stats.Find(s => s.name.Equals(name));
    }

}
