using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;








/// <summary>
/// 背包数据管理器
/// </summary>
public class KnapsackDataHandler : Solarmax.Singleton<KnapsackDataHandler>, IDataHandler
{

	/// <summary>
	/// 所有物品列表
	/// </summary>
	public Dictionary<long, ArticleEntiy>            bagList;


	public bool Init()
	{
        bagList = new Dictionary<long, ArticleEntiy>();

        Release ();

		EventSystem.Instance.RegisterEvent(EventId.OnAddArticleEvent, this, null, OnEventHandler);
		EventSystem.Instance.RegisterEvent(EventId.OnDelArticleEvent, this, null, OnEventHandler);
		EventSystem.Instance.RegisterEvent(EventId.OnUpdateArticleEvent, this, null, OnEventHandler);
		return true;
	}

	public void Tick(float interval)
	{

	}

	public void Destroy()
	{
		Release ();
		EventSystem.Instance.UnRegisterEvent(EventId.OnAddArticleEvent, this);
		EventSystem.Instance.UnRegisterEvent(EventId.OnDelArticleEvent, this);
		EventSystem.Instance.UnRegisterEvent(EventId.OnUpdateArticleEvent, this);
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	public void Release()
	{

	}
		


	private void OnEventHandler(int eventId, object data, params object[] args)
	{
        if (eventId == (int)EventId.OnAddArticleEvent)
        {
            /// 增加物品
        }
        else if(eventId == (int)EventId.OnDelArticleEvent)
        {
            /// 增加物品
            /// 
        }
        else if (eventId == (int)EventId.OnUpdateArticleEvent)
        {
            /// 增加物品
            /// 
        }
    }


    /// <summary>
    /// 根据服务器ID找物品
    /// </summary>
    public ArticleEntiy Find( long sn )
    {
        ArticleEntiy ret = null;
        if (!bagList.TryGetValue(sn, out ret))
            return null;
        return ret;
    }

    /// <summary>
    /// 根据TypeID找物品
    /// </summary>
    public ArticleEntiy FindByTypeID(int typeID )
    {
       
        foreach( var a in bagList )
        {
            if (a.Value.cfg.typeID == typeID)
                return a.Value;
        }
        return null;
    }

    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 增加
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public void Add( long sn, int typeID )
    {
        ArticleEntiy pEntiy = new ArticleEntiy();
        if( pEntiy.InitByID( sn, typeID ) )
        {
            bagList.Add(sn, pEntiy);
        }
        else
        {
            pEntiy = null;
        }
    }

    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 删除
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public void Del( long sn )
    {
        ArticleEntiy pEntiy = Find(sn);
        if( pEntiy != null )
        {
            //bagList.Remove(pEntiy.snID);
        }

        pEntiy = null;
    }

    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 修改物品属性
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public void Modify(long sn, int num )
    {
        ArticleEntiy pEntiy = Find(sn);
        if (pEntiy != null )
        {
            pEntiy.ModifyNum(num);
        }
        else
        {
            pEntiy = null;
        }
    }



    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 使用物品接口
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public bool TryUseArticle( int typeID )
    {
        ArticleEntiy pEntiy = FindByTypeID(typeID);
        if (pEntiy != null)
        {
            return pEntiy.Use();
        }

        return false;
    }
}

