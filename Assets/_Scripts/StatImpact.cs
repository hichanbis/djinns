using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[System.Serializable]
public enum ImpactType
{
    Add,
    Sub,
    Mult
}

[System.Serializable]
public class StatImpact
{
    public string name;
    public ImpactType type;
    public float value;

    public StatImpact()
    {
    }

    public StatImpact(string name, ImpactType type, float value)
    {
        this.name = name;
        this.type = type;
        this.value = value;
    }

    public override string ToString()
    {
        return string.Format("[StatImpact: name={0}, type={1}, value={2}]", name, type, value);
    }

}

