using System;

[System.Serializable]
public struct Stats
{
    public Stat hpNow;
    public Stat hp;
    public Stat mpNow;
    public Stat mp;
    public Stat strength;
    public Stat defense;
    public Stat intelligence;
    public Stat agility;

    public Stats(Stat hp, Stat hpNow, Stat mp, Stat mpNow, Stat strength, Stat defense, Stat intelligence, Stat agility)
    {
        this.hp = hp;
        this.hpNow = hpNow;
        this.mp = mp;
        this.mpNow = mpNow;
        this.strength = strength;
        this.defense = defense;
        this.intelligence = intelligence;
        this.agility = agility;
    }
}


