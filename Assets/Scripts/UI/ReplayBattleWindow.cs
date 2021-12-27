using System;
using UnityEngine;
using Solarmax;

public class ReplayBattleWindow : BaseWindow
{
	public UIButton pauseBtn;

	public UISprite[] raceBgs;
	public UISprite[] raceIcons;
	public UILabel[] names;
	public UILabel[] populations;

	private int playSpeed = 2;
	public UIButton[] playSpeedButton;

	public UILabel time;

	private string populationTemplate = "";

	private int nowFrame;
	private int totalFrame;

	private bool showForWatch = false; // 是否为观战
	private int battleTimeMax;

	public override bool Init ()
	{
		RegisterEvent (EventId.OnBattleReplayFrame);
		RegisterEvent (EventId.ShowForWatchMode);

		return true;
	}

	public override void OnShow ()
	{
		
		UISystem.Get ().ShowWindow ("PopTextWindow");
	}

	public override void OnHide ()
	{

	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.OnBattleReplayFrame) {
			nowFrame = (int)args [0];
			totalFrame = (int)args [1];
		} else if (eventId == EventId.ShowForWatchMode) {
			// 页面显示为观战模式
			// 隐藏加速按钮
			for (int i = 0; i < playSpeedButton.Length; ++i) {
				playSpeedButton [i].gameObject.SetActive (false);
			}
			// 隐藏暂停
			pauseBtn.gameObject.SetActive (false);
			// 显示
			showForWatch = true;
		}
	}

	private void TimerProc()
	{
		// 人口
		for (int i = 1; i < 5; i++)
		{
			Team teamTmp    =  BattleSystem.Instance.sceneManager.teamManager.GetTeam((TEAM)i);
            int current     = 0;
            int currentMax  = 0;
            for (int n = 0; n < teamTmp.battleArray.Count; n++ )
            {
                current     += teamTmp.battleArray[n].current;
                currentMax  += teamTmp.battleArray[n].currentMax;
            }

            populations[i - 1].text = string.Format(populationTemplate, current, currentMax);
		}
		// 剩余时间
		if (showForWatch) {
			// 观战
			int now = Mathf.RoundToInt(battleTimeMax - BattleSystem.Instance.sceneManager.GetBattleTime ());
			if (now >= 0) {
				time.text = string.Format ("{0:D2}:{1:D2}", now / 60, now % 60);
			}
		} else {
			// 重播
			int second = (totalFrame * 5 - nowFrame) / 50;
			time.text = string.Format ("{0:D2}:{1:D2}", second / 60, second % 60);
		}
	}

	public void OnCloseClick()
	{
		if (showForWatch) {
			UISystem.Get ().ShowWindow("CommonDialogWindow");
			EventSystem.Instance.FireEvent (EventId.OnCommonDialog,
				2, DictionaryDataProvider.GetValue (913), new EventDelegate( ()=>{

					BattleSystem.Instance.OnPlayerDirectQuit ();
					BattleSystem.Instance.Reset ();
					UISystem.Get ().HideAllWindow ();
					UISystem.Get ().ShowWindow ("CustomSelectWindowNew");
				}));

		} else {
			BattleSystem.Instance.replayManager.PlayRecordOver ();
			BattleSystem.Instance.Reset ();
			UISystem.Get ().HideAllWindow ();
			UISystem.Get ().ShowWindow ("ReplayWindow");
		}
	}

	public void OnPauseClick()
	{
		if (BattleSystem.Instance.IsPause ()) {
			BattleSystem.Instance.SetPause (false);
			pauseBtn.defaultColor = new Color (1, 1, 1, 1f);
		} else {
			BattleSystem.Instance.SetPause (true);
			pauseBtn.defaultColor = new Color (1, 1, 1, 0.5f);
		}
	}

	public void OnSpeedClick1 ()
	{
		if (playSpeed == 1)
			return;

		BattleSystem.Instance.lockStep.playSpeed = 0.5f;

		playSpeed = 1;
		SetPlaySpeedBtnStatus ();
	}

	public void OnSpeedClick2 ()
	{
		if (playSpeed == 2)
			return;

		BattleSystem.Instance.lockStep.playSpeed = 1;

		playSpeed = 2;
		SetPlaySpeedBtnStatus ();
	}

	public void OnSpeedClick3 ()
	{
		if (playSpeed == 3)
			return;

		BattleSystem.Instance.lockStep.playSpeed = 2;

		playSpeed = 3;
		SetPlaySpeedBtnStatus ();
	}

	private void SetPlaySpeedBtnStatus ()
	{
		for (int i = 0; i < playSpeedButton.Length; ++i) {
			playSpeedButton [i].isEnabled = i != (playSpeed - 1);
		}
	}
}

