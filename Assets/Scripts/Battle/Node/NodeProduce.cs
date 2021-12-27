using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 节点生产模块
/// </summary>
public partial class Node
{
	/// <summary>
	/// 多少帧生产一次飞船
	/// </summary>
	/// <value>The produce frame.</value>
	public int produceFrame { get; set; }

	/// <summary>
	/// 一次生产多少艘飞船
	/// </summary>
	/// <value>The produce number.</value>
	public int produceNum	{ get; set; }

	
	/// <summary>
	/// 生产飞船
	/// </summary>
	/// <param name="dt">Dt.</param>
	protected void UpdateProduce(int frame, float dt, bool fixPopulation =  true)
	{
        if (produceFrame == 0)
            produceFrame = 50;

       
        if (team == TEAM.Neutral)
            return;
        //据说此处要加天赋判断
        //MISTART-329 战斗过程中，防守方在未进入反占领状态时（即有飞船交战时）可生产飞船，进入反占领状态时不生产飞船。
        if (state != NodeState.Idle && state != NodeState.Battle && state != NodeState.Occupied)
            return;

        if (currentTeam == null)
            return;

        // 战斗状态，占领方飞船数为0，则不生产
        if (state == NodeState.Battle && GetShipCount((int)currentTeam.team) == 0)
            return;

        //生产率时间修正
        for (int i = 0; i < battArray.Count; i++ )
        {
            BattleTeam bt = battArray[i];
            if (bt.team.team != currentTeam.team)
                continue;

            float rate      = 0.5f;
            rate            *= bt.GetAttribute( TeamAttr.ProduceSpeed );
            rate            *= nodeManager.sceneManager.GetbattleScaleSpeed();
            int rateFrame   = Mathf.FloorToInt(produceFrame / rate);
            if (frame % rateFrame != 0)
                return;

            if (bt.current >= bt.currentMax )
                continue;

            AddShipNum( produceNum, bt );
            return;
        }
	}
}
