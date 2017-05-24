using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Poison : Status
{
    private int turnPresent = 0;
    private int hpPercentToRemove = 10;
    int successRatePercent = 100;


    public override bool Finished
    {
        get
        {
            return turnPresent == 5;
        }
    }

    public override void Add(Character character)
    {
        //effet visuel icone son whatever (ou alors dans le battlescript ca)
    }

    public override void ApplyEndTurn(Character character)
    {
        float damage = character.GetStat(StatName.hp).baseValue * ((float)hpPercentToRemove / 100);
        character.GetStat(StatName.hpNow).baseValue = Mathf.RoundToInt(Mathf.Clamp(character.GetStat(StatName.hpNow).baseValue - damage, 0, character.GetStat(StatName.hp).baseValue));
        turnPresent++;
    }

    public override void Remove(Character character)
    {
        //Remove effet visuel icone son whatever (ou alors dans le battlescript ca)
    }

}