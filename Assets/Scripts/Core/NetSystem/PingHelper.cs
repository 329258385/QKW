using System;
using UnityEngine;

namespace Solarmax
{
	public class PingHelper : Lifecycle
	{
		/// <summary>
		/// 上次ping值
		/// </summary>
		public float lastPingTime;

		/// <summary>
		/// 最大可容忍无响应ping数量
		/// </summary>
		public int MaxNoReplyCount;

		/// <summary>
		/// ping包间隔时间
		/// </summary>
		public int pingInterval;

		/// <summary>
		/// 当前无响应次数
		/// </summary>
		private int noReplyCount;

		private float timeCount;

		private bool enable;

		/// <summary>
		/// 上一次网络状态
		/// </summary>
		private int lastNetState;

		public void Reset ()
		{
			lastPingTime = 0;
			MaxNoReplyCount = 3;
			pingInterval = 2;
			noReplyCount = 0;

			lastNetState = EngineSystem.Instance.GetNetworkRechability ();
		}


		public bool Init ()
		{
			Reset ();

			enable = true;

			return true;
		}

		public void Tick (float interval)
		{
			if (enable) {

				timeCount += interval;

				if (timeCount > pingInterval) {
					timeCount -= pingInterval;

					Ping ();
				}
			}
		}

		public void Destroy ()
		{
			enable = false;
			Reset ();
		}

		/// <summary>
		/// 开始ping
		/// </summary>
		private void Ping ()
		{
			if (noReplyCount >= MaxNoReplyCount)
			{
				if (NetSystem.Instance.GetConnector ().GetConnectStatus () == ConnectionStatus.CONNECTED) {

					Debug.LogFormat("由于连续{0}次未收到Ping回复，主动断开连接！", noReplyCount);
					// 断开网络连接
					NetSystem.Instance.Close ();
				}

				noReplyCount = 0;
				lastPingTime = 0;
			}

			if (NetSystem.Instance.GetConnector().GetConnectStatus () == ConnectionStatus.CONNECTED) {
				// 发送ping
				NetSystem.Instance.helper.PingNet ();
			}

			int nowNetState = EngineSystem.Instance.GetNetworkRechability ();
			if (lastNetState != nowNetState) {
				lastNetState = nowNetState;

				Debug.LogFormat("由于网络状态改变，主动断开连接！", noReplyCount);
				NetSystem.Instance.Close ();
			}

			++noReplyCount;
		}

		/// <summary>
		/// pong回调
		/// </summary>
		public void Pong (float time)
		{
			lastPingTime = time;
			noReplyCount = 0;
		}

		public void GetNetPic(out string pic, out Color color)
		{
			pic = "icon_net_offline";
			color = Color.red;

			if (lastPingTime > 150)
				color = Color.red;
			else if (lastPingTime > 100)
				color = Color.yellow;
			else if (lastPingTime > 0)
				color = Color.green;

			int nettype = EngineSystem.Instance.GetNetworkRechability ();
			if (nettype == 0) {
				pic = "icon_net_offline";
			} else if (nettype == 1) {
				if (lastPingTime > 150)
					pic = "icon_net_mobile_01";
				else if (lastPingTime > 100)
					pic = "icon_net_mobile_02";
				else if (lastPingTime > 0)
					pic = "icon_net_mobile_03";
			} else if (nettype == 2) {
				if (lastPingTime > 150)
					pic = "icon_net_wifi_01";
				else if (lastPingTime > 100)
					pic = "icon_net_wifi_02";
				else if (lastPingTime > 0)
					pic = "icon_net_wifi_03";
			}
		}
	}
}

