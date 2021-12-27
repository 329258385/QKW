using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 计算node当前状态
/// </summary>
public partial class Node 
{
	/// <summary>
	/// 占领队伍相关属性
	/// </summary>
	/// <value>The occupied team.</value>
	public TEAM				occupiedTeam		= TEAM.Neutral;

	/// <summary>
	/// 捕获队伍
	/// </summary>
	public TEAM				capturingTeam = TEAM.Neutral;

	/// <summary>
	/// 临时队伍
	/// </summary>
	TEAM					temp = TEAM.Neutral;

	/// <summary>
	/// 状态进度UI
	/// </summary>
	private HUDCityOperater mCityHUD = null;

	/// <summary>
	/// 计算当前星球状态
	/// </summary>
	protected void UpdateState(int frame, float dt)
	{
		temp = TEAM.Neutral;
		for (int i = 0; i < (int)TEAM.TeamMax; i++) {

			if (GetShipCount(i) == 0)
				continue;

			//如有2方以上的话，确认是否进入战争状态
			if (temp != TEAM.Neutral) 
			{
				//队友跳过
				if (nodeManager.sceneManager.teamManager.GetTeam (temp).IsFriend (nodeManager.sceneManager.teamManager.GetTeam ((TEAM)i).groupID))
					continue;
				else
				{
					EnterBattleByTeam(NodeState.Battle, temp );
					state = NodeState.Battle;
					return;
				}
			}
			temp = (TEAM)i;
		}


		switch(team)
		{
		case TEAM.Neutral:	//当前星球为中立状态
			{
				//无人占领
				if (temp == TEAM.Neutral)
				{
					state = NodeState.Idle;
					break;
				}

				if (temp == occupiedTeam || occupiedTeam == TEAM.Neutral ) 
				{
					//进入占领流程，继承上次占领进度
					EnterEncircleCityByTeam(NodeState.Occupied);
					state = NodeState.Occupied;
					occupiedTeam = temp;
				}
                else
                {
                    //不是上次占领的队伍
                    //如果是上次占领的队友，继续占领
                    if (nodeManager.sceneManager.teamManager.GetTeam(temp).IsFriend(nodeManager.sceneManager.teamManager.GetTeam(occupiedTeam).groupID))
                    {
                        //加一次核查上次占领方是否离开星球
                        if (GetShipCount((int)occupiedTeam) == 0)
                        {
                            occupiedTeam = temp;
                        }
						EnterEncircleCityByTeam(NodeState.Occupied);
						state = NodeState.Occupied;
					}
                    else //2如果不是上次占领的队友，捕捉
                    {
                        //确认是否进入捕获状态
                        capturingTeam = temp;
						EnterEncircleCityByTeam(NodeState.Capturing);
						state = NodeState.Capturing;
					}
                }
                break;
			}
		default://当前星球被占领
			{
				if (temp == TEAM.Neutral) 
                {
					state = NodeState.Idle;
					break;
				}

				if (temp == team || 
					nodeManager.sceneManager.teamManager.GetTeam (team).IsFriend( nodeManager.sceneManager.teamManager.GetTeam (temp).groupID))
				{
					//如果当前星球和当前队伍同一阵营或者是友方的话
					if (hp >= hpMax) 
                    {
						state			= NodeState.Idle;
						BattleTeamEnterCity(team);
					} 
                    else 
                    {
						occupiedTeam = team;
						EnterEncircleCityByTeam(NodeState.Occupied);
						state = NodeState.Occupied;
					}
				} 
				else 
				{
					//如果当前星球和当前队伍是组队状态，保持原状态
					if (nodeManager.sceneManager.teamManager.GetTeam (capturingTeam).IsFriend (nodeManager.sceneManager.teamManager.GetTeam (temp).groupID))
					{
						EnterEncircleCityByTeam(NodeState.Capturing);
						state			= NodeState.Capturing;
					}
					else
					{
						EnterEncircleCityByTeam(NodeState.Capturing);
						capturingTeam	= temp;
						state			= NodeState.Capturing;
					}
				}
				break;
			}
		}

		if( state == NodeState.Idle )
        {
			UpdateCityHUD();
		}
	}

	
	public void UpdateCityHUD()
	{
		if (battArray.Count <= 0)
		{
			if (mCityHUD != null && mCityHUD.gameObject.activeSelf)
				mCityHUD.gameObject.SetActive(false);
			return;
		}

		if (state == NodeState.Battle)
			return;
		#if !SERVER
		if(mCityHUD == null )
        {
			CreateHUD();
			mCityHUD.SetNode(this);
        }
		
		if(!mCityHUD.gameObject.activeSelf  )
        {
			mCityHUD.gameObject.SetActive(true);
			mCityHUD.ShowCity(HUDCityOperater.UIPanel.Flag, HUDCityOperater.UIBattle.Flag);
		}
		#endif
	}

	private void EnterBattleByTeam( NodeState newState, TEAM atkTeam = TEAM.Neutral )
    {
		foreach (var battle in battArray)
		{
			if( battle.btState != BattleTeamState.Battle )
				battle.EnterBattleStats( this, atkTeam );
		}
	}


	/// <summary>
	/// 检测战斗状态变化
	/// </summary>
	private void EnterEncircleCityByTeam( NodeState newState )
    {
		if (state == newState)
			return;

		// 转变战队围城，为什么是所有的战队，这位这时候敌人的战队应该没有了
		if( newState == NodeState.Capturing || newState == NodeState.Occupied )
        {
			foreach (var battle in battArray)
			{
				battle.EntersEncircleCity();
			}
		}
    }
}
