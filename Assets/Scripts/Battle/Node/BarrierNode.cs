using UnityEngine;
using System.Collections;

/// <summary>
/// 障碍物节点
/// </summary>
public class BarrierNode : Node
{

	/// <summary>
	/// 初始化
	/// </summary>
	public BarrierNode(string name) : base(name)
	{
        nodeType = NodeType.Barrier;
	}
}
