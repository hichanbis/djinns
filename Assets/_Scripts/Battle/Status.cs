using UnityEngine;

public abstract class Status
{
    protected int successRatePercent;
    protected GameObject unit;
    protected string type;

    public abstract bool Finished { get; }
    public abstract void Add(GameObject unit);
    public abstract void Remove();
    public abstract void Apply();

    public int SuccessRatePercent
    {
        get
        {
            return successRatePercent;
        }
    }

    public string Type
    {
        get
        {
            return type;
        }
    }
}


