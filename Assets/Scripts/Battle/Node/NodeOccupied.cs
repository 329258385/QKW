using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;






/// <summary>
/// 占领状态
/// </summary>
public partial class Node
{
	/// <summary>
	/// 占领流程
	/// </summary>
	/// <param name="frame">Frame.</param>
	/// <param name="dt">Dt.</param>
	protected void UpdateOccupied(int frame, float dt)
	{
		if (state != NodeState.Occupied)
			return;

        if (occupiedTeam == TEAM.Neutral)
            return;

        //计算占领比例
        float occupiedRate  = CalcOccupiedRate(occupiedTeam);
		Team team           = nodeManager.sceneManager.teamManager.GetTeam (occupiedTeam);
        occupiedRate        *= CaleOccupiedSpeed(team);
		
		hp                  += (occupiedRate * dt * 0.25f );
		if (hp > hpMax) {
			hp              = hpMax;
            Team t          = nodeManager.sceneManager.teamManager.GetTeam(occupiedTeam);
            BattleTeam bt   = GetOccupiedBattleTeam(occupiedTeam);
            SetRealTeam(t, false, bt);
 
            occupiedTeam    = TEAM.Neutral;
            capturingTeam   = TEAM.Neutral;
            BattleTeamEnterCity(t.team);

            // 音效
            if(mCityHUD != null )
                mCityHUD.gameObject.SetActive(false);
            EventSystem.Instance.FireEvent(EventId.OnOccupiedNode, this);
            AudioManger.Get().PlayCapture(GetPosition());
        }

        OccupiedHUD();
    }


    /// <summary>
    /// 得到占领星球的战斗，根队伍规则一样，默认是第一个来到这个星球的战队
    /// </summary>
    private BattleTeam GetOccupiedBattleTeam( TEAM team )
    {
        for( int i = 0; i < battArray.Count; i++ )
        {
            if (battArray[i].team.team == team)
                return battArray[i];
        }

        return null;
    }

	/// <summary>
	/// 计算占领速度
	/// </summary>
	protected float CalcOccupiedRate(TEAM team)
	{
		int count       = numArray[(int)team];
		Team t          = nodeManager.sceneManager.teamManager.GetTeam (team);
		int teamGrounp  = t.groupID;
		for (int i = 1; i < (int)TEAM.TeamMax; i++)
		{
			if (i == (int)team)
				continue;
			if (nodeManager.sceneManager.teamManager.GetTeam ((TEAM)i).IsFriend(teamGrounp))
				count += numArray [i];
		}

		// 新算法
		//float rate  = (-5000 / (count + 100) + 50) / (3*GetScale());
        float rate = (-5000 / (count + 100) + 50) / (3 * GetScale());
        return rate > 100f ? 100f : rate;
	}


    float CaleOccupiedSpeed(Team team)
    {
        float rate = 1.0f;
        for (int i = 0; i < battArray.Count; i++)
        {
            if (battArray[i].team.team == team.team)
            {
                rate *= battArray[i].GetAttribute(TeamAttr.OccupiedSpeed);
                break;
            }
        }
        return rate;
    }

    public void BattleTeamEnterCity(TEAM team)
    {
        foreach (var battle in battArray)
        {
            if (battle.team.team == team && battle.btState != BattleTeamState.EnterCity && battle.btState != BattleTeamState.Idle )
            {
                battle.EnterCity( this );
            }
        }
    }

    public void OccupiedHUD()
    {
        #if !SERVER

        //未占领完就离开
        m_HPArray.Clear();
		m_teamArray.Clear ();
        if (mCityHUD == null)
        {
            CreateHUD();
            mCityHUD.SetNode(this);
        }

        Team team   = nodeManager.sceneManager.teamManager.GetTeam(occupiedTeam);
        m_HPArray.Add(hp);
        m_teamArray.Add(team);
        if ( mCityHUD.gameObject.activeSelf )
        {
            mCityHUD.ShowPopulationProcess(m_teamArray, m_HPArray);
        }
        else
        {
            mCityHUD.gameObject.SetActive(true);
            mCityHUD.ShowCity(HUDCityOperater.UIPanel.Battle, HUDCityOperater.UIBattle.Flag);
        }
        #endif
    }
}
