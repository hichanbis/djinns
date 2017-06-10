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
        type = "other";
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
        this.unit = unit;
        //Add +50% modifier
        unit.GetComponent<BattleScript>().Character.listStatus.Add(this);
        unit.GetComponent<BattleScript>().Character.GetStat(StatName.defense).modifiers.Add(50);
    }

    public override void Apply()
    {
        timeApplied++;
    }

    public override void Remove()
    {
        //Remove modifiers
        unit.GetComponent<BattleScript>().Character.GetStat(StatName.defense).modifiers.Remove(50);
        unit.GetComponent<BattleScript>().Character.listStatus.Remove(this);
        //Remove effet visuel icone son whatever (ou alors dans le battlescript ca)
    }

}