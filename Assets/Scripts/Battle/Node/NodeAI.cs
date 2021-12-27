using System;
using System.Collections.Generic;




public partial class Node
{
	/// <summary>
	/// 攻击时目标的代价：移动距离+攻击数据
	/// </summary>
	public float                aiValue;

	/// <summary>
	/// 攻击源的攻击力：己方在源上的数据（负值作为排序数据）
	/// </summary>
	public float                aiStrength;
	public float[]				aiTimers = new float[(int)TEAM.TeamMax];

	private List<Node>          oppLinks = new List<Node>();


    public void ResetAICalculateCache()
    {
        
    }


	// 星球上自己和队友的力量
	public int PredictedTeamStrength(TEAM t, bool useFriend = true )
	{
		Team team   = sceneManager.teamManager.GetTeam(t);
        int val     = GetShipCount((int)t);

		if( useFriend ) { 
			for (int i = 0; i < (int)TEAM.TeamMax; i++)
			{
				if (t == (TEAM)i)
					continue;

				Team temp = sceneManager.teamManager.GetTeam((TEAM)i);
				if (team.IsFriend(temp.groupID))
				{
					val += numArray[i];
				}
			}
		}
		return val;
	}


    //  星球上敌方 的力量
	public int PredictedOppStrength(TEAM t)
	{
		int val     = 0;
		Team team   = sceneManager.teamManager.GetTeam (t);
        for (int i = 0; i < (int)TEAM.TeamMax; i++)
        {
            if (t == (TEAM)i)
                continue;

            Team temp = sceneManager.teamManager.GetTeam((TEAM)i);
            if (!team.IsFriend(temp.groupID))
            {
                val += numArray[i];
            }
        }

        return val;
	}

	public int CalebeComingBattle(Team t, int nTargetStrength )
    {
		List<BattleTeam> tempList = new List<BattleTeam>();
		for(int n = 0; n < battArray.Count; n++ )
		{
			BattleTeam bt = battArray[n];
			if( bt != null && bt.team.team == t.team && !bt.IsHomeCity() )
            {
				int nPopulation = bt.Population();
				if (nPopulation >= nTargetStrength)
					tempList.Add(bt);
            }
        }
		if (tempList.Count <= 0)
			return -1;
		int nCount = tempList.Count;
		int nValue = BattleSystem.Instance.battleData.rand.Range(0, nCount);
		return tempList[nValue].ID;
    }


	public bool AICanLink(Node target, Team t)
	{
		bool ret = true;
        if (this.nodeType == NodeType.WarpDoor)
		{
			ret = this.CanWarp (t.team);
		} else {
			ret = ! nodeManager.sceneManager.GetIntersection (this.GetPosition (), target.GetPosition ());
		}

		return ret;
	}

	public int GetOppLinks(Team t, List<Node> allNodes)
	{
		oppLinks.Clear ();
		for (int i = 0; i < allNodes.Count; ++i) {
			Node n = allNodes [i];
			if (n == this)
				continue;
			if (n.team == TEAM.Neutral || n.team != team || n.PredictedOppStrength (t.team) > 0) {
				if (AICanLink (n, t))
					oppLinks.Add (n);
			}
		}

		return oppLinks.Count;
	}

	public void ResetAiTimer( TEAM t, float interval )
    {
		aiTimers[(int)t] = interval;
    }


	protected void UpdateAiTimers( int frame, float dt )
    {
		for (int i = 0; i < aiTimers.Length; ++i)
		{
			if (aiTimers[i] > 0)
			{
				aiTimers[i] -= dt;
				if (aiTimers[i] < 0)
					aiTimers[i] = 0;
			}
		}
	}
	
	public virtual bool CanBeTarget()
	{
        if (nodeType == NodeType.Barrier || nodeType == NodeType.BarrierLine)
			return false;

		return true;
	}


    public virtual bool CanSelectNode()
    {
        int nBattleTeamNum = battArray.Count;
        for (int i = 0; i < nBattleTeamNum; i++)
        {
            if (battArray[i] == null)
                continue;

            if (BattleSystem.Instance.battleData.currentTeam == battArray[i].team.team)
                return true;
        }
        return false;
    }
}

