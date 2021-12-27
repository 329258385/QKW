using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Plugin;





public interface IPoolNode
{
	void Recovery ();
	void Release ();
	void SetPool (ObjectPool pool);
	bool IsActive();
	void Update (int frame, float dt);
}

/// <summary>
/// 数据池
/// </summary>
public abstract class ObjectPool : Listenner
{
	/// <summary>
	/// 未使用池
	/// </summary>
	protected Queue<IPoolNode> pool { get; set; }

	/// <summary>
	/// 已经在使用
	/// </summary>
	protected List<IPoolNode> livePool { get; set; }

	/// <summary>
	/// 初始化
	/// </summary>
	public ObjectPool()
	{
		pool = new Queue<IPoolNode> ();
		livePool = new List<IPoolNode> ();
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	public virtual void Release()
	{
		IPoolNode[] nodes = livePool.ToArray ();
		for (int i = 0; i < nodes.Length; ++i) {
			IPoolNode node = nodes[i];
			Recovery (node);
		}

		for (int i = 0; i < livePool.Count; ++i) {
			livePool [i].Release ();
			RemoveListenner (livePool [i].Update);
			RemoveListenner (livePool [i].Release);
		}

		IPoolNode[] array = pool.ToArray ();
		for (int i = 0; i < array.Length; ++ i) {
			array [i].Release ();
			RemoveListenner (array [i].Update);
			RemoveListenner (array [i].Release);
		}

		pool.Clear ();
		livePool.Clear ();
	}

	/// <summary>
	/// 添加正在使用的列表
	/// </summary>
	void AddLive(IPoolNode node)
	{
		//插入列表
		livePool.Add (node);
	}

	/// <summary>
	/// 获取一个对象
	/// </summary>
	public T GetObject<T>()
	{
		IPoolNode node = default(IPoolNode);

		if (pool.Count > 0) {
			node = pool.Dequeue ();
		}

		if (node == null) {
			node = Activator.CreateInstance (typeof(T)) as IPoolNode;

			//监听
			AddListenner (node.Update);
			AddListenner (node.Release);
		}

		node.SetPool (this);

		AddLive (node);

		return (T)node;
	}

	/// <summary>
	/// 回收
	/// </summary>
	/// <param name="node">Node.</param>
	public void Recovery(IPoolNode node)
	{
		//移除列表
		livePool.Remove (node);

		//释放节点
		node.Recovery ();
		//回归池子
		pool.Enqueue (node);
	}

	/// <summary>
	/// 创建空闲的object，放在pool。
	/// </summary>
	public void CreateFreeObject<T>()
	{
		IPoolNode node = default(IPoolNode);
		node = Activator.CreateInstance (typeof(T)) as IPoolNode;
		node.SetPool (this);
		node.Release ();
		pool.Enqueue (node);
	}

	/// <summary>
	/// 心跳
	/// </summary>
	public void Update(int frame, float dt)
	{
		for (int i = 0; i < livePool.Count; ++i) {
			livePool [i].Update (frame, dt);
		}
	}
}
