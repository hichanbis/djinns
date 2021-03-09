using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BattleAction
{
    public BattleScript fromUnit;
    public List<BattleScript> targets;
    public Ability ability;

    public BattleAction()
    {
    }

    public BattleAction(BattleScript fromUnit, List<BattleScript> targets, Ability ability)
    {
        this.fromUnit = fromUnit;
        this.targets = targets;
        this.ability = ability;
    }


    public override string ToString()
    {
        return string.Format("[BattleAction: fromUnit={0}, targets={1}, ability={2}]", fromUnit, targets == null ? null : targets[0], ability);
    }
	
}


