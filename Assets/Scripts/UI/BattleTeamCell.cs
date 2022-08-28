using System;
using UnityEngine;
using Solarmax;
using System.Collections.Generic;

public class BattleTeamCell : MonoBehaviour
{
    public UISprite         icon;
    public HUDComponent     process;
    private List<Team>      m_teamArray = new List<Team>();
    private List<int>       m_numsArray = new List<int>();


    /// ------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 设置粉丝页信息
    /// </summary>
    /// ------------------------------------------------------------------------------------------------------
    public void UpdateBattleStatus( Node node  )
	{
        /// 飞行过程或将要飞行中
        if( node == null )
        {
            return;
        }

        if(process != null )
        {
            handleProcess(node);
        }
	}

    private void handleProcess(Node node)
    {
        if (node.state == NodeState.Occupied)
        {
            float[] HPArray = { node.hp };
            if (!process.gameObject.activeSelf)
                process.gameObject.SetActive(true);
            process.ShowProgress(node.m_teamArray.ToArray(), HPArray, node.hpMax);
        }

        else if (node.state == NodeState.Capturing)
        {
            float[] HPArray = { node.hp };
            if (!process.gameObject.activeSelf)
                process.gameObject.SetActive(true);
            process.ShowProgress(node.m_teamArray.ToArray(), HPArray, node.hpMax);
        }

        else if (node.state == NodeState.Battle)
        {
            if (!process.gameObject.activeSelf)
                process.gameObject.SetActive(true);
            process.ShowProgress(node.m_teamArray.ToArray(), node.m_HPArray.ToArray());
        }

        else if (node.state == NodeState.Idle)
        {
            if (process.gameObject.activeSelf)
            {
                process.DisPlayLable();
                process.gameObject.SetActive(false);
            }
        }

        if (node.state != NodeState.Idle)
        {
            m_teamArray.Clear();
            m_numsArray.Clear();

            for (int i = 1; i < (int)TEAM.TeamMax; i++)
            {
                int shipNum = node.numArray[i];
                if (shipNum == 0)
                    continue;

                Team team = node.sceneManager.teamManager.GetTeam((TEAM)i);
                m_teamArray.Add(team);
                m_numsArray.Add(shipNum);
            }
        }
    }
}

