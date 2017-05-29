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

    public override void Add(Character character)
    {
        //Add +50% modifier
        character.status.Add(this);
        character.GetStat(StatName.defense).modifiers.Add(50);
    }

    public override void ApplyEndTurn(Character character)
    {
        timeApplied++;
    }

    public override void Remove(Character character)
    {
        //Remove modifiers
        character.GetStat(StatName.defense).modifiers.Remove(50);
        character.status.Remove(this);
        //Remove effet visuel icone son whatever (ou alors dans le battlescript ca)
    }

}