using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Stat
{
    [SerializeField]
    private int baseValue;
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

    //returns base value
    public int GetBaseValue()
    {
        return baseValue;
    }

    public void SetValue(int value)
    {
        if (baseValue == value) return;

        //cannot call delegates before changing the value
        //so I store the old value for comparison
        int oldValue = baseValue;

        baseValue = value;


        if (OnStatChanged != null)
            OnStatChanged(value, value - oldValue);
    }

    public delegate void OnStatChangedDelegate(int value, int dmg);
    
    public event OnStatChangedDelegate OnStatChanged;

}


