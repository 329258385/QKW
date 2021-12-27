using System;
using System.Collections.Generic;

namespace Solarmax
{
	public class DataHandlerSystem : Singleton<DataHandlerSystem>, Lifecycle
	{
		private List<IDataHandler> mDataHandler = new List<IDataHandler>();

		public bool Init ()
		{
			LoggerSystem.Instance.Debug ("DataHandlerSystem   init   begin");

			// 注册数据管理器
			RegistDataHandler (FriendDataHandler.Instance);
			RegistDataHandler (NoticeDataHandler.Instance);
			RegistDataHandler (LevelDataHandler.Instance);




			bool ret = doInit ();

			LoggerSystem.Instance.Debug ("DataHandlerSystem   init   end");

			return ret;
		}

		public void Tick (float interval)
		{
			for (int i = 0, max = mDataHandler.Count; i < max; ++i) {
				mDataHandler [i].Tick (interval);
			}
		}

		public void Destroy ()
		{
			mDataHandler.Clear ();
		}

		private bool doInit ()
		{
			bool ret = true;
			for (int i = 0, max = mDataHandler.Count; i < max; ++i) {
				ret &= mDataHandler [i].Init ();
			}
			return ret;
		}

		private void RegistDataHandler (IDataHandler dataHandler)
		{
			mDataHandler.Add (dataHandler);
		}

	}
}
