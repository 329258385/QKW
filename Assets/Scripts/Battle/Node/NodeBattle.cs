using UnityEngine;
using System;
using Solarmax;
using System.Collections;
using System.Collections.Generic;





/// <summary>
/// 战斗模块
/// </summary>
public partial class Node 
{
    static int                      MULT_COUNT = 5;
    static bool                     MULT_FLAG = false;
    static float                    MULT_NUM = 2f;

    List<TechniqueEntiy>            m_mapSkill = new List<TechniqueEntiy>();
    public TechniqueEntiy           currentSkill = null;


    public SceneManager             sceneManager { get; set; }

	public List<Team>               m_teamArray  = new List<Team>();
	public List<float>              m_HPArray    = new List<float>();

    /// <summary>
    /// 伤害列表
    /// </summary>
    float[]                         dmgs = new float[(int)TEAM.TeamMax];


    public void InitSkills( string skilllib )
    {
        if (skilllib == "NULL")
            return;
    }


	/// <summary>
	/// 战斗
	/// </summary>
	/// <param name="frame">Frame.</param>
	/// <param name="dt">Dt.</param>
	protected void UpdateBattle(int frame, float dt)
	{
		if (state != NodeState.Battle)
			return;

        //计算伤害
        CalcDamage(dt);

        //分摊伤害
        DamageToShip();

        BattleHUD();
    }

    /// ---------------------------------------------------------------------------------------
    /// <summary>
    /// 更新技能逻辑
    /// </summary>
    /// ---------------------------------------------------------------------------------------
    protected void UpdateNodeSkill(int frame, float dt)
    {
        // 增加使用技能行为 fix by ljp
        TickSkillCD(frame, dt);

        AttackTime += dt;
        if (AttackTime < AttackSpeed)
            return;

        AttackTime = 0;
        AutoUseSkill();
    }
	
    /// <summary>
    /// 星球使用技能
    /// </summary>
    void AutoUseSkill()
    {
        if (!currentTeam.Valid())
            return;

        PollcySelectSkill();
    }

    /// <summary>
    /// 选择技能策略
    /// </summary>
    void PollcySelectSkill()
    {
        if (m_mapSkill.Count > 0)
        {
            currentSkill = m_mapSkill[0];
        }
    }


    /// <summary>
    /// tick 技能 cd
    /// </summary>
    void TickSkillCD( int frame, float interval )
    {
        if (!currentTeam.Valid() )
            return;

        for ( int i = 0; i < m_mapSkill.Count; i++) 
        {
			m_mapSkill[i].Tick (frame, interval);
		}
    }


    /// ------------------------------------------------------------------------------------------
    /// <summary>
    /// 更新战斗进度
    /// </summary>
    /// ------------------------------------------------------------------------------------------
	void BattleHUD()
	{
		#if !SERVER
		m_HPArray.Clear();
		m_teamArray.Clear ();

        for (int i = 1; i < (int)TEAM.TeamMax; ++i) 
		{
			int shipNum = numArray [i];
			if (shipNum == 0)
				continue;

			Team team = nodeManager.sceneManager.teamManager.GetTeam ((TEAM)i);
			m_teamArray.Add (team);
			m_HPArray.Add (shipNum);
		}

        
        {
            mCityHUD.ShowCity(HUDCityOperater.UIPanel.Battle);
            mCityHUD.ShowPopulationProcess(m_teamArray, m_HPArray);
        }
        #endif
    }


    /// <summary>
    /// 计算伤害值
    /// </summary>
    void CalcDamage(float dt)
    {
        MULT_FLAG = true;
        Array.Clear(dmgs, 0, dmgs.Length);
        for (int i = 0; i < battArray.Count; ++i )
        {
            if (battArray[i] == null)
                continue;


            /// 这里伤害公式, 飞船本身攻击和飞船人口值
            int count   = battArray[i].current;
            int index   = (int)(battArray[i].team.team);
            float dmg   = count * 1 * dt;
            
            if (dmg == 0)
                continue;

            dmgs[index] = dmgs[index] + dmg;
            if (count >= MULT_COUNT) MULT_FLAG = false;
        }

        //如果全部阵营的飞船，少于MUTL_COUNT,那么所有攻击力提升MULT_NUM倍
        if (MULT_FLAG)
        {
            for (int i = 1; i < dmgs.Length; ++i)
            {
                dmgs[i] *= MULT_NUM;
            }
        }
    }


    /// ---------------------------------------------------------------------------------------------------
    /// <summary>
    /// 对飞船造成伤害
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------
    void DamageToShip()
    {

        for (int i = 1; i < dmgs.Length; ++i)
        {
            float dmg = dmgs[i];
            if (dmg <= 0)
                continue;

            /// 伤害平摊到每个敌对队伍
            Team srcTeam    = nodeManager.sceneManager.teamManager.GetTeam((TEAM)i);
            int nAvgCount   = 0;
            for (int x = 1; x < dmgs.Length; x++ )
            {
                if (i == x) 
                    continue;

                if (dmgs[x] < 0)
                    continue;

                /// 星球上这个队伍没有伤害，也就说明星球上没有这个队伍
                Team dstTeam = nodeManager.sceneManager.teamManager.GetTeam((TEAM)x);
                if (dstTeam == null)
                    continue;

                if (!dstTeam.Valid())
                    continue;

                if (srcTeam.IsFriend(dstTeam.groupID))
                {
                    continue;
                }
                nAvgCount++;
            }

            if (nAvgCount == 0)
                continue;

            dmg = dmg / nAvgCount;
            for (int j = 1; j < dmgs.Length; ++j)
            {
                if (i == j)
                    continue;

                if (dmgs[j] <= 0)
                    continue;

                List<BattleMember> all = GetShips(j);
                if (all == null || all.Count <= 0)
                {
                    continue;
                }

                BattleMember ship = all[0];
                if (nodeManager.sceneManager.teamManager.GetTeam((TEAM)i).IsFriend(ship.currentTeam.groupID))
                {
                    continue;
                }

                for (int n = 0; n < all.Count; n++)
                {
                    if (dmg <= 0) 
                        break;

                    ship = all[n];
                    if (!ship.isALive)
                        continue;

                    int nHP = ship.GetAtt( ShipAttr.Hp );
                    if ( dmg >= nHP )
                    {
                        dmg              -= nHP;
                        nHP              = 0;
                        int oldPoplation = ship.GetAtt(ShipAttr.Population);
                        numArray[j]      -= oldPoplation;

                        //销毁飞船
                        ship.Bomb();
                        //记录摧毁数量
                        nodeManager.sceneManager.teamManager.AddDestory((TEAM)i);
                    }
                    else
                    {
                        int oldPoplation    = ship.GetAtt(ShipAttr.Population);
                        nHP                 -= (int)(dmg + 0.5f);
                        dmg                 = 0;
                        int nBaseHP         = ship.GetAtt(ShipAttr.BaseHp);
                        int nPoplation      = (nHP / nBaseHP) + (nHP % nBaseHP > 0 ? 1 : 0);
                        int nDiff           = oldPoplation - nPoplation;
                        numArray[j]         -= nDiff;
                        ship.SetAtt(ShipAttr.Population, nPoplation);
                        ship.SetAtt(ShipAttr.Hp, nHP);
                    }
                }
            }
        }
    }
}
