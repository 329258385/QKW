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

public partial class BattleMember
{
    /// <summary>
    /// 当前状态
    /// </summary>
    private BattleMemberStatus      _eStatus = BattleMemberStatus.status_Unknown;

    /// <summary>
    /// 攻击间隔
    /// </summary>
    private int                     attacktimer = 0;


    public void ResetAttackTimer()
    {
        attacktimer                 = BattleSystem.Instance.battleData.rand.Range(50, 100);
    }

    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 自动攻击
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    public void AotuBattle( int frame, float dt )
    {
        searchEntiyPublicy( frame, dt );

        if ( _eStatus == BattleMemberStatus.status_Move )
        {
            UpdateMove(frame, dt);
        }

        if (_eStatus == BattleMemberStatus.status_Attack)
        {
            attackPublicy(frame, dt);
        }
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 是否在攻击范围
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private bool IsAtkRange( )
    {
        if( target != null && target.isALive )
        {
            float fRange        = GetAtt( ShipAttr.AttackRange );
            fRange              *= fRange;
            float distance      = (GetPosition() - target.GetPosition()).sqrMagnitude;
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
        if( targetNode != null )
        {
            float fRange        = GetAtt( ShipAttr.AttackRange );
            fRange              *= fRange;
            float distance      = (GetPosition() - targetNode.GetPosition()).sqrMagnitude;
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
        attacktimer--;
        if (attacktimer < GetAtt(ShipAttr.AttackSpeed))
            return;

        if ( target == null && targetNode == null ) 
            return;

        EventGroup.fireEvent((int)BattleEvent.Attack, this, null);
        ResetAttackTimer();
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 索敌策略
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private void searchEntiyPublicy( int frame, float dt )
    {
        target     = FindNearestEnemy();
        if( target != null )
        {
            float fAtkRange     = GetAtt(ShipAttr.AttackRange);
            Vector3 moveDir     = GetPosition() - target.GetPosition();
            float distance      = moveDir.magnitude;

            if( distance < fAtkRange )
            {
                _eStatus        =  BattleMemberStatus.status_Attack;
            }
            else if( distance > fAtkRange )
            {
                _eStatus        = BattleMemberStatus.status_Move;
                Vector3 tarPos  = target.GetPosition();
                RandomAttackPoint(tarPos);
            }
        }
        
        if( targetNode != null )
        {
            if (IsAtkCity())
            {
                _eStatus        = BattleMemberStatus.status_Attack;
            }
            else
            {
                _eStatus        = BattleMemberStatus.status_Move;
                Vector3 tarPos  = targetNode.GetPosition();
                RandomAttackPoint(tarPos);
            }
        }
    }

    public bool CanAttack( BattleMember member )
    {
        return false;
    }


    private void RandomAttackPoint( Vector3 targetPos )
    {
        float randX          = BattleSystem.Instance.battleData.rand.Range(-2.5f, 2.5f);
        float randZ          = BattleSystem.Instance.battleData.rand.Range(-2.5f, 2.5f);

        Vector3 rayStart     = new Vector3(targetPos.x + randX, 2.0f, targetPos.z + randZ);
       
        RaycastHit hit;
        if (Physics.Raycast(rayStart, -Vector3.up, out hit, 2f))
        {
            rayStart         = hit.point;
        }

        SetTargetPos(rayStart);
        EventGroup.fireEvent((int)BattleEvent.MoveToTarget, this, null);
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
    /// 找最近的敌人
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private BattleMember FindNearestEnemy()
    {
        float fMax                  = float.MaxValue;
        BattleMember nearestEnemy   = null;
        List<BattleTeam> arrays     = currentNode.battArray;
        foreach (BattleTeam bt in arrays)
        {
            if (bt.team.team == team )
                continue;

            List<BattleMember> members = bt.members;
            foreach (var member in members)
            {
                float distance      = (GetPosition() - member.GetPosition()).sqrMagnitude;
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

