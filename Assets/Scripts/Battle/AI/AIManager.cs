using System;
using System.Collections.Generic;
using UnityEngine;
using Solarmax;







public enum AIType
{
	Simple,
	Smart,
	Friend,
	FriendSmart,
	Black,
	Unknow,
}

public enum AIStatus
{
	Idle,
	Defend,
	Attack,
	Gather,
	Rebuild,
	Unknow,
}

public class AIManager : Lifecycle2
{
	SceneManager        sceneMagr { get; set; }
	BaseAILogic[]       aiLogics = new BaseAILogic[(int)AIType.Unknow];
	private bool        start;

	public AIManager (SceneManager sceneManager)
	{
		sceneMagr                           = sceneManager;
		aiLogics[(int)AIType.FriendSmart] 	= new FriendSmartAILogic { sceneManager = sceneMagr };
	}


	public bool Init()
	{
		return true;
	}


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 队伍tickAI
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
	public void Tick(int frame, float interval)
	{
		if (!start)
			return;

		for (int i = 0; i < (int)TEAM.TeamMax; i++)
		{
			Team t = sceneMagr.teamManager.GetTeam ((TEAM)i);
			if (t.aiEnable)
			{
				aiLogics [(int)t.aiData.aiType].Tick (t, frame, interval);
			}

			//根据战队
			//if( t.aiEnable )
			//{
			//		foreach( var bt in t.battleArray )
			//      {
			//			if( bt != null )
			//				aiLogics[(int)t.aiData.aiType].Tick(t, frame, interval);
			//		}
			//}
		}
	}

	public void Destroy()
	{
		start = false;
		for (int i = 0; i < (int)TEAM.TeamMax; i++) 
		{
			Team t = sceneMagr.teamManager.GetTeam ((TEAM)i);

			if (!t.aiEnable)
				continue;

			aiLogics [(int)t.aiData.aiType].Release (t);
		}
	}


    public void AddAI( Team t, AIType type, int level, int Difficulty)
	{
		if (type == AIType.Unknow) 
		{
			return;
		}

		aiLogics [(int)type].Init (t, type, level, Difficulty );
	}

	public void Start(float countTie)
	{
		start           = true;
	}


    /// <summary>
    /// AI 队伍的名字
    /// </summary>
    public static string GetAIName(int userId)
	{
		System.Random rand      = new System.Random (userId);
		List<NameConfig> list   = NameConfigProvider.Instance.GetAllData ();
		int r                   = rand.Next(0, list.Count - 1);
		return list [r].name;
	}


    /// <summary>
    /// AI 队伍的头像
    /// </summary>
	public static string GetAIIcon(int userId)
	{
		//      #if SERVER
		//return string.Empty;
		//      #else
		//System.Random rand  = new System.Random (userId);
		//int r               = rand.Next (0, 10);
		//return SelectIconWindow.GetIcon (r);
		//      #endif
		return "";
	}
}

