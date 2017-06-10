using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Poison : Status
{
    private int timeApplied = 0;
    private int hpPercentToRemove = 10;
    private int damage;

    public Poison()
    {
        successRatePercent = 100;
        type = "damage";
    }

    public int Damage
    {
        get
        {
            return damage;
        }
    }


    public override bool Finished
    {
        get
        {
            return timeApplied == 3;
        }
    }

    public override void Add(GameObject unit)
    {
        this.unit = unit;
        //effet visuel icone son whatever (ou alors dans le battlescript ca)
        unit.GetComponent<BattleScript>().Character.listStatus.Add(this);
    }


    public override void Apply()
    {
        damage = Mathf.RoundToInt(unit.GetComponent<BattleScript>().Character.GetStat(StatName.hp).baseValue * ((float)hpPercentToRemove / 100));
        timeApplied++;
    }

    public override void Remove()
    {
        //Remove effet visuel icone son whatever (ou alors dans le battlescript ca)
        unit.GetComponent<BattleScript>().Character.listStatus.Remove(this);
    }

}