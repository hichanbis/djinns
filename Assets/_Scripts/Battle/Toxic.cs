using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Toxic : Status
{
    private int timeApplied = 0;
    private int hpPercentToRemove = 15;

    public Toxic(){
        successRatePercent = 30;
    }

    public override bool Finished
    {
        get
        {
            return timeApplied == 5;
        }
    }

    public override void Add(GameObject unit)
    {
        this.unit = unit;
        unit.GetComponent<BattleScript>().Character.listStatus.Add(this);
        //effet visuel icone son whatever (ou alors dans le battlescript ca)
    }

    public override void Apply()
    {
        float damage = unit.GetComponent<BattleScript>().Character.GetStat(StatName.hp).baseValue * ((float)hpPercentToRemove / 100);
        unit.GetComponent<BattleScript>().TakeDamage(Mathf.RoundToInt(damage));
        timeApplied++;
    }

    public override void Remove()
    {
        //Remove effet visuel icone son whatever (ou alors dans le battlescript ca)
        unit.GetComponent<BattleScript>().Character.listStatus.Remove(this);
    }

}