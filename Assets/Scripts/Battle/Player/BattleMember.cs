using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;
using TimeLines;



public enum BattleMemberStatus
{
    status_Unknown      = 0,        // 未知
    status_Move,                    // 移动
    status_Attack,                  // 攻击
    status_Dead,                    // 死亡
    status_Escape,                  // 溃逃
}


public enum MemberEvents
{
    OnAttrChange,
    HPChange,
    MaxHPChange,

    ReleaseSkill,           // 释放技能

    DisplayAction,          // 播放动作
}

public partial class BattleMember : Lifecycle2
{

    public enum BattleUnitType
    {
        bmt_Unknown = 0,        // 未知
        bmt_Soldier,                    // 士兵
        bmt_Hero,                       // 英雄
        bmt_Commander,                  // 主帅
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
    public BattleUnitType       unitType = BattleUnitType.bmt_Soldier;


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
    private Team                    realTeam { get; set; }

    /// <summary>
    /// 对象池
    /// </summary>
    ShipManager                     pool { get; set; }

	/// <summary>
	/// 是否在被使用
	/// </summary>
    public bool                     isALive { get; set; }

    /// <summary>
    /// 是否需要更新逻辑
    /// </summary>
    public bool                     isNeedUpdate = false;

	/// <summary>
	/// 飞船实体
	/// </summary>
	public EntityMember             entity { get; set; }


    /// <summary>
    /// 飞船属性
    /// </summary>
    protected int[]                 attribute = new int[(int)ShipAttr.MAX];


    private List<BufferEntiy>       bufs = new List<BufferEntiy>();


    public BattleMemberAIPublicy    aiPublicy;

    /// <summary>
    /// 延迟逻辑处理单元
    /// </summary>
    public DelayActionFrame         ActonTicks;

    /// <summary>
    /// 初始化
    /// </summary>
    public BattleMember()
	{
        unitType                    = BattleUnitType.bmt_Soldier;
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

        ActonTicks      = new DelayActionFrame();
        aiPublicy       = new BattleMemberAIPublicy();
        aiPublicy.Init(this, BattleMemberAIPublicy.Publicy.active);
        attribute[(int)ShipAttr.AttackRange]    = config.attackrange;
        attribute[(int)ShipAttr.AttackSpeed]    = config.attackspeed;
        attribute[(int)ShipAttr.WarningRange]   = config.WarningRange;
        attribute[(int)ShipAttr.AttackPower]    = config.damage;
        attribute[(int)ShipAttr.Speed]          = config.speed;
        attribute[(int)ShipAttr.MaxHp]          = config.maxHp;
        attribute[(int)ShipAttr.Armor]          = config.Arms;
        if( this.unitType == BattleUnitType.bmt_Hero )
        {
            attribute[(int)ShipAttr.Population] = 100;
            attribute[(int)ShipAttr.Hp]         = config.maxHp * 100;
        }
        else
        {
            attribute[(int)ShipAttr.Population] = 10;
            attribute[(int)ShipAttr.Hp]         = config.maxHp * 10;
        }
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

        /// 延迟逻辑
        ActonTicks.Tick( interval );

        if (!isNeedUpdate)
        {
            return;
        }

        if (unitType == BattleUnitType.bmt_Soldier && battleTeam.btState != BattleTeamState.Battle )
        {
            bool IsNeedMove = entity.UpdateMove(frame, interval);
            if (!IsNeedMove)
            {
                entity.StartAttackAction();
            }
        }

        if (unitType == BattleUnitType.bmt_Hero && battleTeam.btState == BattleTeamState.Defensive)
        {
            bool IsNeedMove = entity.UpdateMove(frame, interval);
            if (!IsNeedMove)
            {
                entity.StartAttackAction();
            }
        }

        entity.Tick(frame, interval);

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
        entity.OnDeath();
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

    private TechniqueEntiy currentSkill;
    private bool    _isUseTechnique = false;
    private Vector3 _vForwardDir = Vector3.zero;
    // --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 转向
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    public void RotateToTarget()
    {
        if(_isUseTechnique && currentSkill != null && currentSkill.IsApplyTechnique() )
        {

        }
    }

    /// <summary>
    /// 事件系统
    /// </summary>
    public EventHandlerGroup            EventGroup { get; set; }

    // --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 属性变化
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    protected void OnAttrChange( ShipAttr attrid, int oldVal, int newValue )
    {
        EventGroup.fireEvent((int)MemberEvents.OnAttrChange, this, new EventArgs_ThreeVal<ShipAttr, int, int>(attrid, oldVal, newValue ));
        switch( attrid )
        {
            case ShipAttr.Hp:
                {
                    bool oldDead = false;
                    bool newDead = newValue <= 0;
                    if (oldVal != 0)
                    {
                        EventGroup.fireEvent((int)MemberEvents.HPChange, this, new EventArgs_SinVal<bool>(newValue < oldVal));
                    }
                    if (oldDead == false && newDead)
                    {
                        ;//Die();
                    }
                    break;
                }
                break;

            case ShipAttr.MaxHp:
                {
                    if (oldVal == GetAtt(ShipAttr.Hp) || newValue < GetAtt(ShipAttr.Hp))
                    {
                        SetAtt(ShipAttr.Hp, newValue);
                    }
                }
                break;
            default:
                break;
        }
    }


    // --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 技能是否能使用
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    public bool CanUseTechnique( TechniqueEntiy pTechnique, BattleMember target )
    {
        if ( pTechnique == null) return false;
        if (_isUseTechnique ) return false;

        return pTechnique.IsApplyTechnique();
    }


    // --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 使用技能
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    public void UseTechnique(TechniqueEntiy pTechnique,  BattleMember target )
    {
        if( pTechnique != null )
        {
            pTechnique.StardCD();

            _isUseTechnique         = true;
            currentSkill            = pTechnique;
            currentSkill.ApplyTechnique();

            /// 通知使用技能事件
            if( pTechnique.proto.scope > 0 )
            {
                ActionArgs args = new ActionArgs()
                {
                    SkillID         = -1,
                    Source          = this,
                    Target          = target,
                    TargetPos       = Vector3.zero,
                    TimeScale       = 1.0f,
                    OnActionFinishd = currentSkill.OnActionFinish,
                    OnLaunchFinishd = currentSkill.OnLaunchFinish,
                };

                EventGroup.fireEvent((int)MemberEvents.DisplayAction, this, new EventArgs_DouVal<string, ActionArgs>("", args));
            }
            EventGroup.fireEvent((int)MemberEvents.ReleaseSkill, this, null);
        }
    }
}
