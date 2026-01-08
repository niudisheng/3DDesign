using System.Collections.Generic;
using UnityEngine.Events;

public enum EventName
{
    GameStart,
    PlayerHealthChange,
}

public interface IEventInfo
{
}

public class EventInfo : IEventInfo
{
    public UnityAction Actions;

    public EventInfo(UnityAction action)
    {
        Actions += action;
    }
}

public class EventInfo<T> : IEventInfo
{
    public UnityAction<T> Actions;

    // 泛型类构造函数不需要带尖括号<>
    public EventInfo(UnityAction<T> action)
    {
        Actions += action;
    }
}
public class EventInfoInparams : IEventInfo
{
    public UnityAction<object[]> Actions;

    public EventInfoInparams(UnityAction<object[]> action)
    {
        Actions += action;
    }

    // 新的重载构造函数，支持多种形式
    public EventInfoInparams(params UnityAction<object[]>[] actions)
    {
        foreach (var action in actions)
        {
            Actions += action;
        }
    }
}
public class MyEventManager
{
    private static MyEventManager _instance;
    
    public static MyEventManager Instance => _instance ??= new MyEventManager();

    private readonly Dictionary<string, IEventInfo> _eventDic;

    private MyEventManager()
    {
        _eventDic = new Dictionary<string, IEventInfo>();
    }

    #region 添加事件监听
    public void AddEventListener(string eventName, UnityAction action)
    {
        if (_eventDic.ContainsKey(eventName))
        {
            (_eventDic[eventName] as EventInfo).Actions += action;
        }
        else
        {
            _eventDic.Add(eventName, new EventInfo(action));
        }
    }

    // 删除事件监听
    public void RemoveEventListener(string eventName, UnityAction action)
    {
        if (_eventDic.ContainsKey(eventName))
        {
            (_eventDic[eventName] as EventInfo).Actions -= action;
        }
    }

    // 事件触发器
    public void EventTrigger(string eventName)
    {
        if (_eventDic.ContainsKey(eventName))
        {
            (_eventDic[eventName] as EventInfo).Actions?.Invoke();
        }
    }
    #endregion
    
    #region EventName枚举版本
    public void AddEventListener(EventName eventName, UnityAction action)
    {
        AddEventListener(eventName.ToString(), action);
        
    }

    // 删除事件监听
    public void RemoveEventListener(EventName eventName, UnityAction action)
    {
        RemoveEventListener(eventName.ToString(), action);
    }

    // 事件触发器
    public void EventTrigger(EventName eventName)
    {
        EventTrigger(eventName.ToString());
    }
    #endregion
    #region 泛型版本
    public void AddEventListener<T>(string eventName, UnityAction<T> action)
    {
        if (_eventDic.ContainsKey(eventName))
        {
            (_eventDic[eventName] as EventInfo<T>).Actions += action;
        }
        else
        {
            _eventDic.Add(eventName, new EventInfo<T>(action));
        }
    }

    public void RemoveEventListener<T>(string eventName, UnityAction<T> action)
    {
        if (_eventDic.ContainsKey(eventName))
        {
            (_eventDic[eventName] as EventInfo<T>).Actions -= action;
        }
    }

    public void EventTrigger<T>(string eventName, T info)
    {
        if (_eventDic.ContainsKey(eventName))
        {
            (_eventDic[eventName] as EventInfo<T>).Actions?.Invoke(info);
        }
    }
    #endregion

    #region 泛型版本EventName枚举

    public void AddEventListener<T>(EventName eventName, UnityAction<T> action)
    {
        AddEventListener<T>(eventName.ToString(), action);
    }

    public void RemoveEventListener<T>(EventName eventName, UnityAction<T> action)
    {
        RemoveEventListener<T>(eventName.ToString(), action);
    }

    public void EventTrigger<T>(EventName eventName, T info)
    {
        EventTrigger<T>(eventName.ToString(), info);
    }

    #endregion

    #region 多个参数试做,用装箱拆箱来实现
    public void AddEventListener(string eventName, UnityAction<object[]> action)
    {
        if (_eventDic.ContainsKey(eventName))
        {
            (_eventDic[eventName] as EventInfoInparams).Actions += action;
        }
        else
        {
            _eventDic.Add(eventName, new EventInfoInparams(action));
        }
    }

    public void RemoveEventListener(string eventName, UnityAction<object[]> action)
    {
        if (_eventDic.ContainsKey(eventName))
        {
            (_eventDic[eventName] as EventInfoInparams).Actions -= action;
        }
    }
    

    public void EventTrigger(string eventName, params object[] info)
    {
        if (_eventDic.ContainsKey(eventName))
        {
            (_eventDic[eventName] as EventInfoInparams).Actions?.Invoke(info);
        }
    }
    #endregion

    // 清除所有事件
    public void ClearAllEvent()
    {
        _eventDic.Clear();
    }
}

