using UnityEngine;
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

    public override void Add(Character character)
    {
        //effet visuel icone son whatever (ou alors dans le battlescript ca)
        character.status.Add(this);
    }

    public override void ApplyEndTurn(Character character)
    {
        float damage = character.GetStat(StatName.hp).baseValue * ((float)hpPercentToRemove / 100);
        character.GetStat(StatName.hpNow).baseValue = Mathf.RoundToInt(Mathf.Clamp(character.GetStat(StatName.hpNow).baseValue - damage, 0, character.GetStat(StatName.hp).baseValue));
        timeApplied++;
    }

    public override void Remove(Character character)
    {
        //Remove effet visuel icone son whatever (ou alors dans le battlescript ca)
        character.status.Remove(this);
    }

}