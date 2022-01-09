using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;






public partial class BattleMember : Lifecycle2
{

    public enum BattleUnitType
    {
        Member,
        Hero,
    }


    /// <summary>
    /// 场景管理
    /// </summary>
    public SceneManager         sceneManager { get; set; }

    /// <summary>
	/// 所在建筑
	/// </summary>
	public Node                 currentNode { get; set; }

    /// <summary>
    /// 当前位置格子
    /// </summary>
    public HexmapNode           currentHexNode { get; set; }


    /// <summary>
    /// 当前队伍
    /// </summary>
    public TEAM                 team { get { return currentTeam == null ? TEAM.Neutral : currentTeam.team; } }

    /// <summary>
    /// 飞船所属的战队
    /// </summary>
    public BattleTeam           battleTeam = null;

    /// <summary>
    /// 英雄静态配置数据
    /// </summary>
    public HeroConfig           config;

    /// <summary>
    /// 战斗单元类型
    /// </summary>
    public BattleUnitType       unitType = BattleUnitType.Member;


    /// <summary>
    /// 当前队伍信息
    /// </summary>
    public Team                 currentTeam
    {
        get { return realTeam; }
        set
        {
            if (realTeam == value)
                return;

            realTeam = value;
        }
    }


    /// <summary>
    /// 队伍信息
    /// </summary>
    /// <value>The real team.</value>
    private Team                realTeam { get; set; }

    /// <summary>
    /// 对象池
    /// </summary>
    ShipManager                 pool { get; set; }

	/// <summary>
	/// 是否在被使用
	/// </summary>
    public bool                 isALive { get; set; }

    /// <summary>
    /// 是否需要更新逻辑
    /// </summary>
    public bool                 isNeedUpdate = false;

	/// <summary>
	/// 飞船实体
	/// </summary>
	public EntityMember         entity { get; set; }


    /// <summary>
    /// 飞船属性
    /// </summary>
    protected int[]                 attribute = new int[(int)ShipAttr.MAX];


    private List<BufferEntiy>       bufs = new List<BufferEntiy>();


