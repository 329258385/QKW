using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Solarmax;

public class RoomWaitWindow : BaseWindow
{
	public UILabel roomIdLabel;
	public GameObject[] playerGos;
	public UIButton startBtn;
	public UISprite watchAmmor;
	public UILabel watchNumLabel;
	public TweenPosition[] watchPlayers;

	private string matchId;
	private string roomId;
	private NetPlayer[] allPlayers = new NetPlayer[7] {null, null, null, null, null, null, null};
	private int hostId;
	private Color enableColor = new Color (224 / 255.0f, 195 / 255.0f, 207 / 255.0f);

	private bool showWatchPlayer;
	private float watchPlayerPosInterval = -140;
	private int watchCount;

	public override bool Init ()
	{
		RegisterEvent (EventId.OnMatchInit);
		RegisterEvent (EventId.OnMatchUpdate);
		RegisterEvent (EventId.OnMatchQuit);

		return true;
	}

	public override void OnShow ()
	{
		roomIdLabel.text = string.Empty;
		for (int i = 0; i < playerGos.Length; ++i) {
			SetPlayerInfo (playerGos [i], null, -1);
		
			if (i < 4) {
				playerGos [i].transform.Find ("bg").GetComponent <UIEventListener> ().onClick = OnNormalPlayerClick;
			} else {
				playerGos [i].transform.Find ("bg").GetComponent <UIEventListener> ().onClick = OnWatchPlayerClick;
			}
		}
		startBtn.gameObject.SetActive (false);

		showWatchPlayer = false;
		ShowWatch (true);
	}

