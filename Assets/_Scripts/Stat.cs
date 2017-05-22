using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Stat
{
    public StatName name;
    public int baseValue;
    public List<int> modifiers;

    public Stat(StatName name, int baseValue)
    {
        this.name = name;
        this.baseValue = baseValue;
        this.modifiers = new List<int>();
    }

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


