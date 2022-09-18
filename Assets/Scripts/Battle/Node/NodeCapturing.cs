using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;





/// <summary>
/// 捕获星球
/// </summary>
public partial class Node
{

	/// <summary>
	/// 捕获
	/// </summary>
	/// <param name="frame">Frame.</param>
	/// <param name="dt">Dt.</param>
	protected void UpdateCapturing(int frame, float dt)
	{
		if (state != NodeState.Capturing)
			return;

        float rate       = CalcOccupiedRate (capturingTeam);
		Team team        = nodeManager.sceneManager.teamManager.GetTeam (capturingTeam);
		rate            *= CaleCapturedSpeed(team); // 主动方的加成
		rate            *= CaleBeCapturedSpeed(currentTeam);	// 被动方的减弱

		hp              -= rate * dt;
		if (hp <= 0) 
		{
			//考虑到组队情况在UpdateState中会存在Team（1 ～ 6）的先后循环，预设占领队伍就是捕获队伍
			occupiedTeam    = capturingTeam;
			capturingTeam   = TEAM.Neutral;
			hp              = 0;

            BattleTeam bt           = GetCapturedBattleTeam(team);
            // 设置为空白
            Team neutral            = nodeManager.sceneManager.teamManager.GetTeam (TEAM.Neutral);
            
            SetRealTeam(neutral, false, bt);
            #if !SERVER
            // 音效
            mCityHUD.gameObject.SetActive(false);
            AudioManger.Get().PlayCapture(GetPosition());
			#endif
		}

        CapturingHUD();

    }

    float CaleCapturedSpeed( Team team )
    {
        float rate = 1.0f;
        for (int i = 0; i < battArray.Count; i++ )
        {
            if (battArray[i].team.team == team.team)
            {
                rate *= battArray[i].GetAttribute(TeamAttr.CapturedSpeed);
                break;
            }
        }
        return rate;
    }


    float CaleBeCapturedSpeed( Team team )
    {
        float rate = 1.0f;
        for (int i = 0; i < battArray.Count; i++ )
        {
            if (battArray[i].team.team == team.team)
            {
                rate *= battArray[i].GetAttribute(TeamAttr.BeCapturedSpeed);
                break;
            }
        }
        return rate;
    }


    /// <summary>
    /// 得到占领星球的战斗，根队伍规则一样，默认是第一个来到这个星球的战队
    /// </summary>
    private BattleTeam GetCapturedBattleTeam(Team team)
    {
        if(team != null )
        {
            return team.battleArray[0];
        }
        return null;
    }


    void CapturingHUD()
    {
        #if !SERVER

        m_HPArray.Clear();
        m_teamArray.Clear();


        Team team = nodeManager.sceneManager.teamManager.GetTeam(capturingTeam);
        m_teamArray.Add(team);
        m_HPArray.Add(hp);
        
        {
            mCityHUD.ShowCity(HUDCityOperater.UIPanel.Battle);
            mCityHUD.ShowPopulationProcess(m_teamArray, m_HPArray);
        }
        #endif
    }
}