	public override void OnHide ()
	{

	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.OnMatchInit) {
			// room init
			matchId = (string)args [0];
			roomId = (string)args [1];
			IList<NetMessage.UserData> userList = (IList<NetMessage.UserData>)args [2];
			IList<int> userIndexList = (IList<int>)args [3];
			hostId = (int)args [4];

			// format data
			for (int i = 0; i < userList.Count; ++i) {
                NetPlayer pd = new NetPlayer();
				pd.Init (userList [i]);
				int index = userIndexList [i];
				allPlayers [index] = pd;
			}

			SetPage ();

		} else if (eventId == EventId.OnMatchUpdate) {
			// room update

			IList<NetMessage.UserData> userAddList = (IList<NetMessage.UserData>)args [0];
			IList<int> userIndexAddList = (IList<int>)args [1];
			IList<int> userIndexDeleteList = (IList<int>)args [2];
			IList<bool> userKickList = (IList<bool>)args [3];
			IList<int> userChangeFromList = (IList<int>)args [4];
			IList<int> userChangeToList = (IList<int>)args [5];
			if (args.Length == 7)
				hostId = (int)args [6];

			// format data
			// delete
			for (int i = 0; i < userIndexDeleteList.Count; ++i) {
				int index = userIndexDeleteList [i];

				if (allPlayers [index] != null && allPlayers [index].userId == LocalPlayer.Get ().playerData.userId) {
					// 自己退出，则关闭页面
					UISystem.Instance.HideWindow ("RoomWaitWindow");
					UISystem.Instance.ShowWindow ("CreateRoomWindow");

					if (userKickList [i]) {
						// 被踢
						Tips.Make (Tips.TipsType.FlowUp, DictionaryDataProvider.GetValue (909), 1.0f);
					}
				}

				allPlayers [index] = null;
			}
			// add
			for (int i = 0; i < userAddList.Count; ++i) {
                NetPlayer pd = new NetPlayer();
				pd.Init (userAddList [i]);
				int index = userIndexAddList [i];
				allPlayers [index] = pd;
			}
			// change pos
			for (int i = 0; i < userChangeFromList.Count; ++i) {
				int from = userChangeFromList [i];
				int to = userChangeToList [i];

                NetPlayer temp    = allPlayers [from];
				allPlayers [from] = allPlayers [to];
				allPlayers [to]   = temp;
			}

			SetPage ();

		} else if (eventId == EventId.OnMatchQuit) {
			// quit , 谁触发的quit，谁收到quit，
			NetMessage.ErrCode code = (NetMessage.ErrCode) args [0];

			if (code == NetMessage.ErrCode.EC_NotMaster) {
				Tips.Make (Tips.TipsType.FlowUp, DictionaryDataProvider.GetValue (905), 1.0f);
			} else if (code != NetMessage.ErrCode.EC_Ok) {
				Tips.Make (Tips.TipsType.FlowUp, DictionaryDataProvider.Format (901, code), 1.0f);
			}
		}
	}

	/// <summary>
	/// 设置页面
	/// </summary>
	public void SetPage ()
	{
		roomIdLabel.text = roomId.ToString ();
		watchCount = 0;
		for (int i = 0; i < allPlayers.Length; ++i) {
			SetPlayerInfo (playerGos [i], allPlayers [i], hostId);

			if (i >= 4 && allPlayers [i] != null) {
				watchCount++;
			}
		}

		startBtn.gameObject.SetActive (LocalPlayer.Get ().playerData.userId == hostId);
		watchNumLabel.text = DictionaryDataProvider.Format (910, watchCount, 3);
	}

	public void SetPlayerInfo (GameObject go, NetPlayer pd, int hostId)
	{
		UISprite bg = go.transform.Find ("bg").GetComponent<UISprite> ();
		UISprite icon = go.transform.Find ("icon").GetComponent<UISprite> ();
		//GameObject host = go.transform.FindChild ("hostPic").gameObject;
		GameObject delete = go.transform.Find ("delete").gameObject;
		UILabel name = go.transform.Find ("name").GetComponent <UILabel> ();

		if (pd == null) {
			bg.spriteName = "avatar_bg_disable";
			bg.color = Color.white;
			icon.gameObject.SetActive (false);
			//host.gameObject.SetActive (false);
			delete.gameObject.SetActive (false);
			name.gameObject.SetActive (false);
		} else {
			bg.spriteName = "avatar_bg_normal";
			bg.color = enableColor;
			icon.spriteName = pd.icon;
			icon.gameObject.SetActive (true);
			//host.SetActive (pd.userId == hostId);
			bool canDelete = LocalPlayer.Get ().playerData.userId == hostId && pd.userId != hostId;
			delete.SetActive (canDelete);
			if (pd.userId == hostId) {
				name.text = DictionaryDataProvider.Format (908, pd.name);
			} else {
				name.text = pd.name;
			}
			name.gameObject.SetActive (true);
		}
	}

	public void OnDeleteClick ()
	{
		if (LocalPlayer.Get ().playerData.userId != hostId)
			return;
		
		string name = UIButton.current.gameObject.transform.parent.name;
		if (!name.StartsWith ("player"))
			return;

		int index = int.Parse (name.Substring (6));

		int userId = allPlayers [index].userId;
		NetSystem.Instance.helper.QuitMatch (userId);
	}

	public void OnBackClick ()
	{
		UISystem.Instance.ShowWindow ("CommonDialogWindow");
		EventSystem.Instance.FireEvent (EventId.OnCommonDialog, 2, DictionaryDataProvider.GetValue(907)
			, new EventDelegate (() => {
				NetSystem.Instance.helper.QuitMatch();
			}));
	}

	public void OnStartClick ()
	{
		if (LocalPlayer.Get ().playerData.userId != hostId)
			return;

		int playerCount = 0;
		for (int i = 0; i < 4; ++i) {
			if (allPlayers [i] != null)
				++playerCount;
		}
		if (playerCount < 2) {
			Tips.Make (DictionaryDataProvider.GetValue (912));
			return;
		}

		NetSystem.Instance.helper.MatchComplete ();
	}

	/// <summary>
	/// 观战按钮
	/// </summary>
	public void OnWatchClick ()
	{
		ShowWatch ();
	}

	private void ShowWatch (bool first = false)
	{
		if (first) {
			for (int i = 0; i < watchPlayers.Length; ++i) {
				TweenPosition tp = watchPlayers [i];
				tp.SetOnFinished (() => {
					tp.gameObject.SetActive (showWatchPlayer);	
				});

				tp.gameObject.SetActive (false);
			}
		} else {
			if (showWatchPlayer) {
				// 关闭
				for (int i = 0; i < watchPlayers.Length; ++i) {
					TweenPosition tp = watchPlayers [i];
					tp.ResetToBeginning ();
					tp.from = new Vector3 (watchPlayerPosInterval * (i + 1), 0, 0);
					tp.to = Vector3.zero;
					tp.PlayForward ();
					tp.gameObject.SetActive (true);
				}

				showWatchPlayer = false;
			} else {
				// 打开
				for (int i = 0; i < watchPlayers.Length; ++i) {
					TweenPosition tp = watchPlayers [i];
					tp.ResetToBeginning ();
					tp.from = Vector3.zero;
					tp.to = new Vector3 (watchPlayerPosInterval * (i + 1), 0, 0);
                    tp.PlayForward();
					tp.gameObject.SetActive (true);
				}

				showWatchPlayer = true;
			}
		}

		// 方向小标
		if (showWatchPlayer) {
			watchAmmor.transform.localScale = new Vector3 (-1, 1, 1);
		} else {
			watchAmmor.transform.localScale = Vector3.one;
		}
	}

	/// <summary>
	/// 加入观战席位
	/// </summary>
	private void OnWatchPlayerClick (GameObject go)
	{
		Debug.Log ("watch player click : " + go.transform.parent.name);

		int index = int.Parse (go.transform.parent.name.Substring (6));
		if (allPlayers [index] == null) {
			// 目标位置为空
			NetSystem.Instance.helper.RequestMatchMovePos (LocalPlayer.Get ().playerData.userId, index);
		}
	}

	/// <summary>
	/// 加入玩家席位
	/// </summary>
	private void OnNormalPlayerClick (GameObject go)
	{
		Debug.Log ("normal player click : " + go.transform.parent.name);
		int index = int.Parse (go.transform.parent.name.Substring (6));
		if (allPlayers [index] == null) {
			// 目标位置为空
			NetSystem.Instance.helper.RequestMatchMovePos (LocalPlayer.Get ().playerData.userId, index);
		}
	}
}
