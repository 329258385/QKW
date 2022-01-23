using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Solarmax
{
	public class Event2 : IPoolObject<Event2>
	{
		private int mKey;
		private object[] mArgs;

		public void Set(int key, object[] args)
		{
			mKey = key;
			mArgs = args;
		}

		public int GetKey()
		{
			return mKey;
		}

		public  object[] GetArgs()
		{
			return mArgs;
		}
	}

	public enum EventID
    {
		enumInvalidEvent			= 0,

		enumCharEvent_Begin			= 2000,
		enumCharEvent_BeHit			= 2001,			// 角色被hit事件
		enumCharEvent_UseSkill		= 2202,			// 角色使用技能
		enumCharEvent_KillMonster	= 2003,			// 杀死怪物
		enumCharEvent_HitTarget		= 2004,			// 命中
		enumCharEvent_End,
    }

	public class KEvent
    {
		public virtual EventID ID
        {
			get { return EventID.enumInvalidEvent; }
        }
    }


	public class KBeHit : KEvent
    {
        public override EventID ID
        {
			get { return EventID.enumCharEvent_BeHit; }
        }

		public BattleMember		Src;
		public BattleMember		Target;
		public TechniqueEntiy	technique;
		public int				hurt;
	}

	public class KMonster : KEvent
    {
		public override EventID ID
		{
			get { return EventID.enumCharEvent_KillMonster; }
		}

		public BattleMember		killer;
		public BattleMember		dead;
	}
}
