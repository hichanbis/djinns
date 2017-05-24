using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BattleAction
{
    public GameObject fromUnit;
    public List<GameObject> targets;
    public Ability ability;

    public BattleAction()
    {
    }

    public BattleAction(GameObject fromUnit, List<GameObject> targets, Ability ability)
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


