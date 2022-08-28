using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Solarmax;






public class SelectTeamWindow : BaseWindow
{
    /// <summary>
    /// 小队头像
    /// </summary>
    public UISprite[]           iconList;
    public UIEventListener[]    listTeam;

    /// <summary>
    /// 攻策略集合
    /// </summary>
    public UIEventListener[]    usePublicy;
    public UISprite[]           Techniques;


    /// <summary>
    /// 当前选择的分队
    /// </summary>
    private int                 selectBattle = -1;
    private int                 selectIndex  = -1;


    /// <summary>
    /// 本地玩家的队伍
    /// </summary>
    private Team                curTeam = null;

    /// <summary>
    /// 战队
    /// </summary>
    public BattleTeamCell       battle1;
    public BattleTeamCell       battle2;
    public BattleTeamCell       battle3;

    public UILabel              populationValueSelf;        // 自己数据
    public UILabel              populationValueEnemy;       // 敌方数据
    public UISprite             Process;
    public UIEventListener      processListener;


    void Start()
	{
        selectIndex = 0;
        for (int i = 0; i < listTeam.Length; i++)
        {
            listTeam[i].onPress += OnSelectTeamClicked;
        }

        for (int i = 0; i < usePublicy.Length; i++)
        {
            usePublicy[i].onPress += OnUseTechniqueClicked;
        }

        processListener.onDrag      += HandleDivideforces;
    }


	public override bool Init ()
    {
        RegisterEvent(EventId.OnPopulationUp);
        RegisterEvent(EventId.OnPopulationDown);
        RegisterEvent(EventId.OnPlayerMoveStart);
        RegisterEvent(EventId.OnOccupiedNode);
        return true;
	}

    private void Update()
    {
        Team team                   = BattleSystem.Instance.sceneManager.teamManager.GetTeam(BattleSystem.Instance.battleData.currentTeam);
        int current                 = 0;
        int currentMax              = 0;
        for (int i = 0; i < team.battleArray.Count; i++ )
        {
            current                 += team.battleArray[i].current;
            currentMax              += team.battleArray[i].currentMax;
        }
        populationValueSelf.text    = string.Format("{0}/{1}", current, currentMax);


        TEAM t2                     = BattleSystem.Instance.battleData.currentTeam == TEAM.Team_1 ? TEAM.Team_2 : TEAM.Team_1;
        Team Enemy                  = BattleSystem.Instance.sceneManager.teamManager.GetTeam(t2);
        for (int i = 0; i < Enemy.battleArray.Count; i++)
        {
            current                 += Enemy.battleArray[i].current;
            currentMax              += Enemy.battleArray[i].currentMax;
        }
        populationValueEnemy.text    = string.Format("{0}/{1}", current, currentMax);
    }


    /// <summary>
    /// 显示当前飞船数量
    /// </summary>
    public override void OnShow()
	{
        curTeam         = LocalPlayer.Get().playerData.currentTeam;
    }


	public override void OnHide()
	{
        Clear();
	}


    /// ---------------------------------------------------------------------------------------------------------
    /// <summary>
    ///  游戏内触发事件的处理接口
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------------
	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{

        if (eventId == EventId.OnPopulationUp)
        {

            //设置显示人口数量
            int pop         = (int)args[0];
            Team team       = BattleSystem.Instance.sceneManager.teamManager.GetTeam(BattleSystem.Instance.battleData.currentTeam);
            Color color     = team.color;
            color.a         = 0.7f;
        }

        if (eventId == EventId.OnPopulationDown)
        {

            //设置显示人口数量
            int pop        = (int)args[0];
            Team team       = BattleSystem.Instance.sceneManager.teamManager.GetTeam(BattleSystem.Instance.battleData.currentTeam);
            Color color     = team.color;
            color.a         = 0.7f;
        }
    }

   
    /// ---------------------------------------------------------------------------------------------------------
    /// <summary>
    ///  使用技能
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------------
    public void OnUseTechniqueClicked( GameObject obj, bool state)
    {
        if( state )
        {
            //ClickEffect(obj);

            List<TechniqueEntiy> technique = LocalPlayer.Get().vecTechniques;
            if ( obj.name.Equals("A1"))
            {
                //if( !technique[0].IsApplyTechnique() )
                //{
                //    Tips.Make(Tips.TipsType.FlowUp, "技能CD中", 1.0f);
                //    return;
                //}
                curTeam.battleArray[selectIndex].UpdateBattleTeamFormation(Formation.FormationAttack);
                //technique[0].ApplyTechnique(BattleSystem.Instance.battleData.currentBattleTeam);
            }
            if (obj.name.Equals("A2"))
            {
                //if (!technique[1].IsApplyTechnique())
                //{
                //    Tips.Make(Tips.TipsType.FlowUp, "技能CD中", 1.0f);
                //    return;
                //}
                //technique[1].ApplyTechnique(BattleSystem.Instance.battleData.currentBattleTeam);
                curTeam.battleArray[selectIndex].UpdateBattleTeamFormation(Formation.FormationDefensive);
            }
            if (obj.name.Equals("A3"))
            {
                //if (!technique[2].IsApplyTechnique())
                //{
                //    Tips.Make(Tips.TipsType.FlowUp, "技能CD中", 1.0f);
                //    return;
                //}
                //technique[2].ApplyTechnique(BattleSystem.Instance.battleData.currentBattleTeam);
                curTeam.battleArray[selectIndex].UpdateBattleTeamFormation(Formation.FormationSurround );
            }
            /*if (obj.name.Equals("A4"))
            {
                if (!technique[3].IsApplyTechnique())
                {
                    Tips.Make(Tips.TipsType.FlowUp, "技能CD中", 1.0f);
                    return;
                }
                technique[3].ApplyTechnique();
            }*/
            Debug.LogError(" down ");
        }
        else
        {
            Debug.LogError(" Up ");
        }
    }


