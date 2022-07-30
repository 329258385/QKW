using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Plugin;
using Solarmax;





/// <summary>
/// 场景管理
/// </summary>
public class SceneManager : Lifecycle2
{
    

	/// <summary>
	/// 传送管理
	/// </summary>
	/// <value>The warp manager.</value>
	public WarpManager          warpManager { get; set; }

	/// <summary>
	/// 队伍管理 
	/// </summary>
	/// <value>The team mnager.</value>
	public TeamManager          teamManager { get; set; }

	/// <summary>
	/// 星球管理
	/// </summary>
	/// <value>The node manager.</value>
	public NodeManager          nodeManager { get; set; }

	/// <summary>
	/// 飞船管理
	/// </summary>
	/// <value>The ship manager.</value>
	public ShipManager          shipManager { get; set; }

	/// <summary>
	/// ai管理
	/// </summary>
	/// <value>The ai manager.</value>
	public AIManager            aiManager { get; set; }


	// 战斗持续时间
	float                       m_BattleTime = 0f;

	/// <summary>
	/// 战斗节凑调整系数
	/// </summary>
	public float                battleScaleSpeed = 1;

	/// <summary>
	/// 战斗数据
	/// </summary>
	/// <value>The battle manager.</value>
	public BattleData				battleData { get; set; }


	/// <summary>
	/// 初始化
	/// </summary>
	public SceneManager(BattleData bd)
	{
		//战斗数据引用
		battleData      = bd;

		//组队管理
		teamManager     = new TeamManager(this);
		//星球
		nodeManager     = new NodeManager(this);
		//飞船
		shipManager     = new ShipManager(this);
		// AI
		aiManager       = new AIManager(this);
		//传送
		warpManager     = new WarpManager();
	}

	public bool Init()
	{
		if (!teamManager.Init())
			return false;
		if (!nodeManager.Init())
			return false;
		if (!shipManager.Init())
			return false;
		if (!aiManager.Init())
			return false;
		if (!warpManager.Init())
			return false;
		return true;
	}

	public void Tick(int frame, float interval)
	{
		teamManager.Tick(frame, interval);
		nodeManager.Tick(frame, interval);
		shipManager.Tick(frame, interval);
		aiManager.Tick(frame, interval);
		warpManager.Tick(frame, interval);
		m_BattleTime += interval;
	}

	public void Destroy()
	{
		teamManager.Destroy();
		nodeManager.Destroy();
		shipManager.Destroy();
		aiManager.Destroy();
		warpManager.Destroy();
		m_BattleTime = 0f;
	}


	/// <summary>
	/// 释放
	/// </summary>
	public void Release()
	{

	}

	/// <summary>
	/// 重置
	/// </summary>
	public void Reset()
	{
		// 设置战斗总时间
		m_BattleTime = 0f;
	}


	public float GetBattleTime()
	{
		return m_BattleTime;
	}

	/// <summary>
	/// 执行帧包
	/// </summary>
	public void RunFramePacket(FrameNode node)
	{
		if (node.msgList == null || node.msgList.Length == 0)
			return;
		for (int i = 0; i < node.msgList.Length; i++) {
			ExcutePacket(node.msgList[i] as Packet);
		}
	}

	void ExcutePacket(Packet packet)
	{
		if (packet.packet.type == 0) 
        {
            // 暂时屏蔽网络移动
			//nodeManager.MoveTo(packet.packet.move, packet.team);
		}
		else if (packet.packet.type == 1) 
        {
			//newSkillManager.OnCast(packet.packet.skill);
		}
		else if (packet.packet.type == 2) {
			// 投降包
			BattleSystem.Instance.OnPlayerGiveUp(packet.packet.giveup.team);
        }
	}


	/// <summary>
	/// 创建场景
	/// </summary>
	public void CreateScene(IList<int> usr, bool isEditer = false)
	{
		MapConfig table = MapConfigProvider.Instance.GetData (battleData.matchId);
		if (table == null)
        {
			Release ();
			UISystem.Get ().HideAllWindow ();
			UISystem.Get ().ShowWindow ("LogoWindow");
			return;
		}

		CreateScene (isEditer, table);
	}

	public void CreateScene (bool isEditer, MapConfig table)
	{
		battleData.currentTable = table;

		//创建星球
		CreateNode (table.builds);

		//创建障碍物
		CreateLines(table.lines);

		// 创建战队
		CreatreLocalBattleTeam();

		// 根据地图配置
		CreatePVEBattleTeams(table.players);
	}

