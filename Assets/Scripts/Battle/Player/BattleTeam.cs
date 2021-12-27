using UnityEngine;
using System.Collections.Generic;
using Solarmax;
using Plugin;

/// <summary>
/// 飞船状态
/// </summary>
public enum BattleTeamState
{
    Idle,                   // 初始态
    Move,
    Battle,
    Defensive,              // 防御
    Encirclecity,           // 围城
    EnterCity,              // 入城
    Max,
}

public enum Formation
{
    FormationNull,
    FormationMove,          // 移动队形
    FormationAttack,        // 攻击队形
    FormationDefensive,     // 防御队形
    FormationSurround,      // 包围队形
}

/// <summary>
/// 战斗单元对象
/// </summary>
public class BattleTeam
{
    private static int          GenIDPool = 1;
    public int                  ID;

    /// <summary>
    /// 战队所属的队伍 
    /// </summary>
    public Team                 team;

    /// <summary>
    /// 战队内的飞船数量
    /// </summary>
    public List<BattleMember>   members = new List<BattleMember>();


    /// <summary>
    /// 战队所在的星球
    /// </summary>
    public Node                 curentNode = null;

    /// <summary>
    /// 状态
    /// </summary>
    public BattleTeamState      btState = BattleTeamState.Idle;

    /// <summary>
    /// 战队队形
    /// </summary>
    public Formation            btFormation = Formation.FormationNull;

    /// <summary>
    /// 函数集
    /// </summary>
    RunLockStepLogic[]          handler { get; set; }

    RunLockStepLogic[]          formationHandler { get; set; }

    public BattleTeam( )
    {
        ID                                                  = GenIDPool++;
        handler                                             = new RunLockStepLogic[(int)BattleTeamState.Max];
        formationHandler                                    = new RunLockStepLogic[5];
        handler[(int)BattleTeamState.Idle]                  = UpdateOrbit;
        handler[(int)BattleTeamState.Move]                  = UpdateMove;
        handler[(int)BattleTeamState.Battle]                = UpdateOrbit;
        handler[(int)BattleTeamState.Defensive]             = UpdateMove;
        handler[(int)BattleTeamState.Encirclecity]          = UpdateMove;
        handler[(int)BattleTeamState.EnterCity]             = UpdateEnterCity;


        formationHandler[(int)Formation.FormationNull ]     = UpdateOrbit;
        formationHandler[(int)Formation.FormationMove]      = UpdateMoveFormation;
        formationHandler[(int)Formation.FormationAttack]    = UpdateAttackFormation;
        formationHandler[(int)Formation.FormationDefensive] = UpdateDefensiveFormation;
        formationHandler[(int)Formation.FormationSurround]  = UpdateSurroundFormation;
    }


    /// ---------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 空闲
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------------
    void UpdateOrbit(int frame, float dt)
    {

    }


    /// --------------------------------------------------------------------------------------
    /// <summary>
    /// 移动
    /// </summary>
    /// --------------------------------------------------------------------------------------
    private void UpdateMove(int frame, float interval)
    {
        formationHandler[(int)btFormation](frame, interval);
    }


    public bool IsMoving()
    {
        return btFormation == Formation.FormationAttack || btFormation == Formation.FormationDefensive || btFormation == Formation.FormationMove || btFormation == Formation.FormationSurround;
    }

    /// <summary>
    /// 当前飞船数量
    /// </summary>
    public int current
    {
        get
        {
            int nSum = 0;
            for (int i = 0; i < members.Count; i++)
            {
                nSum += members[i].GetAtt( ShipAttr.Population );
            }
            return nSum;
        }
    }

    public int GetAttribute(TeamAttr e)
    {
        return 1;
    }


    public int Population()
    {
        int nPopulation = Leader.GetAtt(ShipAttr.Population);
        for (int i = 0; i < members.Count; i++)
        {
            nPopulation += members[i].GetAtt(ShipAttr.Population);
        }
        return nPopulation;
    }


    /// <summary>
    /// 飞船上限
    /// </summary>
    public int currentMax
    {
        get
        {
            return 600;
        }
    }


