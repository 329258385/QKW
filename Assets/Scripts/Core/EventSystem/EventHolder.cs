using System;
using System.Collections.Generic;
using UnityEngine;



namespace Solarmax
{
    
    public class EventHolder
    {
        readonly static int MAX_EVENT_STACKS = 10;
        class KEventHandler
        {
            public uint m_id = 0;
            public uint m_key = 0;
			public uint m_customData = 0;
			public IEventInterface m_EI = null;
			public bool m_bDeleteFlag = false;
        }
        Dictionary<EventID, List<KEventHandler>> m_Events = new Dictionary<EventID, List<KEventHandler>>();

		private int		m_handlingEventStack;
        // 上次呼吸时间
        private float   m_lastBreatheTime = 0;
        private uint    nextID = 1;

        public void Clear()
        {
            m_Events.Clear();
            m_lastBreatheTime = 0;
        }

		/**
		 * 注册一个新事件响应器
		 * param:
		 *		eventID:		事件id
		 *		pEI:			响应对象
		 *		key:			事件响应器的用户定义Key
		 * return:
		 *		如果成功，返回事件响应器的唯一标识id，否则返回0
		 */
		public uint RegEventHandler(EventID eventID, uint key = 0, uint customData = 0)
		{
			if (!m_Events.ContainsKey(eventID))
			{
				List<KEventHandler> eventList = new List<KEventHandler>();
				m_Events.Add(eventID, eventList);
			}
			KEventHandler EH = new KEventHandler();
			EH.m_id = nextID++;
			EH.m_key = key;
			EH.m_customData = customData;
			m_Events[eventID].Insert(0, EH);
			return EH.m_id;
		}

		/**
		 * 响应事件
		 * param:
		 *		eventID:		事件id
		 *		pEventData:		事件的关联数据
		 */
		public void OnEvent( KEvent EventData)
		{
			if (m_handlingEventStack >= MAX_EVENT_STACKS)
			{
				return;
			}
			if (!m_Events.ContainsKey(EventData.ID))
			{
				return;
			}

			EventID eventID = EventData.ID;
			m_handlingEventStack++;
			List<KEventHandler> eventList = m_Events[eventID];
			for (int i = eventList.Count - 1; i >= 0; i--)
			{
				//最外层，可以对事件进行真正的删除处理
				KEventHandler handler = eventList[i];
				if (!handler.m_bDeleteFlag)
				{
					if (handler.m_EI.OnEvent(eventID, EventData, handler.m_customData))
					{
						handler.m_bDeleteFlag = true;
					}
				}
				else
				{
					if (m_handlingEventStack == 1)
					{
						eventList.RemoveAt(i);
					}
				}
			}
			m_handlingEventStack--;
		}

		/**
		* 根据EventID和Key移除所有的响应EventID的与key关联的事件响应器
		* param:
		*		eventID:	事件id
		*		key:		key值(为0认为移除全部)
		*/
		public void RemoveEventHandler(EventID eventID, uint key)
		{
			if (!m_Events.ContainsKey(eventID))
				return;
			var e = m_Events[eventID].GetEnumerator();
			while (e.MoveNext())
			{
				KEventHandler handler = e.Current;
				if (handler.m_key == key || key == 0)
					handler.m_bDeleteFlag = true;
			}
			e.Dispose();
		}

		/**
		* 根据Key移除所有的与其关联的事件响应器
		* param:
		* key:		key值
		*/
		public void RemoveEventHandlerByKey(uint key)
		{
			var e = m_Events.GetEnumerator();
			while (e.MoveNext())
			{
				KeyValuePair<EventID, List<KEventHandler>> pair = e.Current;
				List<KEventHandler> eventList = pair.Value;

				var e2 = eventList.GetEnumerator();
				while (e2.MoveNext())
				{
					KEventHandler handler = e2.Current;
					if (handler.m_key == key)
						handler.m_bDeleteFlag = true;
				}
				e2.Dispose();
			}
			e.Dispose();
		}

		/**
		* 根据id删除唯一一个Trigger
		* param:
		*		id:			id值
		*/
		public void RemoveEventHandlerByID(uint id)
		{
			var e = m_Events.GetEnumerator();
			while (e.MoveNext())
			{
				KeyValuePair<EventID, List<KEventHandler>> pair = e.Current;
				List<KEventHandler> eventList = pair.Value;

				var e2 = eventList.GetEnumerator();
				while (e2.MoveNext())
				{
					KEventHandler handler = e2.Current;
					if (handler.m_id == id)
						handler.m_bDeleteFlag = true;
				}
				e2.Dispose();
			}
			e.Dispose();
		}

		/**
		* 根据事件处理对象的指针移除所有跟key相关的响应器，如果key填0则移除所有
		* param:
		*		pEI:		事件响应器
		*		key:		key值
		*		eventID:	事件id
		*/
		public void RemoveEventHandler(IEventInterface EI, uint key = 0, EventID eventID = EventID.enumInvalidEvent)
		{
			if (eventID != EventID.enumInvalidEvent)
			{
				if (!m_Events.ContainsKey(eventID))
					return;
				var e = m_Events[eventID].GetEnumerator();
				while (e.MoveNext())
				{
					KEventHandler handler = e.Current;
					if (handler.m_EI == EI && (key == 0 || handler.m_key == key))
						handler.m_bDeleteFlag = true;
				}
				e.Dispose();
			}
			else
			{
				var e = m_Events.GetEnumerator();
				while (e.MoveNext())
				{
					KeyValuePair<EventID, List<KEventHandler>> pair = e.Current;
					List<KEventHandler> eventList = pair.Value;

					var e2 = eventList.GetEnumerator();
					while (e2.MoveNext())
					{
						KEventHandler handler = e2.Current;
						if (handler.m_EI == EI && (key == 0 || handler.m_key == key))
							handler.m_bDeleteFlag = true;
					}
					e2.Dispose();
				}
				e.Dispose();
			}
		}

		public void Update()
		{
			if (Time.time > m_lastBreatheTime)
			{
				CheckAndRemoveOnce();
				// 5-10分钟检查一次
				m_lastBreatheTime += 5000;//1000 * 60 * 5 + rand() % 300;
			}
		}

		protected void CheckAndRemoveOnce()
		{
			var e = m_Events.GetEnumerator();
			while (e.MoveNext())
			{
				KeyValuePair<EventID, List<KEventHandler>> pair = e.Current;
				List<KEventHandler> eventList = pair.Value;
				for (int i = eventList.Count - 1; i >= 0; i--)
				{
					KEventHandler handler = eventList[i];
					if (handler.m_bDeleteFlag)
						eventList.RemoveAt(i);
				}
			}
			e.Dispose();
		}
	}
}
