using System;
using System.Collections.Generic;
using UnityEngine;
using Solarmax;






public class BufferEntiy : Lifecycle2
{
	
    public enum EBuffState
    {
        EBS_Idle        = 0,        // 空闲
        EBS_Initing     = 1,        // 初始化
        EBS_Updating    = 2,        // 更新
        EBS_Destroying  = 3,	    // 删除
    }

    public CTagBufferConfig         config;

    /// <summary>
	/// 用于对队伍buff，影响其挂载特效
	/// </summary>
	protected Node                  effectNode;
    protected BattleMember          effectShip;
    public float                    life = 0f;          /// buffer 生存时间
    public bool                     bDestroy = false;

    private int                     effectFrameNum = 0;
    

    /// <summary>   
    /// 创建技能
    /// </summary>
    static public BufferEntiy CreateBufferEntiy(int nBufferID, float life )
    {
        BufferEntiy entity = new BufferEntiy();
        if (entity != null)
        {
            entity.InitByID(nBufferID, life);
        }
        return entity;
    }

    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// buffer 初始化接口
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public bool Init()
    {
        effectNode      = null;
        effectShip      = null;
        effectFrameNum  = 0;
        return true;
    }


    public bool InitByID( int nID, float life )
    {
        CTagBufferConfig cfg = SkillBufferConfigProvider.Get().GetData(nID);
        if (cfg == null)
            return false;

        config      = cfg;
        this.life   = life;
        return true;
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// buffer 心跳接口
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public void Tick(int frame, float interval)
    {
        /// 消失了，就不用管了，上层去处理释放
        if (bDestroy) return;

        life -= interval;
        if(life <= 0 )
        {
            UnEffect();
        }

        effectFrameNum++;
        if ( config.interval > 0 && effectFrameNum % config.interval == 0 )
        {
            DoIntervalEffect();
        }
    }

    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// buffer 释放接口
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public void Destroy()
    {
        bDestroy = true;
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// buffer 起zuoy
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public void DoEffect()
    {
        bDestroy = false;
        effectFrameNum = 1;
        if ( config.logicID > 0 )
        {
            ApplyEffect.DoEffect(effectShip, null, config);
        }
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public void UnEffect()
    {
        bDestroy = true;
        
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 给飞船增加buffer
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public bool ApplyShipBuffer(BattleMember castShip )
    {
        /// 作用属性
        effectShip = castShip;
        DoEffect();

        /// 表现效果
        if( !string.IsNullOrEmpty(config.effectId ))
        {
            PlayBuffEffect();
        }
        return true;
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 给星球增加buffer
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public bool ApplyNodeBuffer( Node castNode )
    {
        /// 作用属性
        effectNode  = castNode;
        DoEffect();

        /// 表现效果
        if (!string.IsNullOrEmpty(config.effectId))
        {
            PlayBuffEffect();
        }
        return true;
    }



    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 主动释放技能
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
	public void PlayBuffEffect( )
    {
        if (config == null )
            return;

        //if( config.targetType == TARGETTYPE.Ship )
        //{
        //    bufferDisplay = EffectManager.Get().PlayBufferEffect( Vector3.zero, config.effectId, effectShip.entity.go );
        //}

        //else if( config.targetType == TARGETTYPE.Node )
        //{
        //    bufferDisplay = EffectManager.Get().PlayBufferEffect(Vector3.zero, config.effectId, effectNode.GetGO() );
        //}
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 间隔效果
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    private void DoIntervalEffect( )
    {
        ApplyEffect.DoEffect( effectShip, null , config );
    }
}

