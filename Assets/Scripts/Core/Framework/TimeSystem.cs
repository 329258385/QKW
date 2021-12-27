using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Solarmax
{
    public class TimeSystem : Singleton<TimeSystem>, Lifecycle
    {
        private DateTime _BaseTime = new DateTime(1970, 1, 1); // 不能声明为const

        private int mFrames;
        private double mTotalTickSeconds;
        private double mLocalStartTime;
        private float mRemoteTimeOffset;


		/// <summary>
		/// 用于服务器时间
		/// </summary>
		private DateTime serverTimeBegin; // utc time
		private DateTime localTimeBegin;

        public TimeSystem()
        {
            mFrames = 0;
            mTotalTickSeconds = 0;
            mLocalStartTime = 0;
            mRemoteTimeOffset = 0;
        }

        public bool Init()
        {
			LoggerSystem.Instance.Debug("TimeSystem    init  begin");

            this.mLocalStartTime = this.GetMillisecods();
            this.mTotalTickSeconds = 0;

            LoggerSystem.Instance.Debug("TimeSystem    init  end");
            return true;
        }

        public void Tick(float interval)
        {
            //LoggerSystem.Instance.Debug("每tick  时间s: " + interval);

            this.mFrames++;
            this.mTotalTickSeconds += interval;
        }

        public void Destroy()
        {
			LoggerSystem.Instance.Debug("TimeSystem    destroy  begin");

			LoggerSystem.Instance.Debug("TimeSystem    destroy  end");
        }

        public float GetRemoteTimeOffset()
        {
            return mRemoteTimeOffset;
        }

        public void SetRemoteTimeOffset(float timeOffset)
        {
            mRemoteTimeOffset = timeOffset;
        }

        public double GetMillisecods()
        {
            return DateTime.Now.Subtract(_BaseTime).TotalMilliseconds;
        }

        public double GetLocalMilliseconds()
        {
            return this.GetMillisecods() - this.mLocalStartTime;
        }

        public double GetServerMilliseconds()
        {
            return this.GetLocalMilliseconds() - this.mRemoteTimeOffset;
        }

        public void ResetFrame()
        {
            this.mFrames = 0;
        }

        public int GetFrame()
        {
            return mFrames;
        }

        public double GetRunTime()
        {
            return mTotalTickSeconds;
        }

        public DateTime GetClientTime()
        {
            return DateTime.Now;
        }

        /*
        public DateTime GetServerTime()
        {

        }
        */

        public int GetIntervalDay(double start, double end)
        {
            double intervalMilliseconds = end - start;
            int intervalDay = (int)(intervalMilliseconds / (1000 * 60 * 60 * 24));
            return intervalDay;
        }

		/// <summary>
		/// 设置服务器开始时间
		/// </summary>
		/// <param name="timeStamp">Time stamp.</param>
		public void SetServerTime(int timeStamp)
		{
			serverTimeBegin = new DateTime (1970, 1, 1);
			serverTimeBegin = serverTimeBegin.AddSeconds (timeStamp);
			//serverTimeBegin = TimeZone.CurrentTimeZone.ToLocalTime (serverTimeBegin);

			localTimeBegin = DateTime.Now;
		}

		/// <summary>
		/// 获取服务器时间，大致
		/// </summary>
		/// <returns>The server time.</returns>
		public DateTime GetServerTime()
		{
			TimeSpan ts = (DateTime.Now - localTimeBegin);
			return serverTimeBegin.Add (ts);
		}

		public DateTime GetServerTimeCST()
		{
			return TimeZone.CurrentTimeZone.ToLocalTime (GetServerTime());
		}

		public DateTime GetTime (int seconds)
		{
			DateTime dt = new DateTime (1970, 1, 1);
			dt = dt.AddSeconds (seconds);

			return dt;
		}

		public DateTime GetTimeCST (int seconds)
		{
			return TimeZone.CurrentTimeZone.ToLocalTime (GetTime (seconds));
		}

		public DateTime GetTimeCST (DateTime dt)
		{
			return TimeZone.CurrentTimeZone.ToLocalTime (dt);
		}
    }
}