    /// <summary>
    /// 战队主舰
    /// </summary>
    public BattleMember             Leader = null;

    /// <summary>
    /// 是否在进城中
    /// </summary>
    /// <returns></returns>
    public bool IsHomeCity()
    {
        if (btState == BattleTeamState.EnterCity || btState == BattleTeamState.Battle )
            return true;
        return false;
    }


    /// <summary>
    /// 创捷飞行编队
    /// </summary>
    public void CreateFormation( Simpleheroconfig hero, int order, SceneManager sceneManager, Node master )
    {
        btState     = BattleTeamState.Idle;
        HeroConfig config       = HeroConfigProvider.Get().GetData(hero.heroID);
        Leader                  = sceneManager.AddHero(master, (int)team.team, config );
        if( hero != null )
        {
            /// 初始化战队成员
            Leader.battleTeam   = this;
            GameObject go       = Leader.entity.go;
            if (config != null && config.ArmsType > 0 )
            {
                HeroConfig memberCfg        = HeroConfigProvider.Get().GetData(config.ArmsType);
                for (int i = 0; i < 6; i++)
                {
                    BattleMember member = sceneManager.AddMember(master, (int)team.team, memberCfg);
                    member.battleTeam   = this;
                    members.Add(member);
                }
            }
        }

        if( hero != null )
        {
            EnterNode( master );
        }
    }


    public void Tick (int frame, float interval)
    {
        handler[(int)btState](frame, interval);
    }


    /// --------------------------------------------------------------------------------------
    /// <summary>
    /// 编队移动
    /// </summary>
    /// --------------------------------------------------------------------------------------
    private void UpdateMoveFormation( int frame, float interval )
    {
        Transform tf = Leader.entity.go.transform;
        for (int i = 0; i < members.Count; i++)
        {
            Vector3 newPos = tf.TransformPoint(MyBattleOrder.MarchFormation[i] * MyBattleOrder.MAX_FORMATION_SPACE );
            members[i].SetTargetPosition(newPos);
        }
    }

    /// --------------------------------------------------------------------------------------
    /// <summary>
    /// 编队保持攻击队形
    /// </summary>
    /// --------------------------------------------------------------------------------------
    private void UpdateAttackFormation(int frame, float interval)
    {
        Vector3[] formation = MyBattleOrder.BattleFormation;
        Transform tf        = Leader.entity.go.transform;
        int index           = 0;
        int count           = members.Count;
        int n               = 0;
        for ( int x = 0; x < MyBattleOrder.MAX_FORMATION_ROW; x++ )
        {
            for (int y = 0; y < MyBattleOrder.MAX_FORMATION_COL; y++)
            {
                if (!formation[index].Equals(Vector3.zero))
                {
                    if (formation[index].y < 4f && n < count)
                    {
                        Vector3 targetpos   = formation[index] * MyBattleOrder.MAX_FORMATION_SPACE;
                        targetpos           = tf.TransformPoint(targetpos);
                        targetpos.y         = tf.position.y;
                        members[n].SetTargetPosition(targetpos);
                        n++;
                    }
                }
                index++;
            }
        }
    }

    // --------------------------------------------------------------------------------------
    /// <summary>
    /// 编队保持防御队形
    /// </summary>
    /// --------------------------------------------------------------------------------------
    private void UpdateDefensiveFormation(int frame, float interval)
    {
        Vector3[] formation = MyBattleOrder.DefensiveFormation;
        Transform tf        = Leader.entity.go.transform;
        int index           = 0;
        int n               = 0;
        for (int x = 0; x < MyBattleOrder.MAX_FORMATION_ROW; x++)
        {
            for (int y = 0; y < MyBattleOrder.MAX_FORMATION_COL; y++)
            {
                if (!formation[index].Equals(Vector3.zero))
                {
                    
                    if (formation[index].y < 4f && n < members.Count)
                    {
                        Vector3 targetpos   = formation[index] * MyBattleOrder.MAX_FORMATION_SPACE;
                        targetpos           = tf.TransformPoint(targetpos);
                        targetpos.y         = tf.position.y;
                        members[n].SetTargetPosition(targetpos);
                        n++;
                    }
                }
                index++;
            }
        }
    }

