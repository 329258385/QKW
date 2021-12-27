using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;









public class BattleNodeCell : MonoBehaviour
{
    public UISprite     icon;
    public HUDComponent process;

    private List<Team>  m_teamArray = new List<Team>();
    private List<int>   m_numsArray = new List<int>();

    public void handleProcess( Node node )
	{
        if (node == null)
            return;

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

        else if( node.state == NodeState.Idle )
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