	public void CreateBattleScene(MapConfig table, List<BuildTypeBehaviour> mapList )
    {
		foreach (BuildTypeBehaviour item in mapList)
		{
			/// 3D空间不显示障碍物和障碍线
			NodeType eType = (NodeType)item.nodeType;
			if (!battleData.mapEdit && (eType == NodeType.Barrier || eType == NodeType.BarrierLine))
			{
				continue;
			}

			Node node = nodeManager.CreateAndModifyNode(eType, item.tag, item.camption, item.gameObject );
			if (node != null )
			{
				node.sceneManager = this;
			}
		}

		// 创建战队
		CreatreLocalBattleTeam();

		// 根据地图配置
		//CreatePVEBattleTeams(table.players);
	}

	
	/// <summary>
	/// 创建障碍物
	/// </summary>
	public void CreateLines(List < List < string > > lines)
	{
		if (lines == null)
			return;

		foreach(List<string> node in lines)
		{
			AddBarrierLines (node[0], node[1]);
		}
	}

	
    /// <summary>
    /// 创建战场内本人的战队
    /// </summary>
    void CreatreLocalBattleTeam( )
    {
		List<Simpleheroconfig>  temp = LocalPlayer.Get().battleTeam;
        foreach (var item in temp)
        {
            Team tm                 = teamManager.GetTeam(TEAM.Team_1);
            if (tm == null)
                continue;

            /// 队伍的人口数
            LocalPlayer.Get().playerData.currentTeam = tm;
            BattleTeam bt1          = new BattleTeam();
            if(bt1 != null )
            {
				bt1.team			= tm;
                bt1.CreateFormation(item, 0, this, nodeManager.GetNodeByType( NodeType.MasterA ) );
                tm.AddBattleTeam(bt1);
            }
        }
    }


	/// <summary>
	/// 创建PVE战队
	/// </summary>
	/// <param name="players"></param>
    private void CreatePVEBattleTeams(List<MapPlayerConfig> players)
    {
		Team tm = teamManager.GetTeam(TEAM.Team_2);
		if (tm == null)
			return;

		BattleSystem.Instance.sceneManager.aiManager.AddAI(tm, AIType.FriendSmart, 1, BattleSystem.Instance.battleData.aiLevel);
		tm.aiEnable = true;
		foreach (var item in players)
		{
			TEAM camption			= (TEAM)item.camption;
			Simpleheroconfig hero	= new Simpleheroconfig();
			int[] HeroPools			= new int[5] { 3001, 3002, 3003, 3004, 3005 };
			{
				hero.heroID			= HeroPools[UnityEngine.Random.Range(0, 5)];
			}
			HeroConfig config		= HeroConfigProvider.Get().GetData(hero.heroID);
			BattleTeam bt1			= new BattleTeam();
			if (bt1 != null)
			{
				bt1.team			= tm;
				Node node			= nodeManager.GetNode( item.tag );
				bt1.CreateFormation(hero, 0, this, node);
				tm.AddBattleTeam(bt1);
			}
		}
    }

	/// <summary>
	/// 创建战场内的建筑
	/// </summary>
	void CreateNode(List < MapBuildingConfig > building)
	{
		if (building == null)
			return;

		foreach (MapBuildingConfig item in building) {
            MapNodeConfig nodecfg = MapNodeConfigProvider.Instance.GetData(item.type, item.size);

            if (nodecfg == null)
                continue;

            /// 3D空间不显示障碍物和障碍线
            NodeType eType = (NodeType)nodecfg.typeEnum;
            if (!battleData.mapEdit && (eType == NodeType.Barrier || eType == NodeType.BarrierLine) )
            {
                continue;
            }

            float dx    = (float)System.Math.Round(item.x, 2);
            float dy    = (float)System.Math.Round(item.y, 2);
            Node node   = nodeManager.AddNode(dx, dy, item, nodecfg, battleData.mapEdit);
            if (node != null)
            {
                node.sceneManager   = this;
                node.InitSkills(nodecfg.skills);
                node.SetNodeAngle(item.fAngle);
			}
		}
	}


	/// <summary>
	/// 获取星球
	/// </summary>
	public Node GetNode(string tag)
	{
		return nodeManager.GetNode (tag);
	}

