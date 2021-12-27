using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;





/// <summary>
/// 飞船管理
/// </summary>
public class ShipManager : Lifecycle2
{

    /// <summary>
    /// 战斗单元池子
    /// </summary>
    protected Queue<BattleMember>   mFreeObjects;

    public List<BattleMember>       mBusyObjects;
	public SceneManager				sceneManager;

	List<BattleMember>[] flyMap { get; set; }
	
	/// <summary>
	/// 初始化
	/// </summary>
	public ShipManager(SceneManager smer)
	{
		sceneManager = smer;
        mFreeObjects = new Queue<BattleMember>();
        mBusyObjects = new List<BattleMember>();
		flyMap       = new List<BattleMember>[(int)TEAM.TeamMax] {null, null, null};
	}

	public bool Init()
	{
		return true;
	}

	public void Tick(int frame, float interval)
	{
		for (int i = mBusyObjects.Count - 1; i >= 0; --i) {
			BattleMember ship = mBusyObjects [i];
			ship.Tick (frame, interval);
		}
	}


    public BattleMember Alloc()
    {
		BattleMember ret = null;
        if (mFreeObjects.Count > 0)
        {
            ret = mFreeObjects.Dequeue();
        }
        else
        {
            ret = new BattleMember();
        }

        mBusyObjects.Add(ret);
        return ret;
    }


	public void Destroy()
	{
		for (int i = mBusyObjects.Count - 1; i >= 0; --i) {
			BattleMember ship = mBusyObjects[i];
            Recycle(ship);
        }
        mBusyObjects.Clear();

		ReleaseFly ();

		BattleMember[] array = mFreeObjects.ToArray ();
		for (int i = 0, max = array.Length; i < max; ++i) {
			BattleMember ship = array [i];
			ship.Destroy ();
		}

		// 清空池
		Clear();
	}


	/// <summary>
	/// 释放资源
	/// </summary>
	void ReleaseFly()
	{
		for (int i = 0; i < flyMap.Length; ++i) {
			flyMap [i] = null;
		}
	}

    public void Recycle(BattleMember ship )
    {

        /// 飛船特殊回收
        if (null != ship)
        {
            ship.OnRecycle();
        }
    }


    public void Clear()
    {
        mFreeObjects.Clear();
        mBusyObjects.Clear();
    }


	/// <summary>
	/// 获取移动中的战斗单元
	/// </summary>
	public List<BattleMember> GetFlyShip(TEAM team)
	{
		List<BattleMember> all = null;
		all = flyMap [(int)team];

		if (all == null) {
			all = new List<BattleMember> ();
			flyMap [(int)team] = all;
		}
		return all;
	}

	/// <summary>
	/// 添加飞行中的飞船
	/// </summary>
	public void AddFlyShip(BattleMember ship)
	{
		List<BattleMember> all = null;
		all = flyMap [(int)ship.team];

		if (all == null) {
			all = new List<BattleMember> ();
			flyMap [(int)ship.team] = all;
		}

		if (all.Contains (ship))
			return;

		all.Add (ship);
	}

	/// <summary>
	/// 移除飞行中的飞船
	/// </summary>
	public void RemoveFlyShip(BattleMember ship)
	{
		List<BattleMember> all = null;
		all = flyMap [(int)ship.team];

		if (all == null) {
			all = new List<BattleMember> ();
            flyMap [(int)ship.team] = all;
		}

		if (!all.Contains (ship))
			return;

		all.Remove (ship);
	}

	public void SilentMode (bool status)
	{
		for (int i = mBusyObjects.Count - 1; i >= 0; --i) {
			BattleMember ship = mBusyObjects [i];
			ship.entity.SilentMode (status);
		}
	}
}
