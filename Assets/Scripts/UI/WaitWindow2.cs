using UnityEngine;
using System.Collections.Generic;
using Solarmax;

public class WaitWindow2 : BaseWindow
{
	public UILabel tips;
	public UILabel time;
	public GameObject infoTemplate;
	public GameObject[] posRoots; //1v1, 1v1v1, 1v1v1v1, 2v2
	public GameObject cancelBtn;

	private List<GameObject> infoList = new List<GameObject>();

	private int playerCount;

	private int timeCount;

	public void Awake()
	{
		
	}

	public override bool Init ()
	{
		RegisterEvent (EventId.OnRoomRefresh);
		RegisterEvent (EventId.OnStartMatch3);
		RegisterEvent (EventId.OnMatch3Notify);
		RegisterEvent (EventId.OnStartMatch2);

		return true;
	}

	public override void OnShow()
	{
		InvokeRepeating ("UpdateTime", 0f, 1.0f);

		// 隐藏所有位置节点
		for (int i = 0; i < posRoots.Length; ++i)
		{
			posRoots [i].SetActive (false);
		}

		AudioManger.Get ().PlayAudioBG ("Wandering");

		tips.gameObject.SetActive (false);

		// 设置房间显示数据
		SetPage(true);

	}

	public void UpdateTime()
	{
		time.text = string.Format ("{0:D2}:{1:D2}", timeCount / 60, timeCount % 60);

		timeCount++;
	}

	public override void OnHide()
	{
		for (int i = 0; i < infoList.Count; ++i)
		{
			Destroy (infoList [i]);
		}
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		switch ((EventId)eventId) 
		{
		case EventId.OnRoomRefresh:
			{
				IList<NetMessage.UserData> userDatas = args [0] as IList<NetMessage.UserData>;

				for (int i = 0; i < playerCount; ++i) {
					if (i < userDatas.Count) {
                        NetPlayer pd = new NetPlayer();
						pd.Init (userDatas [i]);
						SetPosInfo (i, pd);
					} else {
						SetPosInfo (i, null);
					}
				}

				//cancelBtn.SetActive (needShowCancel);

			}
			break;
		case EventId.OnStartMatch3:
			{
				playerCount = 4;

				SetPage (false);
			}
			break;
		case EventId.OnMatch3Notify:
			{

				List<NetPlayer> playerList = args [0] as List<NetPlayer>;
				for (int i = 0; i < playerCount; ++i) {
					if (i < playerList.Count) {
                        NetPlayer pd = playerList [i];
						SetPosInfo (i, pd);
					} else {
						SetPosInfo (i, null);
					}
				}

				//cancelBtn.SetActive (needShowCancel);

			}
			break;
		case EventId.OnStartMatch2:
			{


				//cancelBtn.gameObject.SetActive (false);
			}
			break;
		}
	}

	private void PlayAnimation()
	{

		string statename = string.Empty;
		if (BattleSystem.Instance.battleData.teamFight) {
			statename = "WaitWindow_2v2_in";
		} else {
			if (playerCount == 2)
				statename = "WaitWindow_1v1_in";
			else if (playerCount == 3)
				statename = "WaitWindow_1v1v1_in";
			else if (playerCount == 4)
				statename = "WaitWindow_1v1v1v1_in";
		}

		Animator ani = gameObject.GetComponent<Animator> ();
		ani.Play (statename);
	}


	/// <summary>
	/// 设置页面信息
	/// </summary>
	/// <param name="matchId">Match identifier.</param>
	private void SetPage(bool useMapInfo)
	{
		if (useMapInfo) {
			if (string.IsNullOrEmpty (BattleSystem.Instance.battleData.matchId))
				return;

			MapConfig matchMap = MapConfigProvider.Instance.GetData (BattleSystem.Instance.battleData.matchId);
			playerCount = matchMap.player_count;
		}

		if (playerCount < 2)
			return;

		// 根据类型和人数判断选用哪种模式
		GameObject root = GetPosRoot();
		root.SetActive (true);

		for (int i = 0; i < playerCount; ++i) {
			GameObject pos = root.transform.Find ("pos" + (i + 1)).gameObject;
			GameObject info = NGUITools.AddChild (pos, infoTemplate);
			info.name = "info";
			info.SetActive (true);
			SetPosInfo (i, null);
		}

		tips.text = string.Format ("成功加入{0}人战斗", playerCount);
		tips.gameObject.SetActive (true);
	}

	private GameObject GetPosRoot()
	{
		GameObject root = null;
		if (BattleSystem.Instance.battleData.teamFight) {
			root = posRoots [3];
		} else {
			root = posRoots [playerCount - 2];
		}

		return root;
	}

	private void SetPosInfo(int pos, NetPlayer data)
	{
		GameObject root = GetPosRoot ();
		GameObject info = root.transform.Find ("pos" + (pos + 1)).Find("info").gameObject;

		if (data == null) {
			info.transform.Find ("full").gameObject.SetActive(false);
			info.transform.Find ("empty").gameObject.SetActive(true);

			return;
		}

		Team t =  BattleSystem.Instance.sceneManager.teamManager.GetTeam((TEAM)(pos + 1));
		Color color = t.color;
		color.a = 1.0f;

		info.transform.Find ("full").gameObject.SetActive(true);
		info.transform.Find ("empty").gameObject.SetActive(false);

		UILabel score = info.transform.Find ("full/score/num").GetComponent<UILabel> ();
		//UISprite kuang = info.transform.FindChild ("full/icon").GetComponent<UISprite> ();
		NetTexture icon = info.transform.Find ("full/usericon").GetComponent<NetTexture> ();
		UILabel name = info.transform.Find ("full/name").GetComponent<UILabel> ();
		//UILabel unionName = info.transform.FindChild ("full/union/name").GetComponent<UILabel> ();

		score.text = data.score.ToString();
		icon.picUrl = data.icon;
		icon.picColor = color;
		//kuang.color = color;
		name.text = data.name;
		name.color = color;
		//unionName.text = data.unionName;

		Animator ani = info.GetComponent<Animator> ();
		if (data.userId == LocalPlayer.Get ().playerData.userId) {
			ani.Play ("WaitWindow_information1_mine");
		} else {
			ani.Play ("WaitWindow_information1_other");
		}
	}

	public void OnCloseClick()
	{
		UISystem.Get ().HideWindow ("WaitWindow");
		UISystem.Get ().ShowWindow ("CustomSelectWindowNew");
	}

	public void OnQuitClicked()
	{
		NetSystem.Instance.helper.RequestQuitRoom ();
	}
}

