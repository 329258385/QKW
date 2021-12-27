using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Solarmax;

public class SelectRaceWindow : BaseWindow
{

	public UITexture[] PicList;
	public UIEventListener[] iconList;
	public GameObject PicSelect;
	public UIPlayAnimation aniPlayer;	// 动画

	public UISprite spriteBtn;
	public UILabel NameLabel;

	string[] raceName = {"归零者","歌者","瓦肯人","博格人","克林贡人","罗姆兰人","可汗"};

	public override void OnShow()
	{
		NameLabel.gameObject.SetActive (false);
		PicSelect.SetActive (false);

		for (int i = 0; i < PicList.Length; i++) 
		{
			PicList[i].mainTexture = Resources.Load(string.Format("HeroIcon/icon{0}",i + 1)) as Texture2D;
		}
		for (int i = 0; i < iconList.Length; i++) {
			iconList [i].onClick += OnIconClicked;
		}
		spriteBtn.color = Color.grey;
		NameLabel.text = string.Empty;
		PicSelect.SetActive (false);
	}


	public override void OnHide()
	{

	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		
	}

	void OnIconClicked(GameObject obj)
	{
		NameLabel.gameObject.SetActive (true);
		PicSelect.SetActive (true);
		string name = obj.transform.parent.name;
		string aniName = string.Empty;
		spriteBtn.color = Color.white;
		switch (name)
		{
		case "race1":
			NameLabel.text = raceName [0];
			aniName = ("SelectRaceWindow_race1");
			break;
		case "race2":
			NameLabel.text = raceName [1];
			aniName = ("SelectRaceWindow_race2");
			break;
		case "race3":
			NameLabel.text = raceName [2];
			aniName = ("SelectRaceWindow_race3");
			break;
		case "race4":
			NameLabel.text = raceName [3];
			aniName = ("SelectRaceWindow_race4");
			break;
		case "race5":
			NameLabel.text = raceName [4];
			aniName = ("SelectRaceWindow_race5");
			break;
		}
		NameLabel.transform.parent = obj.transform.parent;
		Vector3 pos = NameLabel.transform.localPosition;
		pos.x = 0f;
		pos.y = -235f;
		NameLabel.transform.localPosition = pos;
		NameLabel.transform.localScale = Vector3.one;
		PicSelect.transform.parent = obj.transform.parent;
		PicSelect.transform.localPosition = Vector3.zero;
		PicSelect.transform.localScale = Vector3.one;
		PlayAnimation (aniName);
	}


	public void OnStartGame()
	{
		NetSystem.Instance.helper.MatchGame2 ();
	}

	private void PlayAnimation(string state)
	{
		aniPlayer.clipName = state;
		aniPlayer.resetOnPlay = true;
		aniPlayer.Play (true, false);
	}
}
