using UnityEngine;

class Rng
{
    public static bool GetSuccess(int successRatePercent)
    {
        return Random.Range(0, 99) < successRatePercent;
    }
}

