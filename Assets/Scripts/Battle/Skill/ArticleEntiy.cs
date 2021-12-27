using System;
using System.Collections.Generic;
using Solarmax;
using UnityEngine;





/// <summary>
/// 物品对象
/// </summary>
public class ArticleEntiy : Lifecycle2
{

    /// <summary>
    /// 物品配置信息
    /// </summary>
    public ArticleConfig            cfg;


    /// <summary>
    /// 物品当前数量
    /// </summary>
    public int                      curCoodDown;

    /// <summary>
    /// 是否可以使用
    /// </summary>
    public bool                     bCanUse = false;


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 初始化接口
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public bool Init()
    {

        return true;
    }


    public bool InitByID(long snID, int typeID )
    {
        ArticleConfig config = ArticleConfigProvider.Get().GetData(typeID);
        if (config == null)
            return false;

        cfg             = config;
        curCoodDown     = config.cd;
        return true;
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 心跳接口
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public void Tick(int frame, float interval)
    {
        if (curCoodDown > 0)
            curCoodDown--;

        if (curCoodDown <= 0)
            bCanUse = true;
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 释放接口
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public void Destroy()
    {

    }

    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 修改物品数量属性
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public void ModifyNum( int num)
    {
        
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 使用物品
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public bool Use()
    {
        return bCanUse;
    }
}