    // --------------------------------------------------------------------------------------
    /// <summary>
    /// 编队保持包围队形
    /// </summary>
    /// --------------------------------------------------------------------------------------
    private void UpdateSurroundFormation(int frame, float interval)
    {
        Vector3[] formation = MyBattleOrder.SurroundFormation;
        Transform tf        = Leader.entity.go.transform;
        int index           = 0;
        int n               = 0;
        for (int x = 0; x < MyBattleOrder.MAX_FORMATION_ROW; x++)
        {
            for (int y = 0; y < MyBattleOrder.MAX_FORMATION_COL; y++)
            {
                if (!formation[index].Equals(Vector3.zero) )
                {
                    if (formation[index].y < 4f && n < members.Count )
                    {
                        Vector3 targetpos   = formation[index] * MyBattleOrder.MAX_FORMATION_SPACE;
                        targetpos           = tf.TransformPoint(targetpos);
                        targetpos.y         = tf.position.y;
                        members[n].SetTargetPosition(targetpos);
                        n++;
                    }
                }
                index++;
            }
        }
    }

    public void OnBattleMove( Node from, Node to, bool bWarp )
    {
        if( bWarp )
        {
            /// 开始跳跃特效
            EffectManager.Get().PlayBufferEffect(Leader.GetPosition(), "IonMarker", 3f);
        }

        FadeInMembers();

        Leader.MoveTo(to, bWarp);
        for (int i = 0; i < members.Count; i++)
        {
            members[i].MoveToFly(to);
        }

        btState         = BattleTeamState.Move;
        btFormation     = Formation.FormationMove;
    }


    private void FadeInMembers()
    {
        Leader.entity.go.SetActive(true);
        Leader.isNeedUpdate = true;
        for (int i = 0; i < members.Count; i++)
        {
            members[i].entity.go.SetActive(true);
            members[i].isNeedUpdate = true;
        }
    }

    private void UpdateEnterCity( int frame, float interval )
    {
        bool bEnterCity     = true;
        bEnterCity          = Leader.entity.UpdateMove(frame, interval);
        for (int i = 0; i < members.Count; i++)
        {
            bEnterCity      = members[i].entity.IsNeedMove();
            if (bEnterCity)
                return;
        }

        if(!bEnterCity )
        {
            btState         = BattleTeamState.Idle;
            btFormation     = Formation.FormationNull;
            Leader.isNeedUpdate = false;
            Leader.entity.go.SetActive( false );
            for (int i = 0; i < members.Count; i++)
            {
                members[i].isNeedUpdate = false;
                members[i].entity.go.SetActive(false);
            }
        }
    }

    private bool IsReadyTarget()
    {
        bool bEnterCity = Leader.entity.IsNeedMove();
        for (int i = 0; i < members.Count; i++)
        {
            bEnterCity = members[i].entity.IsNeedMove();
        }
        return bEnterCity;
    }


    /// <summary>
    /// 计算相机的观测目标
    /// </summary>
    public void ChangeShipsStats(MemberState state )
    {
        this.Leader.entity.shipState = state;
        for( int i = 0; i < members.Count; i++ )
        {
            members[i].entity.shipState = state;
        }
    }


    /// <summary>
    /// 处理围城指令
    /// </summary>
    public void EntersEncircleCity()
    {
        Leader.entity.ClearTarget();
        Leader.PriorityEncircleCity();
        for (int i = 0; i < members.Count; i++)
            members[i].entity.ClearTarget();

        btState      = BattleTeamState.Encirclecity;
        btFormation  = Formation.FormationSurround;
    }

