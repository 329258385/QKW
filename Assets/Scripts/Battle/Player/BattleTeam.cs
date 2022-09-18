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

/// <summary>
/// 战斗单元对象
/// </summary>
public class BattleTeam
{
    readonly int                MAX_MEMBERS = 8;
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
    /// 函数集
    /// </summary>
    RunLockStepLogic[]          handler { get; set; }

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
                nSum += members[i].GetAtt(ShipAttr.Population);
            }
            return nSum;
        }
    }

    public BattleTeam( )
    {
        ID                                                  = GenIDPool++;
        handler                                             = new RunLockStepLogic[(int)BattleTeamState.Max];
        handler[(int)BattleTeamState.Move]                  = UpdateMove;
        handler[(int)BattleTeamState.Battle]                = UpdateBattle;
        handler[(int)BattleTeamState.Defensive]             = UpdateMove;
        handler[(int)BattleTeamState.Encirclecity]          = UpdateMove;
        handler[(int)BattleTeamState.EnterCity]             = UpdateEnterCity;
    }


    public void Tick(int frame, float interval)
    {
        handler[(int)btState](frame, interval);
    }

    /// ---------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 空闲
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------------
    void UpdateBattle(int frame, float dt)
    {
        for (int i = 0; i < members.Count; i++)
        {
            bool IsNeedMove = members[i].IsNeedMove();
            if (IsNeedMove)
                members[i].UpdateMove(frame, dt);
            else
                members[i].UpdateBattle();
        }
    }


    /// ---------------------------------------------------------------------------------------------
    /// <summary>
    /// 移动
    /// </summary>
    /// ---------------------------------------------------------------------------------------------
    private void UpdateMove(int frame, float interval)
    {
        Transform tf = Leader.entity.go.transform;
        for (int i = 0; i < members.Count; i++)
        {
            Vector3 newPos = tf.TransformPoint(MyBattleOrder.MarchFormation[i] * MyBattleOrder.MAX_FORMATION_SPACE);
            members[i].SetTargetPos(newPos);
        }

        for (int i = 0; i < members.Count; i++)
        {
            bool IsNeedMove = members[i].IsNeedMove();
            if (IsNeedMove)
                members[i].UpdateMove(frame, interval);
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


    /// ---------------------------------------------------------------------------------------------
    /// <summary>
    /// 创捷飞行编队
    /// </summary>
    /// ---------------------------------------------------------------------------------------------
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
                for (int i = 0; i < MAX_MEMBERS; i++)
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

    /// ---------------------------------------------------------------------------------------------
    /// <summary>
    /// 战斗移动
    /// </summary>
    /// ---------------------------------------------------------------------------------------------
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
            members[i].EventGroup.fireEvent((int)BattleEvent.MoveToTarget, this, null);
        }

        btState         = BattleTeamState.Move;
    }

    /// -------------------------------------------------------------------------------------
    /// <summary>
    /// 战队进入战斗状态
    /// </summary>
    /// -------------------------------------------------------------------------------------
    public void EnterBattleStats(Node node = null, TEAM atkTeam = TEAM.Neutral)
    {
        if (btState == BattleTeamState.Encirclecity || btState == BattleTeamState.Defensive)
        {
            return;
        }

        if (node != null)
        {
            Vector3 enemyPostion = Vector3.zero;
            List<BattleTeam> battleArray = node.battArray;
            foreach (var bt in battleArray)
            {
                if (bt.team.team != atkTeam)
                {
                    bt.LeaveCity(enemyPostion);
                    bt.btState      = BattleTeamState.Defensive;
                    bt.FadeInMembers();
                }
            }
        }
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

    /// ----------------------------------------------------------------------------------------
    /// 进入城
    /// ----------------------------------------------------------------------------------------
    private void UpdateEnterCity( int frame, float interval )
    {
        bool bEnterCity = Leader.UpdateMove(frame, interval);
        for (int i = 0; i < members.Count; i++)
        {
            bEnterCity      = members[i].IsNeedMove();
            if (bEnterCity)
                return;
        }

        if(!bEnterCity )
        {
            btState         = BattleTeamState.Idle;
            Leader.isNeedUpdate = false;
            Leader.entity.go.SetActive( false );
            for (int i = 0; i < members.Count; i++)
            {
                members[i].isNeedUpdate = false;
                members[i].entity.go.SetActive(false);
            }
        }
    }

    /// ----------------------------------------------------------------------------------------
    /// <summary>
    /// 计算相机的观测目标
    /// </summary>
    /// ----------------------------------------------------------------------------------------
    public void ChangeShipsStats(MemberState state )
    {
        this.Leader.shipState = state;
        for( int i = 0; i < members.Count; i++ )
        {
            members[i].shipState = state;
        }
    }

    /// ----------------------------------------------------------------------------------------
    /// <summary>
    /// 处理围城指令
    /// </summary>
    /// ----------------------------------------------------------------------------------------
    public void EntersEncircleCity()
    {
        Leader.ClearTarget();
        Leader.PriorityEncircleCity();
        for (int i = 0; i < members.Count; i++)
            members[i].ClearTarget();

        btState      = BattleTeamState.Encirclecity;
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
                Leader.EventGroup.fireEvent((int)BattleEvent.Stop, this, null);
                for (int i = 0; i < members.Count; i++)
                {
                    node.AddMember(members[i]);
                    members[i].EventGroup.fireEvent((int)BattleEvent.Stop, this, null);
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
        Leader.SetTargetPos(city.GetPosition());
        for (int i = 0; i < members.Count; i++)
        {
            members[i].SetTargetPos(city.GetPosition());
        }
    }

    /// ----------------------------------------------------------------------------------------
    /// <summary>
    /// 防御队伍出城
    /// </summary>
    /// ----------------------------------------------------------------------------------------
    private void LeaveCity( Vector3 targetPostion )
    {
        Node city               = Leader.currentNode;
        Vector3 cityPosition    = city.GetPosition();
        Vector3 moveDir         = targetPostion - cityPosition;
        moveDir.Normalize();
        cityPosition            = cityPosition + (moveDir * 7.5f);
        Leader.SetTargetPos(cityPosition);
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
        Leader.shipState = MemberState.ORBIT;

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

