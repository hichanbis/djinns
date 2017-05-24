public abstract class Status
{
    protected int successRatePercent;

    public abstract bool Finished { get; }
    public abstract void Add(Character character);
    public abstract void Remove(Character character);
    public abstract void ApplyEndTurn(Character character);
    public int SuccessRatePercent
    {
        get
        {
            return successRatePercent;
        }
    }

}


