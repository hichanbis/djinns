using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[CreateAssetMenu()]
public class AbilityCollection : ScriptableObject
{
    public List<Ability> abilities;

    public Ability GetAbilityFromId(string id)
    {
        return abilities.Find(ab => ab.id.Equals(id));
    }

    public Ability GetAbilityFromName(string name)
    {
        return abilities.Find(ab => ab.name.Equals(name));
    }
}

