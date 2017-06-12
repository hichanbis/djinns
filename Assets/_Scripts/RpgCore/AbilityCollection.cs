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

    public void Save(){
        Database.SaveWithReferences<AbilityCollection>(instance, Application.dataPath + "/_Databases/AbilityCollectionWithRefsSaved.xml");
    }

    public void ReplaceWithRefs(){
       
        Debug.Log("Replacing Statuses in Abilities with existing ones");

        for (int i=0; i < AbilityCollection.Instance.abilities.Count; i++)
        {
            Ability ability = AbilityCollection.Instance.abilities[i];
            if (ability.statuses.Count > 0)
            {
                for (int j = 0; j < ability.statuses.Count; j++)
                {
                    var refToNull = ability.statuses[j];
                    ability.statuses[j] = StatusCollection.Instance.FindStatusFromId(ability.statuses[j].id);
                    refToNull = null;
                }
            }
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

