using UnityEngine;
using System.Collections;
using System.Collections.Generic;




/// <summary>
/// 节点主城灭亡模块
/// </summary>
public partial class Node
{
	/// <summary>
	/// 原始队伍，队伍变更则失败
	/// </summary>
	public TEAM initTEAM;
	/// <summary>
	/// 主城被灭，战斗失败
	/// </summary>
	/// <param name="dt">Dt.</param>
	protected void UpdateLost(int frame, float dt)
	{
		if (initTEAM == TEAM.Neutral)
			return;
		if (state != NodeState.Idle)
			return;
		if (team == initTEAM)
			return;

		if (team == TEAM.Neutral)
			return;

		DestroyTeam (initTEAM);

		initTEAM = TEAM.Neutral;

		// 直接失败
		// nodeManager.sceneManager.battleManager.NotifyHomeLose (team, initTEAM);
	}

	void DestroyTeam(TEAM dt)
	{
		nodeManager.sceneManager.DestroyTeam (dt);
	}
}
