using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Guard : Status
{
    private int timeApplied = 0;

    public Guard()
    {
        successRatePercent = 100;
    }

    public override bool Finished
    {
        get
        {
            return timeApplied == 1;
        }
    }

    public override void Add(GameObject unit)
    {
        //Add +50% modifier
        unit.GetComponent<BattleScript>().Character.status.Add(this);
        unit.GetComponent<BattleScript>().Character.GetStat(StatName.defense).modifiers.Add(50);
    }

    public override void ApplyEndTurn(GameObject unit)
    {
        timeApplied++;
    }

    public override void Remove(GameObject unit)
    {
        //Remove modifiers
        unit.GetComponent<BattleScript>().Character.GetStat(StatName.defense).modifiers.Remove(50);
        unit.GetComponent<BattleScript>().Character.status.Remove(this);
        //Remove effet visuel icone son whatever (ou alors dans le battlescript ca)
    }

}