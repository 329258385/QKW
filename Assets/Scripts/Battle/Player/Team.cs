using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Solarmax;





public class Team 
{
	/// <summary>
	/// team管理器 
	/// </summary>
	/// <value>The manager.</value>
	public TeamManager      teamManager { get; set; }

	/// <summary>
	/// 队伍的玩家数据
	/// </summary>
	/// <value>The player data.</value>
	public NetPlayer        playerData { get; set; }

	/// <summary>
	/// 队伍对应的ai数据
	/// </summary>
	/// <value>The ai data.</value>
	public FriendSmartAIData aiData { get; set; }

	/// <summary>
	/// ai是否启动
	/// </summary>
	public bool             aiEnable = false;

	/// <summary>
	/// 当前阵营颜色
	/// </summary>
	public Color			color  { get; set; }
	public Color            color1 { get; set; }
	public Color			color2 { get; set; }
	public Color			color3 { get; set; }
	public Color			color4 { get; set; }


	public string           iconname { get; set; }

	/// <summary>
	/// 当前队伍
	/// </summary>
	/// <value>The team.</value>
	public TEAM             team { get; set;}

	
	/// <summary>
	/// 摧毁的飞船数量
	/// </summary>
	/// <value>The destory.</value>
	public int              destory { get; set; }

	/// <summary>
	/// 组队ID
	/// </summary>
	/// <value>The destory.</value>
	public int              groupID { get; set; }

	/// <summary>
	/// 结果队伍积分
	/// </summary>
	/// <value>The score mod.</value>
	public int              scoreMod { get; set; }

	/// <summary>
	/// 结果序列，从-3， -2， -1， 0。默认为0，所以排序按照resultorder，scoremode，destroy依次排序
	/// </summary>
	/// <value>The result order.</value>
	public int              resultOrder { get; set; }

	/// <summary>
	/// 结果排名
	/// </summary>
	public int					resultRank { get; set; }

    /// <summary>
	/// 结果类型
	/// </summary>
	public NetMessage.EndType	resultEndtype { get; set; }

	/// <summary>
	/// 联赛mvp
	/// </summary>
	/// <value>The league mvp.</value>
	public int					leagueMvp { get; set; }

	/// <summary>
	/// 是否已经死亡了，(单机标记投降和死亡都是End，pvp死亡才是End）
	/// </summary>
	public bool					isEnd { get; set; }


    /// <summary>
    /// 战队ID 列表
    /// </summary>
    public List<BattleTeam>		battleArray = new List<BattleTeam>();

	/// <summary>
	/// 初始化
	/// </summary>
	public Team()
	{
		playerData              = new NetPlayer();
		playerData.Reset ();
		playerData.currentTeam  = this;
		Clear ();
	}

	/// <summary>
	/// 清理数据
	/// </summary>
	public void Clear()
	{

		playerData.Reset ();
		destory             = 0;
		scoreMod            = 0;
		resultOrder         = 0;
		resultRank          = 0;
		resultEndtype       = NetMessage.EndType.ET_Dead;
		leagueMvp           = 0;
		isEnd               = false;
		groupID             = -1;
		aiEnable            = false;
        battleArray.Clear();
	}

	public void StartTeam()
	{

	}


	public bool IsFriend(int group)
	{
		if (groupID == -1)
			return false;
		if (group == -1)
			return false;
		if (groupID != group)
			return false;
		return true;
	}


	/// <summary>
	/// 判断当前队伍是否有效，是否为有数值的队伍
	/// </summary>
	public bool Valid()
	{
		return playerData.userId != -1;
	}


    //---------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 队伍心跳
    /// </summary>
    //---------------------------------------------------------------------------------------------------------
    public void Tick(int frame, float interval)
    {
        for (int i = 0; i < battleArray.Count; i++)
        {
            battleArray[i].Tick(frame, interval);
        }
    }


	/// <summary>
	/// 判断队伍是否死亡
	/// 条件：当前人口为0，当前无拥有星球
	/// </summary>
	public bool CheckDead()
	{
        bool isDead = true;
        for (int i = 0; i < battleArray.Count; i++ )
        {
            if( battleArray[i].current > 0 )
                isDead = false;
        }

        if (teamManager.sceneManager.nodeManager.CheckHaveNode((int)team))
            return false;

        return isDead;
	}

	
    public void AddBattleTeam( BattleTeam bt )
    {
        battleArray.Add(bt);
    }

	/// <summary>
	/// 改变某个战队内所有成员状态
	/// </summary>
	public void ChangeShipsStats(MemberState state, int battleID )
	{
		for (int i = 0; i < battleArray.Count; i++)
		{
			if( battleArray[i].ID == battleID)
				battleArray[i].ChangeShipsStats(state);
		}
	}
}
