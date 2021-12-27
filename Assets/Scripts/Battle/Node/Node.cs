using UnityEngine;
using System.Collections.Generic;
using Solarmax;





/// <summary>
/// 节点类型
/// </summary>
public enum NodeType
{
	None,
	Planet,		    //1 星球
	Castle,		    //2 堡垒
	WarpDoor,	    //3 星门
	Tower,		    //4 防御塔
	Barrier,	    //5 障碍物
	BarrierLine,    //6 障碍物线
	MasterA,		//7 主基地
	MasterB,		//8 主基地
	Power,		    //9 能量塔
}


/// <summary>
/// 节点状态
/// </summary>
public enum NodeState
{
	Idle,		    //空闲状态
	Battle,		    //战争
	Capturing,	    //攻击
	Occupied,	    //占领
}

/// <summary>
/// 建筑父节点
/// </summary>
public abstract partial class Node : Lifecycle3
{

    /// <summary>
    /// 星球的节点类型
    /// </summary>
    public NodeType         nodeType = NodeType.None;


    /// <summary>
    /// 星球的渲染度数
    /// </summary>
    public float            nodeAngle = 0f;

	/// <summary>
	/// 节点标记
	/// </summary>
	/// <value>The tag.</value>
	public string tag { 
		get { return nodeTag; } 
		set { 
			nodeTag = value;
		}
	}

    /// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// 星球管理
	/// </summary>
    /// ----------------------------------------------------------------------------------------------------
	public NodeManager nodeManager { get; set; }

	/// <summary>
	/// 标记
	/// </summary>
	string              nodeTag;

	
	/// <summary>
	/// 当前所属队伍
	/// </summary>
	/// <value>The team.</value>
	public TEAM team 
	{ 
		get 
		{ 
			if (currentTeam == null)
				return TEAM.Neutral;
			return currentTeam.team;
		}
	}

	/// <summary>
	/// 当前队伍
	/// </summary>
	/// <value>The current team.</value>
	public Team currentTeam 
	{ 
		get { return realTeam; } 
		set { 
			SetRealTeam (value);
		}
	}

	/// <summary>
	/// 设置星球归属方
	/// Note：
	/// </summary>
	public void SetRealTeam( Team value, bool initNode = false, BattleTeam bt = null )
	{
		if (realTeam == value)
			return;


        Team selfTeam = nodeManager.sceneManager.teamManager.GetTeam(nodeManager.sceneManager.battleData.currentTeam);
        if( realTeam != null )
        {
            if( selfTeam == realTeam && bt != null )
            {
                EventSystem.Instance.FireEvent(EventId.OnPopulationDown, population);
            }
        }

        realTeam = value;
        if (realTeam != null)
        {
            if (selfTeam == realTeam && bt != null )
            {
                EventSystem.Instance.FireEvent(EventId.OnPopulationUp, population);
            }
        }
	}


	/// <summary>
	/// 修改当前基础人口上限
	/// </summary>
	/// <param name="Population">Population.</param>
	public void SetBasePopulation(int Pop)
	{
		
	}

	/// <summary>
	/// team信息
	/// </summary>
	Team            realTeam { get; set; }

	/// <summary>
	/// 星球状态
	/// </summary>
	NodeState       nodeState;

	/// <summary>
	/// 当前节点状态
	/// </summary>
	/// <value>The state.</value>
	public NodeState state { 
		get { return nodeState; } 
		set {
			if (value == nodeState)
				return;
			nodeState = value;
		}
	}

	/// <summary>
	/// 血量
	/// </summary>
	/// <value>The hp.</value>
	public float hp {
		get;
		set;
	}

	/// <summary>
	/// 当前血量上限
	/// </summary>
	/// <value>The hp max.</value>
	public float hpMax 
	{
        get { return GetAttribute(NodeAttr.HpMax); }
	}

	/// <summary>
	/// 攻击范围
	/// </summary>
	/// <value>The attack rage.</value>
	public float            AttackRage 
	{
        get { return GetAttribute(NodeAttr.AttackRange); }
	}

