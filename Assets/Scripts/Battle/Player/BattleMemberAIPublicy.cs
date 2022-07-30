using Solarmax;
using System;
using System.Collections.Generic;
using UnityEngine;





public enum BattleMemberStatus
{
    status_Unknown = 0,             // 未知
    status_Move,                    // 移动
    status_Attack,                  // 攻击
    status_Dead,                    // 死亡
    status_Escape,                  // 溃逃
    status_Wander,                  // 游走
    status_AtkCity,                 // 围城
    status_Repel,                   // 击退
}


/// <summary>
/// 战斗单元类型
/// </summary>
public enum BattleMemberType
{
    BMT_0,          // 轻步兵
    BMT_1,          // 重步兵
    BMT_2,          // 盾排兵
    BMT_3,          // 轻骑兵
    BMT_4,          // 重骑兵
    BMT_5,          // 弓箭兵
    BMT_6,          // 飞行兵
    BMT_7,          // 炮兵兵
}


public class BattleMemberAIPublicy
{
    private BattleMember            mOwer = null;

    /// <summary>
    /// 当前状态
    /// </summary>
    private BattleMemberStatus      _eStatus = BattleMemberStatus.status_Unknown;

    /// <summary>
    /// 是否游走CD中
    /// </summary>
    private bool                    Iswandering = false;

    /// <summary>
    /// 技能池子
    /// </summary>
    private List<TechniqueEntiy>    techniques = new List<TechniqueEntiy>();

    public EventHandlerGroup        EventGroup { get; set; }

    /// <summary>
    /// 攻击间隔
    /// </summary>
    private float                   attacktimer = 0.0f;

    /// <summary>
    /// 游走间隔
    /// </summary>
    private float                   wandertimer = 0.0f;



