using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Solarmax;

public class ChestWindow : BaseWindow
{
	/// <summary>
	/// The chest0.
	/// </summary>
	public GameObject chest0;
	public UISprite curItemIcon;
	public UILabel curItemNum;
	public UILabel curItemName;
	public UILabel curItemDesc;
	public UISlider curItemSlider;
	public UILabel curItemPersent;
	public UISprite curChestIcon0;
	public UISprite curChestLast;

	public GameObject chest1;
	public GameObject itemTemp;
//	public UISprite curChestIcon1;
	int itemCount = 0;
	NetMessage.SCGainChest msg;

	int chestType = 0;

	public override bool Init ()
	{
		RegisterEvent (EventId.OnChestNotify);
		RegisterEvent (EventId.OnChestTime);
		RegisterEvent (EventId.OnChestBattle);


		return true;
	}

	public override void OnShow ()
	{
		//chest0.SetActive (false);
		chest1.SetActive (false);
		itemTemp.SetActive (false);
	}

	public override void OnHide()
	{
		
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		switch((EventId)eventId)
		{
		case EventId.OnChestNotify:
			{
				msg = args [0] as NetMessage.SCGainChest;
				if (msg == null)
					return;

			}
			break;
		case EventId.OnChestTime:
			{
				
			}
			break;
		case EventId.OnChestBattle:
			{
				
			}
			break;
		}
//		if (itemCount > 0) 
		{
			OnWindownClicked ();
		}
	}


	void OnWindownClicked()
	{
		if (itemCount > 0) {
			//chest0.SetActive (true);
			chest1.SetActive (false);
			itemCount--;
			curChestLast.spriteName = itemCount.ToString ();
			NetMessage.PackItem item = null;

			int curvalue = 0;
			int maxvalue = 0;
			int skillid = 0;
			switch (chestType) {
			case 0:
				item = msg.items[itemCount];
				curvalue = msg.add_num [itemCount];
				maxvalue = msg.levelup_num [itemCount];
            	skillid = msg.skillids [itemCount];
				break;
			
			}
			ShowItem (item,curvalue,maxvalue,skillid);
		} 
		else if (itemCount == 0) 
		{
			//chest0.SetActive (false);
			chest1.SetActive (true);
			ShowAllItem ();
			itemCount = -1;
		} else {
			
		}
	}

	void ShowItem(NetMessage.PackItem item, int curvalue, int maxvalue, int skill)
	{
		
	}

	void ShowAllItem()
	{
		
		
	}

}