	/// <summary>
	/// 攻击速度
	/// </summary>
	/// <value>The attack speed.</value>
    public float			AttackSpeed { get { return GetAttribute(NodeAttr.AttackSpeed); } }

	/// <summary>
	/// 攻击力
	/// </summary>
	/// <value>The attack power.</value>
    public float			AttackPower { get { return GetAttribute(NodeAttr.AttackPower); } }

	/// <summary>
	/// 人口上限
	/// </summary>
	/// <value>The population.</value>
    public int				population { get { return (int)GetAttribute(NodeAttr.Poplation); } }


	/// <summary>
	/// 节点属性
	/// </summary>
	private float[]         attribute = new float[(int)NodeAttr.MAX];

	
    /// <summary>
    /// 星球上战队
    /// </summary>
    public List<BattleTeam> battArray = new List<BattleTeam>();


    /// <summary>
    /// 飞船数量
    /// </summary>
    public int[]            numArray = new int[(int)TEAM.TeamMax];


    public bool             nodeIsHide = false;


	/// <summary>
	/// 初始化
	/// </summary>
	public Node(string name)
	{
		tag             = name;
        nodeIsHide      = false;
	}

	public virtual bool Init ( GameObject go = null )
	{
		InitNode(go);
		InitNodeTouch();
		for (int i = 0; i < attribute.Length; ++i) 
        {
			attribute [i] = 0f;
		}
		// 设置默认属性
        SetAttribute(NodeAttr.OccupiedSpeed, 1);
		return true;
	}

	void InitNodeTouch()
	{
		switch (nodeType)
		{
			case NodeType.None: break;
			case NodeType.Barrier: break;
			case NodeType.BarrierLine: break;
			default:
				{
					// 挂载触摸脚本
					TouchHandler touch = entity.go.GetComponentInChildren<TouchHandler>();
					if (null != touch)
					{
						touch.SetNode(this);
					}
				}
				break;
		}
	}

	public virtual void Tick (int frame, float interval)
	{
		if (entity != null) {
			entity.Tick (frame, interval);
		}

		UpdateAiTimers(frame, interval);
		UpdateHideStatus();
		UpdateRevolution (frame, interval);
    }

	public virtual void Destroy ()
	{
		if (entity != null) 
		{
			entity.Destroy ();
		}

        state       = NodeState.Idle;
		for (int i = 0; i < numArray.Length; i++) 
        {
			numArray[i] = 0;
		}
	}

    
    /// ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// 添加飞船
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------
    public BattleMember AddMember(int team, HeroConfig config )
    {
		BattleMember ship		 = null;
        Team teamData	 = nodeManager.sceneManager.teamManager.GetTeam((TEAM)team);

        //获取一个ship对象
        ship             = nodeManager.sceneManager.shipManager.Alloc();
		ship.unitType	 = BattleMember.BattleUnitType.Member;
		ship.sceneManager = nodeManager.sceneManager;
		ship.currentNode = this;
		ship.InitMember(config, (TEAM)team);

		//设置飞船在哪个星球上
		AddMember(ship);
		ship.InitShip(nodeManager.sceneManager.shipManager, false);
        ship.SetColor(teamData.color);
		ship.SetPosition(GetPosition());
		ship.SetTargetPosition(GetPosition());
		return ship;
    }

	public BattleMember AddHero( int team, HeroConfig config )
    {
		BattleMember hero = nodeManager.sceneManager.shipManager.Alloc(); ;
		Team teamData	= nodeManager.sceneManager.teamManager.GetTeam((TEAM)team);
		if ( hero != null )
        {
			hero.unitType	  = BattleMember.BattleUnitType.Hero;
			hero.sceneManager = nodeManager.sceneManager;
			hero.currentNode  = this;
			hero.InitMember(config, (TEAM)team);

			AddMember(hero);
			hero.InitShip(nodeManager.sceneManager.shipManager, false);
			hero.SetColor(teamData.color);
			hero.SetPosition(GetPosition());
			hero.SetTargetPosition(GetPosition());
		}
		return hero;
    }

