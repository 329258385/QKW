using UnityEngine;
using System.Collections;








/// <summary>
/// 主基地
/// </summary>
public class MasterNode : Node 
{

	public MasterNode(string name) : base(name)
	{
        //nodeType = NodeType.Master;
	}

	public override bool Init(GameObject go)
	{
		return base.Init(go);
	}

	public override void Tick (int frame, float interval)
	{
		base.Tick (frame, interval);

		//设置环绕
		//UpdateOrbit (frame, interval);
		//设置流程判断
		UpdateState (frame, interval);
		//捕获
		UpdateCapturing (frame, interval);
		//设置占领流程
		UpdateOccupied (frame, interval);
		//战斗
		UpdateBattle (frame, interval);
		//胜负
		UpdateLost (frame, interval);
	}

	public override void Destroy ()
	{
		base.Destroy ();
	}
}
