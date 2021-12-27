using UnityEngine;
using System.Collections;

/// <summary>
/// 障碍物连线
/// </summary>
public class BarrierLineNode : Node 
{

	/// <summary>
	/// 初始化
	/// </summary>
	public BarrierLineNode(string name) : base (name)
	{
        nodeType = NodeType.BarrierLine;
	}
}
