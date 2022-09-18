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

public class EventHandlerGroup
{
    Dictionary<int, EventHandler>           m_eventHandlers;
    public EventHandlerGroup( Type enumType )
    {
        m_eventHandlers                     = new Dictionary<int, EventHandler>();
    }
    
    ~EventHandlerGroup()
    {

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


    public void fireEvent(int eventID, object sender, EventArgs args)
    {
        EventHandler handlerGroup;
        if (m_eventHandlers.TryGetValue(eventID, out handlerGroup))
        {
            if (handlerGroup != null)
                handlerGroup(sender, args);
        }
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
