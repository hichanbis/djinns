using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu()]
public class StatusCollection : ScriptableObject
{
    public List<Status> statuses;

    public Status GetStatusFromId(string id)
    {
        return statuses.Find(s => s.name.Equals(id));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        LoadStatuses();
    }

    private void OnEnable()
    {
        EditorApplication.projectChanged -= LoadStatuses;
        EditorApplication.projectChanged += LoadStatuses;
    }

    private void OnDisable()
    {
        EditorApplication.projectChanged -= LoadStatuses;
    }

    private void LoadStatuses()
    {
        statuses = EditorUtils.FindAssetsByType<Status>("Assets/_Databases/Statuses");
    }
#endif
}

