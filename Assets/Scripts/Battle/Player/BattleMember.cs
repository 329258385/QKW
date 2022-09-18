using UnityEngine;
using System.Collections.Generic;
using Solarmax;
using Nebukam.ORCA;
using Unity.Mathematics;
using Plugin;




public enum MemberEvents
{
    OnAttrChange,
    HPChange,
    MaxHPChange,

    ReleaseSkill,           // 释放技能

    DisplayAction,          // 播放动作
}


/// <summary>
/// 飞船状态
/// </summary>
public enum MemberState
{
    ORBIT,          // 环绕中
    PREJUMP1,       // 飞行预备中
    JUMPING,        // 飞行中
    MoveTo,
    MAX,
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
    /// 集群寻路代理对象
    /// </summary>
    private Agent                   mAgent;


    /// <summary>
    /// 位置
    /// </summary>
    public float3                  position = float3.zero;

    /// <summary>
    /// 初始化
    /// </summary>
    public BattleMember()
	{
        unitType                    = BattleUnitType.bmt_Soldier;
        EventGroup                  = new EventHandlerGroup(typeof(BattleEvent));
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
        return position;
    }

    public void SetPosition(float3 pos)
    {
        this.position       = pos;
        this.EventGroup.fireEvent((int)BattleEvent.SetPos, this, null);
    }


    /// <summary>
    /// 设置目标位置
    /// </summary>
    /// <param name="pos"></param>
    public void SetTargetPosition( Vector3 pos )
    {
        targetPos           = new float3(pos.x, pos.y, pos.z );
    }


    public bool Init ()
	{
		if(sceneManager != null) 
			currentTeam     = sceneManager.teamManager.GetTeam (TEAM.Neutral);
		isALive             = false;
		currentNode         = null;

        
        return true;
	}

    public bool InitMember( HeroConfig config, TEAM team )
    {
        this.config     = config;
        currentTeam     = sceneManager.teamManager.GetTeam(team);
        isALive         = false;
        currentNode     = null;

        aiPublicy       = new BattleMemberAIPublicy(this);
        attribute[(int)ShipAttr.AttackRange]    = config.attackrange;
        attribute[(int)ShipAttr.AttackSpeed]    = config.attackspeed;
        attribute[(int)ShipAttr.WarningRange]   = config.WarningRange;
        attribute[(int)ShipAttr.AttackPower]    = config.damage;
        attribute[(int)ShipAttr.Speed]          = config.speed;
        attribute[(int)ShipAttr.MaxHp]          = config.maxHp;
        attribute[(int)ShipAttr.Armor]          = config.Arms;
        if( this.unitType == BattleUnitType.bmt_Hero )
        {
            attribute[(int)ShipAttr.Population] = 1;
            attribute[(int)ShipAttr.Hp]         = config.maxHp * 100;
        }
        else
        {
            attribute[(int)ShipAttr.Population] = 1;
            attribute[(int)ShipAttr.Hp]         = config.maxHp * 10;
        }

        handler = new RunLockStepLogic[(int)MemberState.MAX];
        handler[(int)MemberState.ORBIT]         = UpdateOrbit;
        handler[(int)MemberState.PREJUMP1]      = UpdatePreJump1;
        handler[(int)MemberState.JUMPING]       = UpdateJumping;
        handler[(int)MemberState.MoveTo]        = UpdateMoveTo;
        shipState                               = MemberState.ORBIT;
        return true;
    }

    public void InitAgent( Vector3 pos )
    {
        position                = new float3(pos.x, pos.y, pos.z);
        //mAgent                  = ORCASimulator.Instance.AddAgent(position);
        //mAgent.m_prefVelocity   = Unity.Mathematics.float3.zero;
        //mAgent.velocity         = Unity.Mathematics.float3.zero;
        //mAgent.radius           = 0.5f;
        //mAgent.height           = 1.0f;
        //mAgent.maxSpeed         = 0.0f;
        //mAgent.navigationEnabled= false;
        //mAgent.layerIgnore      = ORCALayer.L10;

        //if (unitType == BattleUnitType.bmt_Hero)
        //{
        //    mAgent.layerOccupation  = ORCALayer.L10;
        //    mAgent.layerIgnore      = ORCALayer.L20 | ORCALayer.L21 | ORCALayer.L30;
        //}
        //else
        //{
        //    mAgent.layerOccupation  = ORCALayer.L20;
        //    mAgent.layerIgnore      = ORCALayer.L21;
        //}
    }

    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 飞船的心跳逻辑
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
	public void Tick(int frame, float interval)
    {
        MoveUpdate(frame, interval);
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
        EventGroup.fireEvent((int)BattleEvent.ALine, this, null);
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
        EventGroup.fireEvent((int)BattleEvent.Die, this, null);
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


    private TechniqueEntiy  currentSkill;
    private bool            _isUseTechnique = false;
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
    public EventHandlerGroup            EventGroup;

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
            EventGroup.fireEvent((int)MemberEvents.ReleaseSkill, this, null);
        }
    }
}
