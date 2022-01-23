using System;
using System.Collections.Generic;
using System.Linq;




/// <summary>
/// 消息组，消息代理
/// </summary>

public enum EventTypeGroup
{
    On1TouchDown,
    On1TouchMove,
    On1TouchUp,
    On2TouchMove,
}

public class EventHandlerGroup : Singleton<EventHandlerGroup>
{
    [System.NonSerialized]
    Solarmax.EventHolder                    m_eventList = new Solarmax.EventHolder();

    Dictionary<int, EventHandler>           m_eventHandlers;
    public EventHandlerGroup()
    {
        m_eventHandlers                     = new Dictionary<int, EventHandler>();
    }

    public void AddEvent(Type enumType)
    {
        var events = Enum.GetValues(enumType);
        foreach (var i in events)
        {
            AddEvent((int)i);
        }
    }

    public void AddEvent( int eventID )
    {
        if (!m_eventHandlers.ContainsKey(eventID))
        {
            m_eventHandlers.Add(eventID, null);
        }
    }

    public void addEventHandler( int eventID, EventHandler handler )
    {
        if( m_eventHandlers.ContainsKey(eventID) == true )
        {
            WeakReference weak          = new WeakReference(handler);
            m_eventHandlers[eventID]    += (EventHandler)weak.Target;
        }
    }

    public void clearEventHandler(int eventID)
    {
        if (m_eventHandlers.ContainsKey(eventID) == true)
        {
            m_eventHandlers[eventID] = null;
        }
    }

    public void delEventHandler(int eventID, EventHandler handler)
    {
        if (m_eventHandlers.ContainsKey(eventID) == true)
        {
            m_eventHandlers[eventID] -= handler;
        }
    }

    public uint RegEvent( Solarmax.EventID eventID, uint key )
    {
        m_eventList.RegEventHandler(eventID, key);
    }

    public void RemoveEvent( Solarmax.EventID eventID, uint key )
    {
        m_eventList.RemoveEventHandler(eventID, key);
    }


    public void fireEvent(int eventID, object sender, EventArgs args)
    {
        EventHandler handlerGroup;
        if (m_eventHandlers.TryGetValue(eventID, out handlerGroup))
        {
            if (handlerGroup != null)
                handlerGroup(sender, args);
        }
    }

    public void FireEvent( Solarmax.KEvent evt )
    {
        m_eventList.OnEvent(evt);
    }


    public void clearEvents()
    {
        var keys = m_eventHandlers.Keys.ToArray();
        for (int i = 0; i < keys.Length; ++i)
        {
            m_eventHandlers[keys[i]] = null;
        }
    }
}
