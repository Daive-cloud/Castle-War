using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventCenter
{
    public static EventCenter instance;
    public Dictionary<string, IEventInfo> eventDictionary = new();
    public static EventCenter Instance
    {
        get
        {
            instance ??= new EventCenter();
            return instance;
        }
    }

    public void AddEventListener(string _event, UnityAction _action)
    {
        if (eventDictionary.ContainsKey(_event))
        {
            (eventDictionary[_event] as EventInfo).actions += _action;
        }
        else
        {
            eventDictionary.Add(_event, new EventInfo(_action));
        }
    }
    public void AddEventListener<T>(string _event, UnityAction<T> _action)
    {
        if (eventDictionary.ContainsKey(_event))
        {
            (eventDictionary[_event] as EventInfo<T>).actions += _action;
        }
        else
        {
            eventDictionary.Add(_event, new EventInfo<T>(_action));
        }
    }

    public void EventTrigger(string _event)
    {
        if (eventDictionary.ContainsKey(_event))
        {
            var info = eventDictionary[_event] as EventInfo;
            if (info.actions != null)
            {
                info.actions.Invoke();
            }
        }
    }

    public void EventTrigger<T>(string _event,T _parm)
    {
        if (eventDictionary.ContainsKey(_event))
        {
            var info = eventDictionary[_event] as EventInfo<T>;
            if (info.actions != null)
            {
                info.actions.Invoke(_parm);
            }
        }
    }
    public void RemoveEventListener(string _event, UnityAction _action)
    {
        if (eventDictionary.ContainsKey(_event))
        {
            (eventDictionary[_event] as EventInfo).actions -= _action;
        }
    }
    
    public void RemoveEventListener<T>(string _event, UnityAction<T> _action)
    {
        if (eventDictionary.ContainsKey(_event))
        {
            (eventDictionary[_event] as EventInfo<T>).actions -= _action;
        }
    }

    public void Clear() => eventDictionary.Clear();

}

public interface IEventInfo { }

public class EventInfo : IEventInfo
{
    public UnityAction actions; // C#的委托是多播委托，其中维护了一个事件列表
    public EventInfo(UnityAction _action)
    {
        actions += _action;
    }
}

public class EventInfo<T> : IEventInfo
{
    public UnityAction<T> actions;

    public EventInfo(UnityAction<T> _action)
    {
        actions += _action;
    }
}


