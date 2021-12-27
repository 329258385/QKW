using UnityEngine;
using System.Collections;
using System;
using System.Text;
using Solarmax;





/// <summary>
/// 背包界面
/// </summary>
public class KnapsackWindow : BaseWindow
{
	private Color       colorB = new Color (0.0f, 0.82f, 1.0f, 1.0f);	//0x00D1FF
	private Color       colorW = new Color (1.0f, 1.0f, 1.0f, 0.5f);	//

	void Start ()
	{
	}

	private void Awake()
	{
	}

	public override bool Init ()
	{

		return true;
	}

	public override void OnShow ()
	{
	}

	public override void OnHide ()
	{
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		
	}
		
	/// <summary>
	/// 显示技能详情
	/// </summary>
	private void ShowSkillInfo ()
	{
		
	}
		
	public void OnCloseClick()
	{
		UISystem.Get ().HideWindow ("RaceSkillInfoDialogWindow");
	}
}

