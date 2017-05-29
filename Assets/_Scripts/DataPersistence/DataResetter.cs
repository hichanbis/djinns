using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataResetter : MonoBehaviour
{

    public List<SaveData> saveDataList;
    // Use this for initialization
    void Awake()
    {
        foreach (SaveData saveData in saveDataList)
        {
            saveData.Reset();
        }
    }
}
