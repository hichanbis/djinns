using System;
using System.Collections.Generic;

[Serializable]
public class CharacterSaveData
{
    public List<string> abilityIds;
    public List<string> statusIds;
    public CharacterSaveData()
    {
        abilityIds = new List<string>();
        statusIds = new List<string>();
    }
}
