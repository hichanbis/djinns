using UnityEngine;
using System.IO;

public class SavableScriptableObject : ScriptableObject
{

    public virtual void Save(int index)
    {
        string path = Application.persistentDataPath + Path.DirectorySeparatorChar + "slot" + index + Path.DirectorySeparatorChar + GetType().Name + ".json";
        //SetSatisfiedConditionsBeforeSave();
        SaveLoad.SaveToFile(this, path);
    }

    public virtual void Load(int index)
    {
        string path = Application.persistentDataPath + Path.DirectorySeparatorChar + "slot" + index + Path.DirectorySeparatorChar + GetType().Name + ".json";
        SaveLoad.LoadFromFile(this, path);
        //SetConditionsToSavedStatusAfterLoad();
    }
}
