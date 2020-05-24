using System;
using System.Collections.Generic;

[Serializable]
public class CharacterSaveData
{
    public string id;
    public Element element;
    public List<string> abilityIds;
    public List<string> statusIds;
    public Stats stats;

    public CharacterSaveData() { }

    public CharacterSaveData(Character character)
    {
        id = character.id;
        element = character.element;
        stats = character.stats;
        abilityIds = new List<string>();
        statusIds = new List<string>();

        for (int i = 0; i < character.abilities.Count; i++)
        {
            abilityIds.Add(character.abilities[i].id);
        }

        for (int i = 0; i < character.statuses.Count; i++)
        {
            statusIds.Add(character.statuses[i].id);
        }
    }
}
