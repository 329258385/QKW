using UnityEngine;
using System.Collections;
using Solarmax;





/// <summary>
/// 特效父类
/// </summary>
public class EffectNode : IPoolObject<EffectNode>
{
	/// <summary>
	/// 是否使用
	/// </summary>
	public bool             isActive { get; set; }
	public GameObject       gameObject;
    public Transform        transform;
	public string			nameKey = string.Empty;

	public EffectNode()
	{
	
	}


    /// ---------------------------------------------------------------------------------------------------
    /// <summary>
    /// 特效心跳逻辑
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------
	public virtual void Tick (int frame, float interval)
	{
		
	}


    /// ---------------------------------------------------------------------------------------------------
    /// <summary>
    /// 释放资源
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------
	public virtual void Destroy ()
	{
		isActive = false;
        if (gameObject != null)
            GameObject.Destroy(gameObject);
	}


    
    /// ---------------------------------------------------------------------------------------------------
	/// <summary>
	/// 用于效果隐藏的释放，而非最后释放
	/// </summary>
    /// ---------------------------------------------------------------------------------------------------
	public override void OnRecycle ()
	{
		isActive = false;
        if (gameObject != null)
            gameObject.SetActive(false);
	}


    /// ---------------------------------------------------------------------------------------------------
	/// <summary>
	/// 是否使用状态
	/// </summary>
    /// ---------------------------------------------------------------------------------------------------
	public bool IsActive()
	{
		return isActive;
	}


    /// ---------------------------------------------------------------------------------------------------
	/// <summary>
	/// 初始化特效
	/// </summary>
    /// ---------------------------------------------------------------------------------------------------
	public virtual void InitEffectNode ( Vector3 position, Quaternion rotation )
	{
		
	}
}
