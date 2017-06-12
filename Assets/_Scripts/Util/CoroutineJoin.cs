using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CoroutineJoin
{
    List<bool> _subTasks = new List<bool>();

    private readonly MonoBehaviour _owningComponent;

    public CoroutineJoin(MonoBehaviour owningComponent)
    {
        _owningComponent = owningComponent;
    }

    public void StartSubtask(IEnumerator routine)
    {
        _subTasks.Add(false);
        _owningComponent.StartCoroutine(StartJoinableCoroutine(_subTasks.Count - 1, routine));
    }

    public Coroutine WaitForAll()
    {
        return _owningComponent.StartCoroutine(WaitForAllSubtasks());
    }

    private IEnumerator WaitForAllSubtasks()
    {
        while (true)
        {
            bool completedCheck = true;
            for (int i = 0; i < _subTasks.Count; i++)
            {
                if (!_subTasks[i])
                {
                    completedCheck = false;
                    break;
                }
            }

            if (completedCheck)
            {
                break;
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator StartJoinableCoroutine(int index, IEnumerator coroutine)
    {
        yield return _owningComponent.StartCoroutine(coroutine);
        _subTasks[index] = true;
    }
}
