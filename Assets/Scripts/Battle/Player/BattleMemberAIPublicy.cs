using System;
using System.Collections.Generic;
using UnityEngine;






public class BattleMemberAIPublicy
{
    public enum Publicy
    {
        active,         // 主动攻击
        defense,        // 主动防御，然后攻击
    }

    private BattleMember            mOwer = null;
    private Publicy                 mPublicy = Publicy.active;

    /// <summary>
    /// 技能池子
    /// </summary>
    private List<ArticleEntiy>      skillpool = new List<ArticleEntiy>();


    /// <summary>
    /// 是否游走CD中
    /// </summary>
    private bool                    Iswandering = false;
    private float                   wanderTimer = 0f;
    private float                   wanderMaxTimer = 5f;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="bm"></param>
    public void Init( BattleMember bm, Publicy publicy )
    {
        mOwer               = bm;
        mPublicy            = publicy;
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 自动攻击
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    public void AotuBattle( int frame, float dt )
    {
        
        /// 战内攻击策略
        {
            attackPublicy( frame, dt );
        }
        
        /// 战内游走策略
        {
            wanderPublicy( frame, dt );
        }

        /// -------------------------游走逻辑--------------------------------
        if( Iswandering )
        {
            wanderTimer     += dt;
            if( wanderTimer >= wanderMaxTimer )
            {
                wanderTimer = 0f;
                Iswandering = false;
            }
        }

        /// 使用技能
        foreach( var it in skillpool )
        {
            it.Tick(frame, dt);
        }
    }



    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 是否在警戒范围
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private bool IsAlertRange()
    {
        if (mOwer.target != null && mOwer.target.isALive)
        {
            float fRange    = mOwer.GetAtt(ShipAttr.WarningRange);
            fRange          *= fRange;
            float distance  = (mOwer.GetPosition() - mOwer.target.GetPosition()).sqrMagnitude;
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
    private bool IsAtkRange()
    {
        if( mOwer.target != null && mOwer.target.isALive )
        {
            float fRange        = mOwer.GetAtt( ShipAttr.AttackRange );
            fRange              *= fRange;
            float distance      = (mOwer.GetPosition() - mOwer.target.GetPosition()).sqrMagnitude;
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
        mOwer.target            = FindNearestEnemy();
        if( mOwer.target == null ) return;
        if( IsAtkRange() )
        {
            mOwer.entity.UpdateBattle();
        }
        else
        {
            mOwer.entity.UpdateMove( frame, dt );
        }
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 游走策略
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private void wanderPublicy( int frame, float dt )
    {
        if (mPublicy == Publicy.defense)
        {
            int curHP           = this.mOwer.GetAtt(ShipAttr.Hp);
            int maxHP           = this.mOwer.GetAtt(ShipAttr.MaxHp);
            if( curHP < (int)(maxHP * 0.5f) && !Iswandering )
            {
                Vector3 target  = HexMap.Instance.Find3x3MinCost( mOwer.GetPosition() );
                Iswandering     = true;
                wanderTimer     = 0f;
                mOwer.SetTargetPosition(target);
            }

            mOwer.entity.UpdateMove( frame, dt );
        }

        if( mPublicy == Publicy.active )
        {

        }
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 求救
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private void callHelpPublicy()
    {

    }



    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 评估风险策略
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    private Vector3 appraisePublicy( )
    {
        Vector3 result      = Vector3.zero;
        if(mPublicy == Publicy.active )
        {
            ///  主动
        }

        
        if ( mPublicy == Publicy.defense )
        {
            ///  被动
        }
        return result;
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
                if (distance <= fMax && member.isALive && member.unitType == BattleMember.BattleUnitType.Member)
                {
                    nearestEnemy    = member;
                    fMax            = distance;
                }
            }
        }

        return nearestEnemy;
    }

    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 优先攻击最近的英雄
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    public void PriorityAttackNearestHero()
    {
        if (mOwer.currentNode == null)
            return;

        float fMax                  = float.MaxValue;
        BattleMember nearestEnemy   = null;
        List<BattleTeam> arrays     = mOwer.currentNode.battArray;
        foreach (BattleTeam bt in arrays)
        {
            if (bt.team.team == mOwer.team)
                continue;

            List<BattleMember> members = bt.members;
            foreach (var member in members)
            {
                float distance = (mOwer.GetPosition() - member.GetPosition()).sqrMagnitude;
                if (distance <= fMax && member.isALive && member.unitType == BattleMember.BattleUnitType.Member)
                {
                    nearestEnemy = member;
                    fMax = distance;
                }
            }
        }
    }

}

