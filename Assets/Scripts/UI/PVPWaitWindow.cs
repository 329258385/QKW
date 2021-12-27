using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Solarmax;

public class PVPWaitWindow : BaseWindow
{
	public UILabel tips;
	public UILabel timeLabel;
	public UILabel[] playerNames;
	public GameObject[] championGos;
	public UILabel[] playerChampions;

	private int timeCount;

	private Queue<int> arrivingUserIndex = new Queue<int>();
	private NetPlayer[] nowUserDatas = new NetPlayer[4] {null, null, null, null};

	private string matchId;
	private string roomId;

	public override bool Init ()
	{
		RegisterEvent (EventId.OnMatchInit);
		RegisterEvent (EventId.OnMatchUpdate);

		return true;
	}

	public override void OnShow ()
	{
		timeCount = -1;
		timeLabel.gameObject.SetActive (false);

		InvokeRepeating ("TimeUpdate", 0f, 1.0f);

		MapConfig map = MapConfigProvider.Instance.GetData (BattleSystem.Instance.battleData.matchId);
		for (int i = 0; i < 4; ++i) {
			if (i < map.player_count) {
				// 默认设置shape显示，这样当成是颜色
				SetNodeShapeShow (i, true);
				// 设置下面的info的位置
				MapPlayerInfoRootPos (i, true);
			} else {
				MapPlayerInfoRootPos (i, false);
			}
		}

	}

	public override void OnHide ()
	{

	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.OnMatchInit) {
			matchId = (string)args [0];
			roomId = (string)args [1];
			IList<NetMessage.UserData> userList = (IList<NetMessage.UserData>)args [2];
			IList<int> userIndexList = (IList<int>)args [3];
			timeCount = (int)args [4];

			// format data
			for (int i = 0; i < userList.Count; ++i) {
                NetPlayer pd = new NetPlayer();
				pd.Init (userList [i]);
				if (pd.userId > 0) {
					// 真实玩家
				} else {
					// AI
					pd.name = AIManager.GetAIName (pd.userId);
					pd.icon = AIManager.GetAIIcon (pd.userId);
				}

				int index = userIndexList [i];
				nowUserDatas [index] = pd;
				// 动画
				UserEnterEffect (index);
			}

			CheckAllEntered ();
		} else if (eventId == EventId.OnMatchUpdate) {
			
			IList<NetMessage.UserData> userAddList = (IList<NetMessage.UserData>)args [0];
			IList<int> userIndexAddList = (IList<int>)args [1];
			IList<int> userIndexDeleteList = (IList<int>)args [2];

			// format data
			// delete
			for (int i = 0; i < userIndexDeleteList.Count; ++i) {
				int index = userIndexDeleteList [i];
				nowUserDatas [index] = null;
				// 动画
			}
			// add
			for (int i = 0; i < userAddList.Count; ++i) {
                NetPlayer pd = new NetPlayer();
				pd.Init (userAddList [i]);
				if (pd.userId > 0) {
					// 真实玩家
				} else {
					// AI
					pd.name = AIManager.GetAIName (pd.userId);
					pd.icon = AIManager.GetAIIcon (pd.userId);
				}

				int index = userIndexAddList [i];
				nowUserDatas [index] = pd;
				// 动画
				UserEnterEffect (index);
			}

			CheckAllEntered ();
		}
	}

	private void CheckAllEntered ()
	{
		bool allplayerenter = true;
		for (int i = 0; i < nowUserDatas.Length; ++i) {
			if (nowUserDatas [i] == null)
				allplayerenter = false;
		}
		if (allplayerenter) {
			tips.text = "成功加入4人战斗";
		}
	}

	private void TimeUpdate()
	{
		if (timeCount < 0)
			return;

		timeLabel.text = string.Format ("{0:D2}:{1:D2}", timeCount / 60, timeCount % 60);
		timeLabel.gameObject.SetActive (true);

		--timeCount;
	}

	private void UserEnterEffect(int index)
	{
		string targetTag = string.Empty;
		if (index == 0) targetTag = "A";
		else if (index == 1) targetTag = "B";
		else if (index == 2) targetTag = "C";
		else if (index == 3) targetTag = "D";
		Node target = BattleSystem.Instance.sceneManager.nodeManager.GetNode (targetTag);
		/*if (target != null && target.entity != null ) {
			target.entity.FadeEntity ( true, 0.5f );
		}*/
		Node from = BattleSystem.Instance.sceneManager.nodeManager.GetNode ("E");
		//from.MoveTo ((TEAM)(index + 1), target, 60);

//		Debug.LogFormat ("Team : {0}, from : {1}, to : {2}", index + 1, from.tag, target.tag);

		arrivingUserIndex.Enqueue (index);
		Invoke ("UserEnterNode", 0.5f);

	}

	private void UserEnterNode()
	{
		lock (this) {
			if (arrivingUserIndex.Count == 0)
				return;

			// 创建一个环绕效果

			int index = arrivingUserIndex.Dequeue ();
			SetNodeShapeShow (index, false);
			SetPlayerInfo (index, false);
		}
	}

	private void SetNodeShapeShow(int index, bool status)
	{
		// 设置球体颜色
		string tag = string.Empty;
		if (index == 0) tag = "A";
		else if (index == 1) tag = "B";
		else if (index == 2) tag = "C";
		else if (index == 3) tag = "D";

		Node n = BattleSystem.Instance.sceneManager.nodeManager.GetNode (tag);
		GameObject go = n.GetGO ();
		SpriteRenderer sr = go.transform.Find ("shape").GetComponent <SpriteRenderer> ();
		Color c = sr.color;
		c.a = status ? 0.8f : 0;
		sr.color = c;
	}

	private void MapPlayerInfoRootPos (int index, bool status)
	{
		if (status) {
			string tag = string.Empty;
			if (index == 0) tag = "A";
			else if (index == 1) tag = "B";
			else if (index == 2) tag = "C";
			else if (index == 3) tag = "D";

			Node n = BattleSystem.Instance.sceneManager.nodeManager.GetNode (tag);
			GameObject go = n.GetGO ();

			// 后面的参数100是向下偏移量
			Vector3 pos = Camera.main.WorldToScreenPoint (go.transform.position) - new Vector3 (0, 90, 0);
			pos = UICamera.mainCamera.ScreenToWorldPoint (pos);
			pos.z = 0;
			playerNames [index].transform.parent.position = pos;
			playerNames [index].transform.parent.gameObject.SetActive (true);
		} else {
			// 不显示
			playerNames [index].transform.parent.gameObject.SetActive (false);
		}
	}

	private void SetPlayerInfo(int index, bool status)
	{
        NetPlayer pd = nowUserDatas [index];
		if (pd == null) {
			//playerNames [index].transform.parent.gameObject.SetActive (false);
			return;
		}

		Team team = BattleSystem.Instance.sceneManager.teamManager.GetTeam ((TEAM)(index + 1));
		Color c = team.color;
		c.a = 0.8f;

		// name
		playerNames[index].text = pd.name;
		playerNames [index].color = c;
		// champion
		championGos[index].SetActive (true);
		playerChampions[index].text = pd.score.ToString ();
	}
}

