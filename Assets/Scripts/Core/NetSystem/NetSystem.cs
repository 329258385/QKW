using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;






namespace Solarmax
{
	public enum NetCtr
	{
		Lobby = 1,
		Room,
	}

    public class NetSystem : Singleton<NetSystem>, Lifecycle
    {
        /**
         * 本类进行实例化正式的网络连接模型
         * 提供各种模型的选择
         * 1，tcp
         * 2，udp
         * 3，websocket
         * */

        private Dictionary<int, INetConnector>  mConnectorMap;

		public PacketHelper                     helper;

		public PingHelper                       ping;

        public NetSystem()
        {
			helper          = new PacketHelper ();
			ping            = new PingHelper ();
            mConnectorMap   = new Dictionary<int, INetConnector>();
        }

        public bool Init()
		{

			LoggerSystem.Instance.Debug("NetSystem    init  begin");

			// 注册链接
			RegisterConnector ((int)NetCtr.Lobby, ConnectionType.TCP, ConnectedCallback, null, DisConnectedCallback, null);

			// 初始化
            foreach (var i in mConnectorMap.Values)
            {
                i.Init();
            }

			helper.RegisterAllPacketHandler ();
			ping.Init ();


			LoggerSystem.Instance.Debug("NetSystem    init  end");
            return true;
        }

        public void Tick(float interval)
        {
            foreach (var i in mConnectorMap.Values)
            {
                i.Tick(interval);
            }

			ping.Tick (interval);
        }

        public void Destroy()
		{
			LoggerSystem.Instance.Debug("NetSystem    destroy  begin");

            foreach (var i in mConnectorMap.Values)
            {
                i.Destroy();
			}
			ping.Destroy ();

			LoggerSystem.Instance.Debug("NetSystem    destroy  end");
        }

        public void RegisterConnector(int uid, ConnectionType type, Callback<bool> connected, Callback<int, Byte[]> recieved, Callback disconnected, Callback error)
        {
            INetConnector ctor  = new TCPConnector(); ;
            ctor.OnConnected    = connected;
            ctor.OnRecieved     = recieved;
            ctor.OnDisconnected = disconnected;
            ctor.OnError        = error;
            ctor.SetUid(uid);

            mConnectorMap.Add(uid, ctor);
        }

        public void Connect(int uid, string address, int port)
        {
            if (mConnectorMap.ContainsKey(uid))
            {
                mConnectorMap[uid].Connect(address, port);
            }
        }

		public void Connect(string address, int port)
		{
			Connect ((int)NetCtr.Lobby, address, port);
		}

		public void Send<T>( int packetId, T proto) where T : class
        {
            int uid = (int)NetCtr.Lobby;
            if (mConnectorMap.ContainsKey(uid))
            {
				NetPacket pa = new NetPacket (packetId);
                pa.EncodeProto<T>(proto);
				mConnectorMap[uid].SendPacket(pa);
            }
        }

        public void Close(int uid)
        {
            if (mConnectorMap.ContainsKey(uid))
            {
                mConnectorMap[uid].DisConnect();
            }
        }

		public void Close()
		{
			Close ((int)NetCtr.Lobby);
		}

        public INetConnector GetConnector(int uid)
        {
            INetConnector ret = null;
            mConnectorMap.TryGetValue(uid, out ret);

            return ret;
        }

		public INetConnector GetConnector()
		{
			return GetConnector ((int)NetCtr.Lobby);
		}

		public void ConnectedCallback(bool status)
		{
			LoggerSystem.Instance.Info ("已连接服务器：" + GetConnector().GetHost().ToString());
		}

		public void DisConnectedCallback()
		{
			#if SERVER
			BattleSystem.Instance.battleData.RefereeBusy = false;
			BattleSystem.Instance.battleData.RefereeFinish = true;
			#else

			//Tips.Make(Tips.TipsType.FlowUp, "网络已断开！", 1);
			ping.Pong (-1);
			EventSystem.Instance.FireEvent (EventId.NetworkStatus, false);

			// 如果被踢，则不重连
			if (LocalPlayer.Get ().isAccountTokenOver)
			{
				return;
			}

			// 此处判断一下是否是在游戏中，如果在PVP游戏中，则重新resume
			BattleData battleData = BattleSystem.Instance.battleData;
			if (battleData.gameState == GameState.Game || battleData.gameState == GameState.GameWatch || battleData.gameState == GameState.Watcher)
			{
				if (battleData.gameType == GameType.PVP || battleData.gameType == GameType.League)
				{
					EventSystem.Instance.FireEvent (EventId.OnBattleDisconnect);
				}
			}
			else
			{
				if (battleData.gameType == GameType.PVP || battleData.gameType == GameType.League)
				{
					BattleSystem.Instance.Reset ();

					UISystem.Get ().HideAllWindow ();
					UISystem.Get ().ShowWindow ("LogoWindow");
				}
			}

			#endif
		}
    }
}
