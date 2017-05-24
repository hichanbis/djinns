using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("AbilityCollection")]
public class AbilityCollection
{
    private static AbilityCollection instance;

    private AbilityCollection() { }

    [XmlArray("Abilities"), XmlArrayItem("Ability")]
    public List<Ability> abilities;

    public static AbilityCollection Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Database.Load<AbilityCollection>(Application.dataPath + "/_Databases/AbilityCollection.xml");
            }
            return instance;
        }
    }


    public Ability FindAbilityFromId(string id)
    {
        return abilities.Find(ab => ab.id.Equals(id));
    }

    public Ability FindAbilityFromName(string name)
    {
        return abilities.Find(ab => ab.name.Equals(name));
    }
}

