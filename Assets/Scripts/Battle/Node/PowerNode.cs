﻿using UnityEngine;
using System.Collections;

/// <summary>
/// 资源星球
/// </summary>
public class PowerNode : Node 
{
	public PowerNode(string name) : base(name)
	{
        nodeType = NodeType.Power;
	}

	public override bool Init ( GameObject go )
	{
		return base.Init ( go );
	}

	public override void Tick (int frame, float interval)
	{
		base.Tick (frame, interval);
		//设置流程判断
		UpdateState (frame, interval);
		//捕获
		UpdateCapturing (frame, interval);
		//设置占领流程
		UpdateOccupied (frame, interval);
		//战斗
		UpdateBattle (frame, interval);
	}

	public override void Destroy ()
	{
		base.Destroy ();
	}
}