	/// <summary>
	/// 叠加数量
	/// </summary>
	public void AddShipNum( int add, BattleTeam bt = null )
	{
        int nBT     = bt.ID;
       
        if (bt != null)
        {
			//获取一个ship对象
			BattleMember ship           = bt.members[0];
            int nMaxPopulation  = ship.GetAtt( ShipAttr.Population );
            for( int i = 0; i < bt.members.Count; i++ )
            {
				BattleMember temp = bt.members[i];
                if (ship == temp)
                    continue;

                if(temp != null && nMaxPopulation > temp.GetAtt( ShipAttr.Population ) )
                {
                    ship = temp;
                    nMaxPopulation = temp.GetAtt( ShipAttr.Population );
                }
            }

            if( ship != null )
            {
                int nHP = ship.GetAtt(ShipAttr.Hp);
                if( nHP <= 0 )
                {
                    ship.OnALive();
                }


                nHP     += add * ship.GetAtt(ShipAttr.BaseHp);
                ship.ChangeAttr(ShipAttr.Population, add );
                ship.ChangeAttr(ShipAttr.Hp, add * ship.GetAtt(ShipAttr.BaseHp) );
            }
        }
    }


	/// <summary>
	/// 添加飞船, 这时候 ship 的战队和队伍归属必须有效
	/// </summary>
	public void AddMember(BattleMember ship)
	{
		ship.currentNode	= this;
		ship.sceneManager	= nodeManager.sceneManager;
		//增加飞船数量
		numArray[(int)ship.team] += ship.GetAtt(ShipAttr.Population);
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="hero"></param>
	//public void AddHero( HeroMember hero )
 //   {
	//	hero.currentNode	= this;
	//	hero.sceneManager = nodeManager.sceneManager;

	//	//增加飞船数量
	//	numArray[(int)hero.team] += hero.GetAtt(ShipAttr.Population);
	//}


	/// <summary>
	/// 移除飞船
	/// </summary>
	public void RemoveShip(BattleMember ship, bool bDelFromNode = true )
	{
        //增加飞船数量
        if( bDelFromNode )
        {
            ship.currentNode    = null;
        }
        numArray[(int)ship.team] -= ship.GetAtt(ShipAttr.Population);
	}


	/// <summary>
	/// 获取某一阵营的飞船
	/// </summary>
	public List<BattleMember> GetShips(int team)
	{
        List<BattleMember> list     = new List<BattleMember>();
        int nBattleTeamNum  = battArray.Count;
        for (int i = 0; i < nBattleTeamNum; i++)
        {
            if (battArray[i] == null)
                continue;
        }
        return list;
	}


	/// <summary>
	/// 获取飞船数量
	/// </summary>
	public int GetShipCount(int team)
	{
        return numArray[team];
	}

    
	/// <summary>
	/// 销毁飞船，返回销毁的真实数
	/// </summary>
	public int BombShip(TEAM t, float rate)
	{
        int count   = GetShipCount((int)t);
		int destroy = Mathf.FloorToInt(count * rate);
        return BombShipNum (t, destroy);
	}

	public int BombShipNum(TEAM t, int destroy)
	{
		int real = 0;
		if (destroy > 0) 
		{
            int nBattleTeamNum = battArray.Count;
            for (int i = 0; i < nBattleTeamNum; i++)
            {
                if (battArray[i] == null)
                    continue;

                if (battArray[i].team.team != t)
                    continue;

                List<BattleMember> list = battArray[i].members;
                if (list == null)
                    continue;

                for (int n = 0; n < list.Count; )
                {
                    numArray[(int)t]--;
                    destroy--;
                    ++real;

                    list[n].Bomb();
                    nodeManager.sceneManager.teamManager.AddDestory(team);
                    if (destroy <= 0 || list.Count == 0)
                        break;
                }
            }
		}
        return real;
	}


	
    /// 战队在星球上移动
    /// </summary>
    public void MoveEffect(Node to, int nBT = 0)
    {
        // warping音效
        if (sceneManager.teamManager.GetTeam(team).IsFriend(this.currentTeam.groupID)
            && this.nodeType == NodeType.WarpDoor)
        {

            EffectManager.Get().AddWarpPulse(this, to, team);
            AudioManger.Get().PlayWarpCharge(this.GetPosition());

            sceneManager.warpManager.AddWarpItem(this, to, team, true);
            return;
        }

        else if ( sceneManager.teamManager.GetTeam(team).IsFriend(this.currentTeam.groupID))
        {
            // 判断是否可以通过定向传送
            if (sceneManager.IsFixedPortal(this.GetPosition(), to.GetPosition()))
            {
                EffectManager.Get().AddWarpPulse(this, to, team);
                AudioManger.Get().PlayWarpCharge(this.GetPosition());

                sceneManager.warpManager.AddWarpItem(this, to, team, true);
                return;
            }
        }
        MoveTo(to, false , nBT );
    }


    public void MoveTo(Node node,bool warp = false, int nBT = 0)
	{

        BattleTeam bt = null;
        for (int i = 0; i < battArray.Count; i++ )
        {
            if (battArray[i].ID == nBT)
            {
                bt = battArray[i];
                break;
            }
        }

        if (bt == null)
            return;

        List<BattleMember> all = bt.members;
		if (all == null )
			return;

		bool bWarp    = CanWarp (team);
		if (warp)
			bWarp     = warp;

		if (bWarp) 
		{
			EffectManager.Get ().AddWarpArrive (node, currentTeam.color);
		}

		bt.OnBattleMove( this, node, bWarp );
        bt.LeaveNode(this);
    }

	/// <summary>
	/// 是否瞬移
	/// </summary>
	public bool CanWarp()
	{
		return (this.team != TEAM.Neutral && nodeType == NodeType.WarpDoor
			&& (this.team == nodeManager.sceneManager.battleData.currentTeam ||
				nodeManager.sceneManager.teamManager.GetTeam (nodeManager.sceneManager.battleData.currentTeam).IsFriend (this.currentTeam.groupID)));
	}

	private bool CanWarp(TEAM team)
	{
		return ((this.team == team || nodeManager.sceneManager.teamManager.GetTeam(team).IsFriend(currentTeam.groupID))&& nodeType == NodeType.WarpDoor);
	}

	/// <summary>
	/// 判断是否己方和队友拥有这个星球
	/// </summary>
	public virtual bool IsOurNode(TEAM team)
	{
		Team thisteam = nodeManager.sceneManager.teamManager.GetTeam(this.team);
		Team t = nodeManager.sceneManager.teamManager.GetTeam (team);

		if (this.team == team)
			return true;
		if (thisteam.IsFriend (t.groupID))
			return true;

		return false;
	}


	private void CreateHUD()
    {
		UnityEngine.Object res = AssetManager.Get().GetResources("UISlgCityOperater");
		if (res != null)
		{
			GameObject nodego	= GetGO();
			GameObject go		= GameObject.Instantiate(res) as GameObject;
			mCityHUD			= go.GetComponent<HUDCityOperater>();
			mCityHUD.transform.parent = UISystem.Get().mUIParent;
			mCityHUD.transform.localScale = Vector3.one;
		}
	}

	/// <summary>
	/// 设置属性值，absolute为绝对值
	/// </summary>
	public void SetAttribute (NodeAttr attr, float num )
	{
		attribute [(int)attr] = num;
	}

	/// <summary>
	/// 获取属性
	/// </summary>

	public float GetAttribute(NodeAttr attr)
	{
		return attribute [(int)attr];
	}


    public void SetNodeSize(float fSize)
    {
        this.fRadius = fSize;
    }

    public void SetNodeAngle( float fAngle )
    {
        nodeAngle = fAngle;
        SetRotation(new Vector3(0f, 0f, nodeAngle));
    }

    /// <summary>
	/// 设置颜色
	/// </summary>
	/// <param name="color">Color.</param>
	public void SetColor(Color color)
    {
        if (entity == null)
            return;

        if (BattleSystem.Instance.battleData.mapEdit)
        {
            entity.SetColor(color);
        }
    }
}
