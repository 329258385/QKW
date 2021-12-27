using UnityEngine;
using System.Collections;






/// <summary>
/// 堡垒
/// </summary>
public class CastleNode : Node 
{

	public CastleNode(string name) : base(name)
	{
        nodeType = NodeType.Castle;
	}

	public override bool Init(GameObject go)
	{
		return base.Init(go);
	}

	public override void Tick (int frame, float interval)
	{
		base.Tick (frame, interval);

		//设置流程判断
		UpdateState (frame, interval);
		//设置占领流程
		UpdateOccupied (frame, interval);
		//战斗
		UpdateBattle (frame, interval);
		//生产飞船
		UpdateProduce (frame, interval);
		//攻击
		AttackToShip (frame, interval);
		//捕获
		UpdateCapturing (frame, interval);
	}

	public override void Destroy ()
	{
		base.Destroy ();
	}
}
