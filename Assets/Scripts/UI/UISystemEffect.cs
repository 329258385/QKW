using System;
using UnityEngine;

/// <summary>
/// 此类用于给UISystem做窗口切换通用特效的操作
/// </summary>
public partial class UISystem
{
	/// <summary>
	/// 渐出窗口，逐渐隐藏
	/// </summary>
	/// <param name="name">Name.</param>
    public void FadeOutWindow(string name)
    {
        if (!IsWindowVisible(name))
        {
            return;
        }

        
        GameObject go = GetWindow(name).gameObject;

        TweenAlpha ta = go.GetComponent<TweenAlpha>();
        if (ta == null)
        {
            ta = go.AddComponent<TweenAlpha>();
        }

        ta.ResetToBeginning();
        ta.from = 1;
        ta.to = 0;
        ta.duration = 0.5f;
        ta.SetOnFinished(() =>
        {
            HideWindow(name);
        });
    }

	public void FadeBattle(bool fadeIn, EventDelegate ed = null)
	{
		// 战斗的节点的渐入
		GameObject battle = BattleSystem.Instance.battleData.sceneRoot;
		battle.SetActive (true);
        TweenAlpha ts = battle.GetComponent<TweenAlpha> ();
		if (ts == null) {
			ts = battle.AddComponent<TweenAlpha> ();
		}
		ts.ResetToBeginning ();
		if (fadeIn) 
		{
			ts.from		= 0.5f;
			ts.to		= 1f;
		} 
		else 
		{
			ts.from		= 0.5f;
			ts.to		= 1f;
		}

		ts.duration = 0.5f; 
		ts.SetOnFinished (() => {
			if (ed != null)
			{
				ed.Execute ();
			}
		});

		ts.Play (true);
	}
}

