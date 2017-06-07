using UnityEngine;

public abstract class Status : MonoBehaviour
{
    protected int successRatePercent;

    public abstract bool Finished { get; }
    public abstract void Add(GameObject unit);
    public abstract void Remove(GameObject unit);
    public abstract void ApplyEndTurn(GameObject unit);
    public int SuccessRatePercent
    {
        get
        {
            return successRatePercent;
        }
    }

}


