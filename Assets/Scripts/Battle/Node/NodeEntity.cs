using UnityEngine;
using System;
using System.Collections;







/// <summary>
/// 实体操作
/// </summary>
public partial class Node
{
	/// <summary>
	/// 实体
	/// </summary>
	/// <value>The entity.</value>
    public DisplayEntity	entity      { get; set; }

	/// <summary>
	/// 攻击点
	/// </summary>
	public Transform		AP;

	/// <summary>
	/// 物体的外观
	/// </summary>
    public string           perfab = string.Empty;

    /// <summary>
    /// 物体缩放
    /// </summary>
    public float            fscale = 1.0f;
    public float            fRadius = 0f;

    /// <summary>
    /// 节点的位置信息
    /// </summary>
    public Vector3          nodePosition = Vector3.zero;

	/// <summary>
	/// 初始化节点
	/// </summary>
	void InitNode( GameObject go = null )
	{
		if (entity != null) 
        {
			entity.Destroy ();
			entity = null;
		}

        entity = CreateEntity(tag, perfab, go);
		AP     = go.transform.Find("AP");
	}

	/// <summary>
	/// 创建一个实体
	/// </summary>
	/// <returns>The entity.</returns>
	DisplayEntity CreateEntity(string name, string perfab, GameObject obj = null )
	{
		DisplayEntity ret = new DisplayEntity(name, false );
        if (ret != null && obj == null )
        {
			ret.nodeType	= nodeType;
            ret.perfab		= InitShape(perfab);
            ret.Init();
        }
		ret.go = obj;
		return ret;
	}


    /// <summary>
    /// 处理特殊星球的外形，主要对于随机可变的 
    /// </summary>
    public string InitShape(string perfab)
    {
        string str = perfab;
        if (nodeType == NodeType.Planet)
        {
            int shape = BattleSystem.Instance.battleData.rand.Range(1, 8);
            return perfab + string.Format("{0:D2}", shape);
        }
        return str;
    }

	/// <summary>
	/// 设置坐标
	/// </summary>
	public void SetPosition(float x, float y, float z = 0.0f )
	{
        nodePosition.x = x;
        nodePosition.y = y;
        nodePosition.z = z;


        if (entity == null)
            return;

        entity.SetPosition(nodePosition);
	}

	/// <summary>
	/// 设置坐标
	/// </summary>
	public void SetPosition(Vector3 pos)
	{
        nodePosition = pos;
		if (entity == null)
			return;

		entity.SetPosition (pos);
	}

	/// <summary>
	/// 获取坐标
	/// </summary>
	/// <returns>The positiong.</returns>
	public Vector3 GetPosition()
	{
        return nodePosition;
	}

	/// <summary>
	/// 设置缩放比例
	/// </summary>
	public void SetScale(float scale)
	{
        fscale = scale;
        if ( entity != null)
        {
            entity.SetScale(fscale);
        }
    }

	/// <summary>
	/// Gets the scale.
	/// </summary>
	public float GetScale()
	{
        return fscale;
	}

	/// <summary>
	/// 设置转动角度
	/// </summary>
	/// <param name="r3">R3.</param>
	public void SetRotation(Vector3 r3)
	{
		if (entity != null) 
		{
            entity.SetEulerAngles(r3);
		}
	}

	/// <summary>
	/// 获取基础距离
	/// </summary>
	/// <returns>The dist.</returns>
	public float GetDist()
	{
        return fscale * 0.65f;
	}

	/// <summary>
	/// 获取飞船分布
	/// </summary>
	/// <returns>The width.</returns>
	public float GetWidth()
	{
        return fRadius;
	}


	/// <summary>
	/// Gets the G.
	/// </summary>
	/// <returns>The G.</returns>
	public GameObject GetGO()
	{
		if (entity == null) 
		{
			return null;
		}
		return entity.go;
	}

	/// <summary>
	/// 获取攻击范围线
	/// </summary>
	/// <returns>The attack rage.</returns>
	public float GetAttackRage()
	{
		return AttackRage;
	}


	/// <summary>
	/// 变形
	/// 只对黑暗星球有效
	/// </summary>
	public void Deformating(string res)
	{
		
	}
}
