using System;
using System.Collections.Generic;
using UnityEngine;
using Solarmax;






/// <summary>
/// AILogic接口
/// 所有的AILogic均实现此接口
/// </summary>
public abstract class BaseAILogic : Lifecycle2
{
	/// <summary>
	/// 场景管理
	/// </summary>
	/// <value>The scene manager.</value>
	public SceneManager             sceneManager { get; set; }

	public delegate bool            OnAIStatusCallback (Team t, float dt);
	protected OnAIStatusCallback[]  aiStatusCallbacks = new OnAIStatusCallback[(int)AIStatus.Unknow];
    public float                    actionTick = 1.0f;

	public BaseAILogic()
	{
		
	}

	public void RegisterCallback(AIStatus status, OnAIStatusCallback callback)
	{
		if (status == AIStatus.Unknow)
			return;

		aiStatusCallbacks [(int)status] = callback;
	}

	public bool Init()
	{
		return true;
	}

	public void Tick(int frame, float interval)
	{

	}

	public void Tick (Team t, int frame, float interval)
	{
        FriendSmartAIData ad       = t.aiData;
        ad.actionTime   += interval;
        if (ad.actionTime < actionTick)
			return;

        ad.actionTime   = 0.0f;
		aiStatusCallbacks[(int)AIStatus.Idle].Invoke(t, interval);
        aiStatusCallbacks[(int)AIStatus.Defend].Invoke(t, interval);
        aiStatusCallbacks[(int)AIStatus.Attack].Invoke(t, interval);
		aiStatusCallbacks[(int)AIStatus.Gather].Invoke(t, interval);
	}

	public void Destroy()
	{
		
	}

    public abstract void Init(Team t, AIType type, int level, int Difficulty);

	public abstract void Release(Team t);

	protected int ComparisonAIValue (Node arg0, Node arg1)
	{
		int ret = arg0.aiValue.CompareTo (arg1.aiValue);

		return ret == 0 ? arg0.tag.CompareTo(arg1.tag) : ret;
	}

	protected int ComparsionAIStrength (Node arg0, Node arg1)
	{
		return arg0.aiStrength.CompareTo(arg1.aiStrength);
	}

    /// <summary>
    /// 得到所有自己的Node 中心
    /// </summary>
    protected Vector3 TraversalAllNodeCenter (Team t)
	{
		Vector3 centerPos = Vector3.zero;
		int num = 0;

		// 访问所有星球
		List<Node> allNodes = sceneManager.nodeManager.GetUsefulNodeList();
		for (int i = 0; i < allNodes.Count; ++i)
		{
			var node = allNodes [i];
			if (!node.IsOurNode(t.team))
				continue;

			centerPos += node.GetPosition ();
			++num;
		}

		if (num > 0) {
			centerPos /= num;
		}

		return centerPos;
	}
}
