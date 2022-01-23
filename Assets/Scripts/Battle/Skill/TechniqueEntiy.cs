using System;
using System.Collections.Generic;
using Solarmax;
using UnityEngine;

/// <summary>
/// 技能对象
/// </summary>
public class TechniqueEntiy
{

    public SkillConfig          proto;

    /// <summary>
    /// 技能使用者
    /// </summary>
    public BattleMember         sender;

    /// <summary>
    /// 目标列表
    /// </summary>
    private List<BattleMember>  targetList = new List<BattleMember>();

    /// <summary>
    /// 技能携带效果列表
    /// </summary>
    private List<ApplyEffect>   effects = new List<ApplyEffect>();
    private List<ApplyEffect>   dels = new List<ApplyEffect>();


    public float                coodown = 0f;
    protected bool              canUse  = false;
    protected bool              bEnable = true;

    public float                MaxCoodown
    {
        get { return proto.cd; }
    }

    /// <summary>
    /// 创建技能
    /// </summary>
    static public TechniqueEntiy CreateTechnique( BattleMember member, int nTechniqueID )
    {
        TechniqueEntiy entity = new TechniqueEntiy();
        if( entity != null )
        {
            entity.InitByID(member, nTechniqueID);
        }
        return entity;
    }

    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 技能 初始化接口
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public bool InitByID( BattleMember member, int nID )
    {
        SkillConfig config = SkillConfigProvider.Get().GetData(nID);
        if (config == null)
            return false;

        sender      = member;
        proto       = config;
        coodown     = config.cd;
        canUse      = true;
        return true;
    }

    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 技能 心跳接口
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public void Tick(int frame, float interval)
    {
        if ( !canUse )
        {
            coodown -= interval;
            if (coodown <= 0)
            {
                canUse  = true;
                coodown = 0;
            }
        }


        foreach( var effect in effects )
        {
            ApplyEffect.State st = effect.Tick(interval);
            if (st == ApplyEffect.State.Finished)
                dels.Add(effect);
        }

        foreach( var ef in dels )
        {
            effects.Remove(ef);
        }
        dels.Clear();
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 技能 释放接口
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public void Destroy()
    {

    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 判断技能是否可以释放
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    private bool IsCanUseTechnique(BattleMember user )
    {
        return true;
    }
    


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 主动释放技能
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public void ApplyTechnique( )
    {
        if ( canUse )
        {
            foreach( var effect in effects )
            {
                effect.DoEffect();
            }
        }
    }


    public void StardCD( )
    {
        if (proto == null)
        {
            coodown     = proto.cd;
            canUse      = false;
        }
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 主动释放技能
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    public virtual bool IsApplyTechnique( )
    {
        if ( canUse )
        {
            return true;
        }
        return false;
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 播放技能特效
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    private void PlayTechniqueEffect( BattleMember targetShip )
    {
        if( string.IsNullOrEmpty(proto.displayID) )
        {
            return;
        }

        /// 播放特效
        GameObject parent   = sender.entity.go;
        Vector3 start       = sender.GetPosition();
        if( targetShip != null )
        {
            Vector3 end     = targetShip.GetPosition();
            Vector3 dir     = (end - start).normalized;
            EffectManager.Get().PlayParticleEffect(start, Quaternion.LookRotation(dir), "", proto.effectLife, parent );
        }
        else
        {
            GameObject go   = sender.entity.go;
            Vector3 dir     = go.transform.forward;
            EffectManager.Get().PlayParticleEffect(start, Quaternion.LookRotation(dir), "", proto.effectLife, parent);
        }
    }


   
    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 计算技能目标,飞行中的飞船
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    private void CalcTargetFlyShips(BattleMember self )
    {
        if ( proto == null || self == null )
        {
            return;
        }


        Vector3 curPos              = self.GetPosition();
        Team currentTeam            = self.currentTeam;
        SceneManager sceneManager   = BattleSystem.Instance.sceneManager;
        ShipManager shipManager     = BattleSystem.Instance.sceneManager.shipManager;
        if( shipManager != null && sceneManager != null )
        {
            for (int i = 1; i < (int)TEAM.TeamMax; i++)
            {
                // 增加隐星效果
                Team targetTeam  = sceneManager.teamManager.GetTeam((TEAM)i);
                if (targetTeam == null)
                    continue;
                
                List<BattleMember> ships = sceneManager.nodeManager.sceneManager.shipManager.GetFlyShip((TEAM)i);
                for (int j = 0; j < ships.Count; j++)
                {
                    float fDistance = (curPos - ships[j].GetPosition()).magnitude;
                    if ( fDistance < proto.scope )
                    {
                        targetList.Add(ships[j]);
                    }
                                                                                                                                                        }
            }
        }
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 计算技能目标, 当前战队的所有飞船
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    private void CalcTargetBattleTeamShips( BattleTeam castTeam )
    {
        if ( proto == null || castTeam == null )
        {
            return;
        }

        List<BattleMember> ships = castTeam.members;
        for (int j = 0; j < ships.Count; j++)
        {
            targetList.Add(ships[j]);
        }
    }
    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 计算技能目标,星球上的飞船
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    private void CalcTargetBaseShips(BattleMember self )
    {
        if (proto == null)
        {
            return;
        }


        SceneManager sceneManager = BattleSystem.Instance.sceneManager;
        NodeManager  nodeManager  = BattleSystem.Instance.sceneManager.nodeManager;
        if (nodeManager != null && sceneManager != null)
        {
            List<Node> useList = nodeManager.GetUsefulNodeList();
            for( int i = 0; i < useList.Count; i++ )
            {
                List<BattleTeam> battArray = useList[i].battArray;
                if (battArray == null)
                    continue;


                for( int n = 0; n < battArray.Count; n++ )
                {
                    List<BattleMember> list = battArray[i].members;
                    if (list == null)
                        continue;

                    for (int x = 0; x < list.Count; x++ )
                    {
                        targetList.Add(list[x]);
                    }
                }
            }
            
        }
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 计算技能目标,需要作用的战队
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    private void CalcTargetBattleTeams( BattleTeam bt, out List<BattleTeam> targetList)
    {
        targetList = null;
        if (proto == null || bt == null )
        {
            return;
        }
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 计算技能目标,需要作用的队伍
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    private void CalcTargetTeams(out List<Team> targetList)
    {
        targetList = null;
        if (proto == null)
        {
            return;
        }
    }


    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 计算技能目标,需要作用的星球
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    private void CalcTargetNodes(out List<Node> targetList)
    {
        targetList = null;
        if (proto == null)
        {
            return;
        }
    }
}

