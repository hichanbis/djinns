using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class StatusCollection : ScriptableObject
{
    public List<Status> statuses;

    public Status FindStatusFromId(string id)
    {
        return statuses.Find(s => s.id.Equals(id));
    }
}

