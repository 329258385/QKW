using System;
using Solarmax;






/// <summary>
/// 简略玩家数据，用于各种功能的
/// 其中level及以上都是必选字段，即网络包一定会传过来。level以下为非必选，需要和功能制作人员确定是否已传递。
/// </summary>
public class SimplePlayerData
{
	/// 以下为必选字段
	public int          userId;
	public string       name;
	public string       icon;
	public int          score;
	public int          level;

	public void Init(NetMessage.UserData sud)
	{
		// 必选字段的required
		userId      = sud.userid;
		name        = sud.name;
		icon        = sud.icon;
		score       = sud.score;
		level       = sud.level;
	}
}



/// <summary>
/// 战舰外观对象
/// </summary>
public class Simpleheroconfig
{
    public Int64                userID;
    public int                  heroID;

	/// <summary>
	/// 飞船属性
	/// </summary>
	public int[]                attribute = new int[(int)HeroAttr.MAX];
	public void InitAttr( HeroConfig config, bool bFromNet = false )
    {
		attribute[(int)HeroAttr.Population]		= 30;
		attribute[(int)HeroAttr.PopulationMax]	= 30;
		attribute[(int)HeroAttr.Hp]				= config.maxHp;
		attribute[(int)HeroAttr.MaxHp]			= config.maxHp;
		attribute[(int)HeroAttr.Armor]			= config.Arms;
		attribute[(int)HeroAttr.Speed]			= config.speed;
		attribute[(int)HeroAttr.AttackSpeed]	= config.attackspeed;
		attribute[(int)HeroAttr.AttackPower]	= config.damage;
		attribute[(int)HeroAttr.AttackRange]	= 1;
		attribute[(int)HeroAttr.WarningRange]	= 1;
	}
}


/// <summary>
/// 玩家数据
/// </summary>
public class NetPlayer
{
	/// <summary>
	/// 当前组队
	/// </summary>
	/// <value>The current team.</value>
	public Team             currentTeam { get; set; }

	/// <summary>
	/// The user identifier.
	/// </summary>
	public int              userId;

	/// <summary>
	/// The name.
	/// </summary>
	public string           name;

	/// <summary>
	/// The icon.
	/// </summary>
	public string           icon;

	/// <summary>
	/// The score.
	/// </summary>
	public int              score;

	/// <summary>
	/// The level.
	/// </summary>
	public int              level;

    /// <summary>
    /// 体力
    /// </summary>
	public int              power;	//体力值

	public NetPlayer()
	{
		power = 0;
	}

	/// <summary>
	/// 初始化
	/// </summary>
	/// <param name="data">Data.</param>
	public void Init(NetMessage.UserData data)
	{
		userId      = data.userid;
		name        = data.name;
		icon        = data.icon;
		score       = data.score;
		level       = data.level;
	}

    public void Init(string strIcom )
    {
        userId      = 10000;
        name        = "lijiangpo";
        icon        = strIcom;
        score       = 0;
        level       = 0;
    }

	public void Init(NetPlayer player )
	{
		userId      = player.userId;
		name        = player.name;
		icon        = player.icon;
		score       = player.score;
		level       = player.level;
	}


	/// <summary>
	/// Reset this instance.
	/// </summary>
	public void Reset()
	{
		userId          = -1;
		name            = string.Empty;
		icon            = string.Empty;
		score           = 0;
		level           = 0;
	}


	public void UpdateFromNetMsg(NetMessage.SCIntAttr msg)
	{
		if (msg.attr.Count == msg.value.Count ) 
		{
			for (int i = 0; i < msg.attr.Count; i++) 
			{
				var attr        = msg.attr [i];
				var value       = msg.value [i];
				if (attr == NetMessage.IntAttr.IA_Power) 
				{
					power = value;
				}
			}
		}
	}
}





