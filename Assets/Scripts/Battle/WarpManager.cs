using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;

public class WarpManager : Lifecycle2
{
	List<WarpItem> list = new List<WarpItem> ();

	public WarpManager()
	{
		
	}
		
	public bool Init()
	{
		
		return true;
	}

	public void Tick(int frame, float interval)
	{
		for (int i = 0; i < list.Count;) 
		{
			list [i].time -= interval;
			if (list [i].time <= 0) 
			{
				list [i].Warp ();
				list.RemoveAt (i);
			}
			else
				i ++;
		}	

	}

	public void Destroy()
	{
		list.Clear ();
	}

	public void AddWarpItem(Node from, Node to, TEAM team, bool warp)
	{
		WarpItem item   = new WarpItem ();
		item.from       = from;
		item.to         = to;
		item.team       = team;
		item.rate       = 1.0f;
		item.num        = 0;
		item.time       = 0.5f;
		item.bwarp      = warp;
		list.Add (item);
	}
}

public class WarpItem
{
	public Node     from;
	public Node     to;
	public TEAM     team;
	public float    rate = 0;
	public int      num = 0;
	public float    time;
	public bool     bwarp;

	public void Warp()
	{
		from.MoveTo (to, bwarp); 
	}
}