    private BattleMemberAIPublicy   aiPublicy;
    /// <summary>
    /// 初始化
    /// </summary>
    public BattleMember()
	{
        unitType = BattleUnitType.Member;
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


    /// <summary>
    /// 获取坐标
    /// </summary>
    /// <returns>The position.</returns>
    public Vector3 GetPosition()
    {
        if (entity == null)
            return Vector3.zero;
        return entity.GetPosition();
    }

    public void SetPosition(Vector3 pos)
    {
        if (entity == null)
            return;

        entity.SetPosition(pos);
    }

    /// <summary>
    /// 设置目标位置
    /// </summary>
    /// <param name="pos"></param>
    public void SetTargetPosition( Vector3 pos )
    {
        if (entity != null)
        {
            entity.TargetPos = pos;
        }
    }


    public bool Init ()
	{
		if(sceneManager != null) 
			currentTeam = sceneManager.teamManager.GetTeam (TEAM.Neutral);
		isALive         = false;
		currentNode     = null;

        return true;
	}

    public bool InitMember( HeroConfig config, TEAM team )
    {
        this.config     = config;
        currentTeam     = sceneManager.teamManager.GetTeam(team);
        isALive         = false;
        currentNode     = null;

        aiPublicy       = new BattleMemberAIPublicy();
        aiPublicy.Init(this, BattleMemberAIPublicy.Publicy.active);
        attribute[(int)ShipAttr.AttackRange]    = config.attackrange;
        attribute[(int)ShipAttr.AttackSpeed]    = config.attackspeed;
        attribute[(int)ShipAttr.WarningRange]   = config.WarningRange;
        attribute[(int)ShipAttr.AttackPower]    = config.damage;
        attribute[(int)ShipAttr.Speed]          = config.speed;
        attribute[(int)ShipAttr.MaxHp]          = config.maxHp;
        attribute[(int)ShipAttr.Armor]          = config.Arms;
        attribute[(int)ShipAttr.Population]     = 100;
        return true;
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 飞船的心跳逻辑
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
	public void Tick(int frame, float interval)
    {
        if (entity == null)
        {
            return;
        }

        if (!isNeedUpdate)
        {
            return;
        }

        if (unitType == BattleUnitType.Member && battleTeam.btState != BattleTeamState.Battle )
        {
            bool IsNeedMove = entity.UpdateMove(frame, interval);
            if( !IsNeedMove )
            {
                entity.StartAttackAction();
            }
        }

        if (unitType == BattleUnitType.Hero && battleTeam.btState == BattleTeamState.Defensive)
        {
            bool IsNeedMove = entity.UpdateMove(frame, interval);
            if (!IsNeedMove)
            {
                entity.StartAttackAction();
            }
        }

        entity.Tick(frame, interval);

        //HexmapNode hexnode = HexMap.Instance.Vector3ToNode( GetPosition() );
        //if( hexnode != null )
        //{
        //    if( currentHexNode != null && hexnode != currentHexNode )
        //    {
        //        HexMap.Instance.ModifyNodeCost(currentHexNode, -1f);
        //        currentHexNode = hexnode;
        //        HexMap.Instance.ModifyNodeCost(currentHexNode,  1f);
        //    }
        //}

        if (battleTeam.btState == BattleTeamState.Battle )
        {
            aiPublicy.AotuBattle(frame, interval);
        }
    }

	public void Destroy()
	{
		if(sceneManager != null) 
			currentTeam = sceneManager.teamManager.GetTeam (TEAM.Neutral);
		isALive     = false;
		currentNode = null;

		if (entity != null) {
			entity.Destroy ();
		}
		entity = null;
	}


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 回收逻辑
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    public void OnRecycle()
	{
		//从当前星球中移除自己
		if (currentNode != null) {
			currentNode.RemoveShip (this, false );
		}

		//从飞行列表中移除
		pool.RemoveFlyShip (this);
		entity.OnRecycle ();
	}


    public void OnALive( )
    {
        if (entity != null)
        {
            entity.OnALive();
        }
    }

    /// <summary>
    /// 初始化飞船
    /// </summary>
    public void InitShip(ShipManager sm, bool noAnim = false, bool noOrbit = true )
	{
		pool         = sm;
		sceneManager = sm.sceneManager;
		
		//创造飞船实体
		if(entity == null)
		{
            entity = new EntityMember("ship", sceneManager.battleData.silent);
            if (entity != null)
            {
                entity.perfab = config.perfab;
                entity.ship = this;
                entity.Init();
            }
		}
		else
		{
			entity.SilentMode (sceneManager.battleData.silent);
		}

        //激活
        isALive         = true;
	}


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 飞船被摧毁逻辑
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
	public void Bomb( NodeType eType = NodeType.None )
	{
        SetAtt(ShipAttr.Hp, 0);
        SetAtt(ShipAttr.Population, 0);
		pool.RemoveFlyShip (this);
        pool.Recycle(this);
        isALive = false;
	}


	/// <summary>
	/// 移动到指定星球
	/// </summary>
	public void MoveTo(Node node, bool warp)
	{
		//添加到飞行列表, 瞬移暂时不加入飞行队列
        if ( !warp )
		    pool.AddFlyShip(this);

        entity.MoveTo(node, warp);
	}


    public void MoveToFly( Node node )
    {
        entity.targetNode = node;
        pool.AddFlyShip(this);
    }

    /// --------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 进入某个星球
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------------
    public void EnterNode(Node node)
	{
		//移除飞行列表
		pool.RemoveFlyShip (this);
		//加入星球
        battleTeam.EnterNode(node, true );
	}


    /// --------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 飞船离开星球
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------------
    public void LeaveNode()
    {
        if (currentNode != null )
        {
            currentNode.RemoveShip(this);
        }
    }

    /// <summary>
    /// 设置属性值，absolute为绝对值10000 分比
    /// </summary>
    public void SetAtt(ShipAttr attr, int value)
    {
        attribute[(int)attr] = value;
    }

    /// <summary>
    /// 获取属性
    /// </summary>
    public int GetAtt(ShipAttr attr)
    {
        return attribute[(int)attr];
    }


    /// <summary>
    /// 改变属性
    /// </summary>
    public int ChangeAttr( ShipAttr attr, int value )
    {
        attribute[(int)attr] = attribute[(int)attr] + value;
        return attribute[(int)attr];
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 给飞船增加buffer
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    public void AddBuffer( BufferEntiy buf )
    {
        if (buf == null)
            return;

        BufferEntiy owerBuff = FindBuffer(buf.config.buffId);
        if( owerBuff != null )
        {
            if( DelBuffer( owerBuff ) )
            {
                owerBuff.UnEffect();
            }
        }

        bufs.Add(buf);
        buf.ApplyShipBuffer(this);
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 删除飞船的buffer
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    public bool DelBuffer( BufferEntiy buf )
    {
        if (buf == null)
            return false;

        return bufs.Remove(buf);
    }


    // --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 查找buffer
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    public BufferEntiy FindBuffer( int nBuffID )
    {
        for( int i = 0; i < bufs.Count; i++ )
        {
            if (bufs[i].config.buffId == nBuffID)
                return bufs[i];
        }
        return null;
    }

    // --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 优先攻击最近的英雄
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    public void PriorityEncircleCity( )
    {
        entity.PriorityEncircleCity();
    }
}
