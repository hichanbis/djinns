using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Stat
{
    public int baseValue;
    public List<int> modifiers;

    public Stat(int baseValue)
    {
        this.baseValue = baseValue;
        this.modifiers = new List<int>();
    }

    //returns modified value
    public int GetValue()
    {
        int value = baseValue;

        foreach (int modifier in modifiers)
        {
            value += baseValue * modifier / 100;
        }

        return value;
    }
}