    /// <summary>
    /// 战队进入战斗状态
    /// </summary>
    public void EnterBattleStats( Node node = null, TEAM atkTeam = TEAM.Neutral )
    {
        if ( btState == BattleTeamState.Encirclecity || btState == BattleTeamState.Defensive )
        {
            return;
        }

        if( node != null )
        {
            Vector3 enemyPostion            = Vector3.zero;
            List<BattleTeam> battleArray    = node.battArray;
            foreach( var bt in battleArray )
            {
                if (bt.team.team == atkTeam)
                {
                    enemyPostion            = bt.Leader.GetPosition();
                    bt.btState              = BattleTeamState.Encirclecity;
                    bt.btFormation          = Formation.FormationAttack;
                }
            }

            foreach( var bt in battleArray )
            {
                if( bt.team.team != atkTeam )
                {
                    bt.LeaveCity(enemyPostion);
                    bt.btState              = BattleTeamState.Defensive;
                    bt.btFormation          = Formation.FormationDefensive;
                    bt.FadeInMembers();
                }
            }
        }
    }

    /// <summary>
    /// 改变战队队形
    /// </summary>
    public void UpdateBattleTeamFormation( Formation  eformation )
    {
        if (btFormation != eformation)
        {
            btFormation = eformation;
        }
    }

   
    /// -----------------------------------------------------------------------------------------
    /// <summary>
    /// 战队进入某个星球
    /// </summary>
    /// -----------------------------------------------------------------------------------------
    public void EnterNode( Node node, bool bEnter = false )
    {
        if( node != null )
        {
            if (bEnter)
            {
               node.AddMember(Leader);
                for (int i = 0; i < members.Count; i++)
                {
                    node.AddMember(members[i]);
                }
            }
            node.battArray.Add(this);
            curentNode      = node;
        }
    }

    /// -----------------------------------------------------------------------------------------
    /// <summary>
    /// 战队离开某个星球
    /// </summary>
    /// -----------------------------------------------------------------------------------------
    public void LeaveNode(Node node)
    {
        node.RemoveShip(Leader);
        for (int i = 0; i < members.Count; i++)
        {
            node.RemoveShip(members[i]);
        }

        node.battArray.Remove(this);
        curentNode = null;
    }


    /// -----------------------------------------------------------------------------------------
    /// <summary>
    /// 从显示逻辑上控制战队成员，进入城堡
    /// </summary>
    /// -----------------------------------------------------------------------------------------
    public void EnterCity( Node city )
    {
        btState             = BattleTeamState.EnterCity;
        Leader.SetTargetPosition(city.GetPosition());
        for (int i = 0; i < members.Count; i++)
        {
            members[i].SetTargetPosition(city.GetPosition());
        }
    }

    /// <summary>
    /// 防御队伍出城
    /// </summary>
    /// <param name="city"></param>
    /// <param name="targetPostion"></param>
    private void LeaveCity( Vector3 targetPostion )
    {
        Node city               = Leader.currentNode;
        Vector3 cityPosition    = city.GetPosition();
        Vector3 moveDir         = targetPostion - cityPosition;
        moveDir.Normalize();
        cityPosition            = cityPosition + (moveDir * 10f);
        Leader.SetTargetPosition(cityPosition);
    }

    
    /// -----------------------------------------------------------------------------------------
    /// <summary>
    /// 清空战队所有的目标
    /// </summary>
    /// -----------------------------------------------------------------------------------------
    public void ClearTarget()
    {
        Leader.entity.ClearTarget();
        for (int i = 0; i < members.Count; i++)
            members[i].entity.ClearTarget();
    }


    /// -----------------------------------------------------------------------------------------
    /// <summary>
    /// 传送队伍
    /// </summary>
    /// -----------------------------------------------------------------------------------------
    public void DeliverTeam( Vector3 postion, Node targetNode)
    {
        Leader.LeaveNode();
        Leader.entity.SetPosition(postion);
        Leader.EnterNode(targetNode);
        Leader.entity.shipState = MemberState.ORBIT;

        for (int i = 0; i < members.Count; i++)
        {
            float angle             = Random.value;
            float x                 = 3f * Mathf.Cos(angle) + postion.x;
            float y                 = 3f * Mathf.Sin(angle) + postion.z;

            members[i].LeaveNode();
            members[i].SetPosition(new Vector3(x, postion.y, y));
            members[i].EnterNode(targetNode);
        }
    }
}

