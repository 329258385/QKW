using System;
using System.Collections.Generic;
using UnityEngine;






public class BattleMemberAIPublicy
{
    /// <summary>
    /// 战斗状态
    /// </summary>
    public enum Battlestate
    {
        Move,                       // 移动
        Escape,                     // 逃跑
        Repel,                      // 击退
        Battle,                     // 战斗
        Wander,                     // 选择游走点
        SerachEntiy,                // 索敌
        AtkCity,
        Death,
    }


    public enum Publicy
    {
        active,                     // 主动攻击
        defense,                    // 主动防御，然后攻击
    }

    private BattleMember            mOwer = null;
    private Publicy                 mPublicy = Publicy.active;
    private Battlestate             mBattleStates = Battlestate.Battle;

    /// <summary>
    /// 是否游走CD中
    /// </summary>
    private bool                    Iswandering = false;
    private float                   wanderTimer = 0f;
    private float                   wanderMaxTimer = 5f;

    private float                    repelTimer = 0f;

    /// <summary>
	/// The attack time.
	/// </summary>
    private float                   AttackTime  = 0f;

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
        /// 索敌策略
        searchEntiyPublicy(frame, dt);
        if ( mBattleStates == Battlestate.Move )
        {
            bool IsNeedMove     = mOwer.entity.UpdateMove( frame, dt );
            if (!IsNeedMove)
            {
                mBattleStates   = Battlestate.Battle;
                return;
            }
        }

        else if ( mBattleStates == Battlestate.SerachEntiy )
        {
            searchEntiyPublicy( frame, dt );
        }

        else if (mBattleStates == Battlestate.Battle)
        {
            /// 战内攻击策略
            AttackTime += dt;
            float AttackSpeed  = mOwer.GetAtt(ShipAttr.AttackSpeed);
            if (AttackTime < AttackSpeed)
                return;

            AttackTime = 0f;
            attackPublicy(frame, dt);
        }

        else if( mBattleStates == Battlestate.Wander )
        {
            if( !Iswandering )
            {
                wanderPublicy(frame, dt);
            }

            /// 游走逻辑
            if (Iswandering)
            {
                wanderTimer     += dt;
                if (wanderTimer >= wanderMaxTimer)
                {
                    wanderTimer = 0f;
                    Iswandering = false;
                }
            }
        }

        else if ( mBattleStates == Battlestate.AtkCity )
        {
            // 攻城
            atkCityPublicy();
        }

        else if ( mBattleStates == Battlestate.Escape )
        {
            /// 逃跑逻辑
            OnEscape();
        }

        else if( mBattleStates == Battlestate.Repel )
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
        if( mOwer.entity.target == null ) return;
        if( IsAtkRange() )
        {
            if( IsAlertRange() && mPublicy == Publicy.defense )
            {
                mBattleStates       = Battlestate.Wander;
            }
            else
            {
                mOwer.entity.UpdateBattle();
            }
        }
        else
        {
            mBattleStates           = Battlestate.SerachEntiy;
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
            float fAlertRange   = mOwer.GetAtt(ShipAttr.WarningRange);
            float fAtkRange     = mOwer.GetAtt(ShipAttr.AttackRange);
            Vector3 moveDir     = mOwer.GetPosition() - mOwer.entity.target.GetPosition();
            float distance      = moveDir.magnitude;

            if( /*distance >= fAlertRange &&*/ distance < fAtkRange )
            {
                mBattleStates   = Battlestate.Battle;
            }
            //else if ( distance < fAlertRange )
            //{
            //    moveDir         = moveDir.normalized;
            //    Vector3 tarPos  = mOwer.GetPosition() - moveDir * fAlertRange;
            //    mBattleStates   = Battlestate.Move;
            //    mOwer.SetTargetPosition(tarPos);
            //}
            else if( distance > fAtkRange )
            {
                Vector3 tarPos      = mOwer.entity.target.GetPosition();
                mBattleStates       = Battlestate.Move;
                mOwer.SetTargetPosition(tarPos);
            }
        }
        else
        {
            if( IsNeedAtkCity() )
            {
                if ( IsAtkCity())
                    mBattleStates   = Battlestate.AtkCity;
                else
                {
                    Vector3 tarPos  = mOwer.entity.targetNode.GetPosition();
                    mBattleStates   = Battlestate.Move;
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
    /// 逃跑
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
            mBattleStates = Battlestate.Battle;
            return;
        }

        repelTimer += dt;
        if( repelTimer > 1.5f )
        {

        }
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
}

