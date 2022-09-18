using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;



/// <summary>
/// 队伍基本信息管理
/// </summary>
public class TeamManager : Lifecycle2
{
	static Team[] STATIC_TEAM = new Team[(int)TEAM.TeamMax]{ 
		new Team { 
			color  = new Color32(0xff, 0xff, 0xff, 0xff), 
			color1 = new Color32(0xff, 0xff, 0xff, 0xff),
			color2 = new Color32(0xff, 0xff, 0xff, 0xff),
			color3 = new Color32(0xff, 0xff, 0xff, 0xff),
			color4 = new Color32(0xff, 0xff, 0xff, 0xff),
			team = TEAM.Neutral,
			iconname = "avatar_bg_normal",
		},

		new Team { 
			color  = new Color32(0x5f, 0xb6, 0xff, 0xff),
			color1 = new Color32(0x00, 0x1e, 0x9f, 0x00),
			color2 = new Color32(0x00, 0x26, 0xff, 0x00),
			color3 = new Color32(0x00, 0x3e, 0xd1, 0x00),
			color4 = new Color32(0x29, 0x04, 0x9f, 0x00),
			team = TEAM.Team_1,
			iconname = "avatar_bg_faction_blue",
		},

		new Team {
			color  = new Color32(0x5f, 0xb6, 0xff, 0xff),
			color1 = new Color32(0x9f, 0x09, 0x00, 0x00),
			color2 = new Color32(0xff, 0x1a, 0x00, 0x00),
			color3 = new Color32(0xff, 0x1c, 0x00, 0x00),
			color4 = new Color32(0x9f, 0x0f, 0x00, 0x00),
			team   = TEAM.Team_2,
			iconname = "avatar_bg_faction_red",
		},
	};

	/// <summary>
	/// 队伍管理
	/// </summary>
	/// <value>The scene manager.</value>
	public SceneManager     sceneManager { get; set; }


	/// <summary>
	/// 队伍信息
	/// </summary>
    public Team[]           teamArray = new Team[STATIC_TEAM.Length];


	public TeamManager(SceneManager mgr)
	{
		sceneManager = mgr;
	}
		
	public bool Init()
	{
		for (int i = 0; i < teamArray.Length; i++)
		{
			teamArray[i] = new Team
			{
				color       = STATIC_TEAM[i].color,
				color1		= STATIC_TEAM[i].color1,
				color2		= STATIC_TEAM[i].color2,
				color3		= STATIC_TEAM[i].color3,
				color4		= STATIC_TEAM[i].color4,
				team        = STATIC_TEAM[i].team,
				iconname    = STATIC_TEAM[i].iconname,
				teamManager = this
			};
		}

		Release ();
		return true;
	}

	public void Tick(int frame, float interval)
	{
		for (int i = 0; i < teamArray.Length; i++)
		{
			teamArray[i].Tick(frame, interval);
		}
	}

	public void Destroy()
	{
		Release ();
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	public void Release()
	{
		foreach (Team team in teamArray) {
			team.Clear ();
		}
	}

	/// <summary>
	/// 获取team
	/// </summary>
	/// <returns>The team.</returns>
	/// <param name="team">Team.</param>
	public Team GetTeam(TEAM team)
	{
		return teamArray [(int)team];
	}


	public Team GetTeamByID(int camption )
	{
		for (int i = 0; i < teamArray.Length; i++)
		{
			if (teamArray[i].team == (TEAM)camption )
				continue;
			return teamArray[i];
		}

		return null;
	}


	public Team GetTeam(int userId)
	{
		for (int i = 0; i < teamArray.Length; i++) {

			if (teamArray [i].playerData.userId != userId)
				continue;
			return teamArray [i];
		}

		return null;
	}


	/// <summary>
	/// 增加摧毁数量
	/// </summary>
	public void AddDestory(TEAM team)
	{
		Team t = GetTeam (team);

		if (t == null)
			return;

		t.destory++;
	}
}