    public BattleMemberAIPublicy( BattleMember bm )
    {
        mOwer               = bm;
    }

    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 自动攻击
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    public void AotuBattle( int frame, float dt )
    {
        /// 更新技能CD
        TechniqueEntiy technique = null;
        foreach ( var instance in techniques )
        {
            instance.Tick( frame, dt );
        }

        /// 索敌策略
        searchEntiyPublicy(frame, dt);
        if ( _eStatus == BattleMemberStatus.status_Move )
        {
            if (!mOwer.entity.UpdateMove(frame, dt))
            {
                _eStatus = BattleMemberStatus.status_Attack;
                return;
            }
        }

        if (_eStatus == BattleMemberStatus.status_Attack)
        {

            attackPublicy(frame, dt);
        }

        if(_eStatus == BattleMemberStatus.status_Wander )
        {
             wanderPublicy(frame, dt);
        }

        if (_eStatus == BattleMemberStatus.status_AtkCity )
        {
            // 攻城
            atkCityPublicy();
        }

        if (_eStatus == BattleMemberStatus.status_Escape )
        {
            /// 逃跑逻辑
            OnEscape();
        }

        if( _eStatus == BattleMemberStatus.status_Repel )
        {
            /// 击退逻辑
            OnRepel( frame, dt );
        }
    }



    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 是否在警戒范围
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private bool IsAlertRange()
    {
        if (mOwer.entity.target != null && mOwer.entity.target.isALive)
        {
            float fRange    = mOwer.GetAtt(ShipAttr.WarningRange);
            fRange          *= fRange;
            float distance  = (mOwer.GetPosition() - mOwer.entity.target.GetPosition()).sqrMagnitude;
            if (distance <= fRange)
            {
                return true;
            }
        }
        return false;
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 是否在攻击范围
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private bool IsAtkRange( )
    {
        if( mOwer.entity.target != null && mOwer.entity.target.isALive )
        {
            float fRange        = mOwer.GetAtt( ShipAttr.AttackRange );
            fRange              *= fRange;
            float distance      = (mOwer.GetPosition() - mOwer.entity.target.GetPosition()).sqrMagnitude;
            if (distance <= fRange )
            {
                return true;
            }
        }
        return false;
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 是否能攻击城
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private bool IsAtkCity( )
    {
        if( mOwer.entity.targetNode != null )
        {
            float fRange        = mOwer.GetAtt( ShipAttr.AttackRange );
            fRange              *= fRange;
            float distance      = (mOwer.GetPosition() - mOwer.entity.targetNode.GetPosition()).sqrMagnitude;
            if (distance <= fRange )
            {
                return true;
            }
        }
        return false;
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 攻击策略
    /// </summary>
    ///  --------------------------------------------------------------------------------------------------------
    private void attackPublicy( int frame, float dt )
    {
        attacktimer         += dt;
        if (attacktimer < mOwer.GetAtt(ShipAttr.AttackSpeed))
            return;

        attacktimer          = 0.0f;
        if ( mOwer.entity.target == null ) return;
        if( IsAtkRange() )
        {
            mOwer.entity.UpdateBattle();
        }
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 索敌策略
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private void searchEntiyPublicy( int frame, float dt )
    {
        mOwer.entity.target = FindNearestEnemy();
        if( mOwer.entity.target != null )
        {
            float fAtkRange     = mOwer.GetAtt(ShipAttr.AttackRange);
            Vector3 moveDir     = mOwer.GetPosition() - mOwer.entity.target.GetPosition();
            float distance      = moveDir.magnitude;

            if( distance < fAtkRange )
            {
                _eStatus        =  BattleMemberStatus.status_Attack;
            }
            else if( distance > fAtkRange )
            {
                _eStatus        = BattleMemberStatus.status_Move;
                Vector3 tarPos  = mOwer.entity.target.GetPosition();
                mOwer.SetTargetPosition(tarPos);
            }
        }
        else
        {
            if( IsNeedAtkCity() )
            {
                if ( IsAtkCity())
                    _eStatus        = BattleMemberStatus.status_AtkCity;
                else
                {
                    Vector3 tarPos  = mOwer.entity.targetNode.GetPosition();
                    _eStatus        = BattleMemberStatus.status_Move;
                    mOwer.SetTargetPosition(tarPos);
                }
            }
        }
    }

    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 游走策略
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private void wanderPublicy( int frame, float dt )
    {
        int curHP           = this.mOwer.GetAtt(ShipAttr.Hp);
        int maxHP           = this.mOwer.GetAtt(ShipAttr.MaxHp);
        if( curHP < (int)(maxHP * 0.5f) && !Iswandering )
        {
            Vector3 target  = Vector3.zero;
            Iswandering     = true;
            wandertimer     = 0f;
            mOwer.SetTargetPosition(target);
        }

        mOwer.entity.UpdateMove( frame, dt );
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 求救
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private TechniqueEntiy UseSkillPublicy( TechniqueEntiy technique )
    {
        if( technique != null )
        {
            technique.ApplyTechnique();
            technique.StardCD();
        }
        return null;
    }



    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 攻城
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private void atkCityPublicy( )
    {
        if ( mOwer.entity.targetNode != null )
        {
            mOwer.entity.UpdateBattle(true);
        }
    }


    private bool IsNeedAtkCity()
    {
        int nTeam = 0;
        for (int i = 0; i < (int)TEAM.TeamMax; i++)
        {
            if (mOwer.currentNode.GetShipCount(i) == 0)
                continue;

            nTeam++;
        }

        return nTeam == 1 ? true : false;
    }


    /// <summary>
    /// 逃跑逻辑
    /// </summary>
    private void OnEscape()
    {

    }

    /// <summary>
    /// 击退
    /// </summary>
    private void OnRepel( int frame, float dt )
    {
        bool IsNeedMove   = mOwer.entity.UpdateMove(frame, dt);
        if (!IsNeedMove)
        {
            this._eStatus =  BattleMemberStatus.status_Attack;
            return;
        }

        //repelTimer += dt;
        //if( repelTimer > 1.5f )
        //{

        //}
    }

    public void MovetoTargetPos()
    {
        _eStatus = BattleMemberStatus.status_Move;
    }

    public void Attack()
    {
        _eStatus = BattleMemberStatus.status_Attack;
    }


    public void Dead()
    {
        _eStatus = BattleMemberStatus.status_Dead;
    }


    public void EnterEscape()
    {
        _eStatus = BattleMemberStatus.status_Escape;
    }

    public void ExitEscape()
    {
        _eStatus = BattleMemberStatus.status_Dead;
    }

    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 找最近的敌人
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private BattleMember FindNearestEnemy()
    {
        Node currentNode            = mOwer.currentNode;
        float fMax                  = float.MaxValue;
        BattleMember nearestEnemy   = null;
        List<BattleTeam> arrays     = currentNode.battArray;
        foreach (BattleTeam bt in arrays)
        {
            if (bt.team.team == mOwer.team )
                continue;

            List<BattleMember> members = bt.members;
            foreach (var member in members)
            {
                float distance      = (mOwer.GetPosition() - member.GetPosition()).sqrMagnitude;
                if (distance <= fMax && member.isALive && member.unitType == BattleMember.BattleUnitType.bmt_Soldier )
                {
                    nearestEnemy    = member;
                    fMax            = distance;
                }
            }
        }

        return nearestEnemy;
    }
}

