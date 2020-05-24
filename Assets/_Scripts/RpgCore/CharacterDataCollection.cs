using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu()]
public class CharacterDataCollection : ScriptableObject
{
    public List<Character> characters;
    
    public Character GetCharacterFromId(string id)
    {
        return characters.Find(ch => ch.id.Equals(id));
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        LoadCharacters();
    }

    private void OnEnable()
    {
        EditorApplication.projectChanged -= LoadCharacters;
        EditorApplication.projectChanged += LoadCharacters;
    }

    private void OnDisable()
    {
        EditorApplication.projectChanged -= LoadCharacters;
    }

    private void LoadCharacters()
    {
        characters = EditorUtils.FindAssetsByType<Character>("Assets/_Databases/CharacterData");
    }
#endif
}



