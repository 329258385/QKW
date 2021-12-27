using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Plugin;

/// <summary>
/// 监听
/// </summary>
public abstract class Listenner
{
	/// <summary>
	/// 释放事件委托
	/// </summary>
	protected List<RunLockStepEvent> releaseHandler;
	//protected RunLockStepEvent releaseHandler;

	/// <summary>
	/// 心跳事件
	/// </summary>
	protected List<RunLockStepLogic> updateHandler;

	/// <summary>
	/// 初始化
	/// </summary>
	public Listenner()
	{
		releaseHandler = new List<RunLockStepEvent> ();
		updateHandler = new List<RunLockStepLogic> ();
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	void Release()
	{

	}

	/// <summary>
	/// 释放事件
	/// </summary>
	/// <param name="handler">Handler.</param>
	protected void AddListenner(RunLockStepEvent handler)
	{
		releaseHandler.Add (handler);
		//this.releaseHandler = this.releaseHandler == null ? new RunLockStepEvent(handler) : this.releaseHandler + new RunLockStepEvent(handler);
	}

	/// <summary>
	/// 移除释放资源事件
	/// </summary>
	/// <param name="handler">Handler.</param>
	protected void RemoveListenner(RunLockStepEvent handler)
	{
		releaseHandler.Remove (handler);
		//this.releaseHandler -= handler;
	}

	/// <summary>
	/// 执行事件
	/// </summary>
	protected void InvokeReleaseEvent()
	{
		for (int i = 0; i < releaseHandler.Count; i++) {
			releaseHandler [i].Invoke ();
		}
//		if (releaseHandler != null)
//			releaseHandler.Invoke ();
	}

	/// <summary>
	/// 心跳事件
	/// </summary>
	/// <param name="handler">Handler.</param>
	protected void AddListenner(RunLockStepLogic handler)
	{
		updateHandler.Add (handler);
		//this.updateHandler = this.updateHandler == null ? handler : this.updateHandler + handler;
	}

	/// <summary>
	/// 移除心跳事件
	/// </summary>
	/// <param name="handler">Handler.</param>
	protected void RemoveListenner(RunLockStepLogic handler)
	{
		updateHandler.Remove (handler);
		//this.updateHandler -= handler;
	}

	/// <summary>
	/// 执行心跳事件
	/// </summary>
	/// <param name="frame">Frame.</param>
	/// <param name="dt">Dt.</param>
	protected void InvokeLogicEvent(int frame, float dt)
	{
		for (int i = 0; i < updateHandler.Count; ++i) {
			updateHandler [i] (frame, dt);
		}
//		if (updateHandler != null)
//			updateHandler.Invoke (frame, dt);
	}
}