    /// ---------------------------------------------------------------------------------------------------------
    /// <summary>
    ///  选择队伍
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------------
    void OnSelectTeamClicked(GameObject obj, bool state)
	{
        if (curTeam == null)
            return;


        int oldSelect           = selectIndex;
        BattleTeam bt           = null;
        Node currentNode        = null;
		if( obj.name == "T1" && curTeam.battleArray.Count>= 1 )
        {
            selectBattle    = curTeam.battleArray[0].ID;
            selectIndex     = 0;
            bt              = curTeam.battleArray[0];
            currentNode     = curTeam.battleArray[0].curentNode;
        }

        if (obj.name == "T2" && curTeam.battleArray.Count >= 2 )
        {
            selectBattle    = curTeam.battleArray[1].ID;
            selectIndex     = 1;
            bt              = curTeam.battleArray[1];
            currentNode     = curTeam.battleArray[1].curentNode;
        }

        if (obj.name == "T3" && curTeam.battleArray.Count >= 3 )
        {
            selectBattle    = curTeam.battleArray[2].ID;
            selectIndex     = 2;
            bt              = curTeam.battleArray[2];
            currentNode     = curTeam.battleArray[2].curentNode;
        }

        BattleSystem.Instance.battleData.currentBattleTeam  = bt;
        BattleSystem.Instance.battleData.BattleTeamID       = selectBattle;
        
        //TouchHandler.SelectHeroNode(currentNode);
    }


    /// ---------------------------------------------------------------------------------------------------------
    /// <summary>
    ///  清除
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------------
    void Clear()
	{
        for (int i = 0; i < listTeam.Length; i++)
        {
            listTeam[i].onPress -= OnSelectTeamClicked;
        }

        for (int i = 0; i < usePublicy.Length; i++)
        {
            usePublicy[i].onPress -= OnUseTechniqueClicked;
        }
	}


    /// ------------------------------------------------------------------------------------------------
    /// <summary>
    /// 更新技能CD
    /// </summary>
    /// ------------------------------------------------------------------------------------------------
    private void UpdateTechniqueCD( )
    {
        List<TechniqueEntiy> technique = LocalPlayer.Get().vecTechniques;
        for (int i = 0; i < Techniques.Length; i++)
        {
            float fill               = technique[i].coodown / technique[i].MaxCoodown;
            if (fill <= 0f)
                fill = 1f;
            Techniques[i].fillAmount = fill;
        }
    }


    /// ------------------------------------------------------------------------------------------------
    /// <summary>
    /// 更新物品的使用状态
    /// </summary>
    /// ------------------------------------------------------------------------------------------------
    private void UpdateArticleStats()
    {
        List<TechniqueEntiy> technique = LocalPlayer.Get().vecTechniques;
        for (int i = 0; i < Techniques.Length; i++)
        {
            float fill = technique[i].coodown / technique[i].MaxCoodown;
            if (fill <= 0f)
                fill = 1f;
            Techniques[i].fillAmount = fill;
        }
    }


    /// ------------------------------------------------------------------------------------------------
    /// <summary>
    /// 处理分兵
    /// </summary>
    /// ------------------------------------------------------------------------------------------------
    private void HandleDivideforces( GameObject obj, Vector2 delte )
    {
        Process.fillAmount                  = delte.x / 1080f;
    }
}

