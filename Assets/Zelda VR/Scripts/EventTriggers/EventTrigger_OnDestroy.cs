using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class EventTrigger_OnDestroy : MonoBehaviour
{
    [Serializable]
    public class TriggerDelegates : UnityEvent<BaseEventData> { }


    [SerializeField]
    internal List<TriggerDelegates> _delegates;


    void OnDestroy()
    {
        InvokeDelegates(null);
    }

    void InvokeDelegates(BaseEventData eventData)
    {
        foreach (TriggerDelegates t in _delegates)
        {
            if (t == null)
                continue;

            t.Invoke(eventData);
        }
    }
}