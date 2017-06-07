﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Poison : Status
{
    private int timeApplied = 0;
    private int hpPercentToRemove = 10;
   
    public Poison(){
        successRatePercent = 30;
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
        //effet visuel icone son whatever (ou alors dans le battlescript ca)
        unit.GetComponent<BattleScript>().Character.status.Add(this);
    }

    public override void ApplyEndTurn(GameObject unit)
    {
        float damage = unit.GetComponent<BattleScript>().Character.GetStat(StatName.hp).baseValue * ((float)hpPercentToRemove / 100);
        StartCoroutine(unit.GetComponent<BattleScript>().TakeDamage(Mathf.RoundToInt(damage)));
        timeApplied++;
    }

    public override void Remove(GameObject unit)
    {
        //Remove effet visuel icone son whatever (ou alors dans le battlescript ca)
        unit.GetComponent<BattleScript>().Character.status.Remove(this);
    }

}