	/// <summary>
	/// 获取类型
	/// </summary>
	public string GetType(int kind)
	{
        string[] array = { string.Empty, "star", "castle", "teleport", "tower", "barrier", "barrierline", "master", "defense", "power" };
		return array[kind];
	}

	/// <summary>
	/// 删除星球
	/// </summary>
	public void RemoveNode(string tag)
	{
		nodeManager.RemoveNode (tag);
		if (battleData.mapEdit) 
		{
			MapBuildingConfig item = battleData.currentTable.builds.Find (s=> tag.Equals(s.tag));

			battleData.currentTable.builds.Remove (item);
			if(battleData.currentTable.players != null)
			{
				MapPlayerConfig player = battleData.currentTable.players.Find (i=> tag.Equals(i.tag));    //i.id==tag);
				if (player != null)
					battleData.currentTable.players.Remove (player);
			}

			if (battleData.currentTable.lines != null) 
			{
				while (true) 
				{
					List<string> line = battleData.currentTable.lines.Find(s=>s[0] == tag || s[1]==tag);

					if (line == null)
						break;

					battleData.currentTable.lines.Remove (line);

					nodeManager.DelBarrierLines (line[0], line[1]);
				}
			}
		}
	}


    /// <summary>
    /// 添加战队成员到战场节点上
    /// </summary>
    public BattleMember AddMember(Node node, int team, HeroConfig config )
    {
        if (node == null)
            return null;

        return node.AddMember(team, config);
    }


	public BattleMember AddHero( Node node, int team, HeroConfig config )
    {
		if (node == null)
			return null;
		return node.AddHero(team, config);
    }

	/// <summary>
	/// 添加障碍物线
	/// </summary>
	public void AddBarrierLines(string barrierX, string barrierY)
	{
		//添加一条障碍物连线
		nodeManager.AddBarrierLines (barrierX, barrierY);
	}

	/// <summary>
	/// 是否相交
	/// </summary>
	public bool GetIntersection(Vector3 v3X, Vector3 v3Y)
	{
		return nodeManager.IntersectBarrierLien (v3X, v3Y);
	}

    public bool IsFixedPortal(Vector3 v3X, Vector3 v3Y)
    {
        return nodeManager.IsFixedPortal(v3X, v3Y);
    }
	
	
    /// <summary>
    /// 得到战斗加速缩放
    /// </summary>
    public float GetbattleScaleSpeed()
	{
        return battleScaleSpeed;
	}

    /// <summary>
    /// 销毁一个队伍，而不是一个战队
    /// </summary>
	public void DestroyTeam(TEAM team)
	{
		List<Node> allNodes = nodeManager.GetUsefulNodeList();
		for (int i = 0; i < allNodes.Count; ++i) 
		{
			Node node = allNodes [i];
			if (node.team == team) 
			{
				node.BombShip (team, 1.0f);
				node.currentTeam = teamManager.GetTeam (TEAM.Neutral);
				// 音效
				AudioManger.Get ().PlayCapture (node.GetPosition ());
			} 
			else if (node.GetShipCount ((int)team) > 0)
			{
				node.BombShip (team, 1.0f);
			}
		}

		List<BattleMember> ships = shipManager.GetFlyShip ((TEAM)team);
		for (int j = ships.Count - 1; j >= 0;) 
		{
		    ships [j].Bomb ();
		}
	}

	
	/// <summary>
	/// 胜利效果
	/// </summary>
	public void ShowWinEffect(Team win, Color color)
	{
		Node node;
		List<Node> list = nodeManager.GetUsefulNodeList ();
		for (int i = 0; i < list.Count; ++i)
		{
			node = list [i];
            if (node.nodeType != NodeType.Barrier && node.nodeType != NodeType.BarrierLine)
			{
				node.entity.color = color;
			}

			// 销毁掉其他的所有飞船
			for (int j = 0; j < (int)TEAM.TeamMax; ++j)
			{
				TEAM t = (TEAM)j;
				Team team = teamManager.GetTeam (t);

				if (win == team)
					continue;
				if (win.IsFriend (team.groupID))
					continue;

				node.BombShip (t, 1.0f);
			}
		}
	}

	public void SilentMode (bool status)
	{
		battleData.silent = status;
		nodeManager.SilentMode (status);
		shipManager.SilentMode (status);
	}
}
