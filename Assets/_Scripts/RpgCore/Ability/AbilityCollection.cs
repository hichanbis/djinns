using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        LoadAbilities();
    }

    private void OnEnable()
    {
        EditorApplication.projectChanged -= LoadAbilities;
        EditorApplication.projectChanged += LoadAbilities;
    }

    private void OnDisable()
    {
        EditorApplication.projectChanged -= LoadAbilities;
    }

    private void LoadAbilities()
    {
        abilities = EditorUtils.FindAssetsByType<Ability>("Assets/_Databases/Abilities");
    }
#endif
}



