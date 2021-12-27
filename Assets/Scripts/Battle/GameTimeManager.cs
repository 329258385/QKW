using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 声音类型
/// </summary>
public enum TimerType
{
	T_Bomber,
	T_Maker,
	T_JumpCharge,
	T_JumpStart,
	T_JumpEnd,
	T_Capture,
	T_Laser,
	T_WarpCharge,
	T_Warp,
	T_Defense,

	Timer_Max,
}

/// <summary>
/// 计时器
/// </summary>
public class Timer
{
	/// <summary>
	/// 计时器
	/// </summary>
	public float timer;

	/// <summary>
	/// 时间限制
	/// </summary>
	public float timeMax;

	public Timer (float max)
	{
		timer = Time.realtimeSinceStartup;
		timeMax = max;
	}

	/// <summary>
	/// 检测是否符合时间间隔需求
	/// </summary>
	/// <returns><c>true</c>, if timer was checked, <c>false</c> otherwise.</returns>
	public bool CheckTimer()
	{
		if (timer == 0)
			timer = Time.realtimeSinceStartup;

		if ((Time.realtimeSinceStartup - timer) > timeMax) 
		{
			timer = Time.realtimeSinceStartup;
			return true;
		}

		return false;
	}
}

/// <summary>
/// 时间管理
/// 特效，声音播放间隔时间管理
/// </summary>
public class GameTimeManager : Singleton<GameTimeManager> 
{
	/// <summary>
	/// 计时器数组
	/// </summary>
	Timer[] timerArray = new Timer[(int)(TimerType.Timer_Max)];

	/// <summary>
	/// 初始化
	/// </summary>
	public void Init()
	{
		RegeditTimer (TimerType.T_Bomber, 		new Timer(0.1f));
		RegeditTimer (TimerType.T_Maker,  		new Timer(0.1f));
		RegeditTimer (TimerType.T_JumpCharge,  	new Timer(0.1f));
		RegeditTimer (TimerType.T_JumpStart,  	new Timer(0.1f));
		RegeditTimer (TimerType.T_JumpEnd,  	new Timer(0.1f));
		RegeditTimer (TimerType.T_Capture,  	new Timer(0.1f));
		RegeditTimer (TimerType.T_Laser,  		new Timer(0.1f));
		RegeditTimer (TimerType.T_WarpCharge,  	new Timer(0.1f));
		RegeditTimer (TimerType.T_Warp,  		new Timer(0.1f));
		RegeditTimer (TimerType.T_Defense,  	new Timer(0.1f));
	}

	/// <summary>
	/// 销毁计时器
	/// </summary>
	public void Release()
	{
	}

	/// <summary>
	/// 注册计时器
	/// </summary>
	/// <param name="type">Type.</param>
	/// <param name="timer">Timer.</param>
	void RegeditTimer(TimerType type, Timer timer)
	{
		timerArray [(int)type] = timer;
	}

	/// <summary>
	/// 检测计时器是否超时
	/// </summary>
	/// <returns><c>true</c>, if timer was checked, <c>false</c> otherwise.</returns>
	/// <param name="type">Type.</param>
	public bool CheckTimer(TimerType type)
	{ 
		Timer timer = timerArray [(int)type];

		return timer == null ? false : timer.CheckTimer();
	}
}
