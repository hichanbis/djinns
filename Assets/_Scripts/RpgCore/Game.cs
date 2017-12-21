using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu()]
public class Game : ScriptableObject
{
    public List<Character> party;
    public string currentScene;
    public Vector3 position;

    public string getGameDesc()
    {
        string partyDesc = "";
        for (int i = 0; i < party.Count; i++)
        {
            partyDesc += party[i].name;
            if (i <= party.Count - 1)
                partyDesc += " / ";
        }
        return partyDesc;
    }


}


