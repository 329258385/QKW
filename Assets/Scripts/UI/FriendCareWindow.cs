using System;
using System.Collections.Generic;
using UnityEngine;
using Solarmax;

public class FriendCareWindow : BaseWindow
{
	/// <summary>
	/// tabs
	/// </summary>
	public UILabel followerTab;
	public UISprite followerTabSubPic;
	public UILabel myfollowTab;
	public UISprite myfollowTabSubPic;
	/// <summary>
	/// The scroll view and grid.
	/// </summary>
	public UIScrollView scrollView;
	public UIGrid grid;
	public GameObject infoTemplate;

	private UILabel selectTab;
	private float selectFollowerTabTime;
	private List<SimplePlayerData> myFollowList;
	private List<bool> myFollowStatus;
	private List<SimplePlayerData> followerList;
	private List<bool> followerStatus;

	void Awake()
	{
		selectTab = null;
		myFollowList = new List<SimplePlayerData> ();
		followerList = new List<SimplePlayerData> ();
		myFollowStatus = new List<bool> ();
		followerStatus = new List<bool> ();

		followerTab.gameObject.GetComponent <UIEventListener> ().onClick += OnTabClick;
		myfollowTab.gameObject.GetComponent <UIEventListener> ().onClick += OnTabClick;

		scrollView.onShowMore = OnScrollViewShowMore;
	}

	public override bool Init ()
	{
		RegisterEvent (EventId.OnFriendLoadResult);
		RegisterEvent (EventId.OnFriendFollowResult);

		return true;
	}

	public override void OnShow()
	{
		myFollowList.Clear ();
		followerList.Clear ();
		myFollowStatus.Clear ();
		followerStatus.Clear ();

		// 默认选择粉丝
		OnTabClick (followerTab.gameObject);
	}

	public override void OnHide()
	{

	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		switch ((EventId)eventId) {
		case EventId.OnFriendLoadResult:
			{
				//int start = (int)args [0];
				bool myfollow = (bool)args [1];

				// 矫正一下是否是当前tab页面数据
				bool confirm = false;
				if (myfollow)
					confirm = selectTab == myfollowTab;
				else
					confirm = selectTab == followerTab;
				if (!confirm)
					return;

				// 数据列表
				List<SimplePlayerData> datas = args [2] as List<SimplePlayerData>;
				// 数据列表状态
				List<bool> followStatus = args [3] as List<bool>;

				RefreshScrollView (datas, followStatus, false);

			}
			break;
		case EventId.OnFriendFollowResult:
			{
				// 关注好友结果
				int userId = (int)args [0];
				bool follow = (bool)args [1]; // 关注或者取消关注行动
				NetMessage.ErrCode errCode = (NetMessage.ErrCode)args [2];

				if (errCode == NetMessage.ErrCode.EC_Ok) {
					Transform t = grid.transform.Find ("infomation_" + userId);
					FriendWindowCell cell = null;
					if (t != null) {
						cell = t.gameObject.GetComponent<FriendWindowCell> ();
						cell.OnCareResult (follow);
					} else {
						Debug.Log ("OnFriendFollowResult   找不到子节点");
					}

					// 我的关注，并且是取消关注某个人，则需要删除节点， 并且删除数据
					if (selectTab == myfollowTab && follow == false) {
						SimplePlayerData spd = null;
						for (int i = 0; i < myFollowList.Count; ++i) {
							if (myFollowList [i].userId == userId)
								spd = myFollowList [i];
						}
						if (spd != null) {
							myFollowList.Remove (spd);
						}
						if (t != null) {
							grid.RemoveChild(t);
							t.transform.SetParent (null);
							Destroy (t.gameObject);
							grid.Reposition ();
						}
					}

				} else {
					string t = follow ? "关注好友" : "取消关注";
					t = string.Format ("{0} userId: {1} 失败！", t, userId);
					Tips.Make (Tips.TipsType.FlowUp, t, 1);
				}
			}
			break;
		}
	}

	private void RefreshScrollView (List<SimplePlayerData> data, List<bool> status, bool useOldData)
	{

		for (int i = 0; i < data.Count; ++i) {
			GameObject go = NGUITools.AddChild (grid.gameObject, infoTemplate);
			go.name = "infomation_" + data [i].userId;
			go.SetActive (true);
			FriendWindowCell cell = go.GetComponent<FriendWindowCell> ();
			if (selectTab == myfollowTab) {
                cell.SetMyFollowInfo(data[i], status[i]);
			} else {
				cell.SetFollowerInfo (data [i], status[i]);
			}
		}

		grid.Reposition ();

		if (selectTab == followerTab) {
			if (followerList.Count == 0 || useOldData)
				scrollView.ResetPosition ();

			if (!useOldData) {
				followerList.AddRange (data);
				followerStatus.AddRange (status);
			}
		} else {
			if (myFollowList.Count == 0 || useOldData)
				scrollView.ResetPosition ();

			if (!useOldData) {
				myFollowList.AddRange (data);
				myFollowStatus.AddRange (status);
			}
		}
	}

	/// <summary>
	/// Raises the tab click event.
	/// </summary>
	/// <param name="go">Go.</param>
	private void OnTabClick(GameObject go)
	{
		if (selectTab != null && go == selectTab.gameObject) {
			return;
		}

		Color unselectcolor = new Color (1, 1, 1, 0.3f);
		followerTab.color = unselectcolor;
		myfollowTab.color = unselectcolor;

		selectTab = go.GetComponent<UILabel>();

		selectTab.color = Color.white;

		followerTabSubPic.gameObject.SetActive (selectTab == followerTab);
		myfollowTabSubPic.gameObject.SetActive (selectTab == myfollowTab);

		grid.transform.DestroyChildren ();

		if (selectTab == followerTab) {
			if (Time.realtimeSinceStartup > selectFollowerTabTime + 3) {
				selectFollowerTabTime = Time.realtimeSinceStartup;
				followerList.Clear ();
				followerStatus.Clear ();
				OnScrollViewShowMore ();
			} else {
				RefreshScrollView (followerList, followerStatus, true);
			}
		} else {
			if (myFollowList.Count == 0)
				OnScrollViewShowMore ();
			else
				RefreshScrollView (myFollowList, myFollowStatus, true);
		}

	}

	/// <summary>
	/// scrollview 加载更多
	/// </summary>
	private void OnScrollViewShowMore()
	{
		if (selectTab == followerTab) {
			NetSystem.Instance.helper.FriendLoad (followerList.Count, false);
		} else if (selectTab == myfollowTab){
			NetSystem.Instance.helper.FriendLoad (myFollowList.Count, true);
		}
	}

	/// <summary>
	/// 加好友入口
	/// </summary>
	public void OnAddFriendClick()
	{
		UISystem.Get ().HideWindow ("FriendCareWindow");
		UISystem.Get ().ShowWindow ("FriendWindow");
	}

	public void OnCloseClick()
	{
		UISystem.Get ().HideWindow ("FriendCareWindow");
		UISystem.Get ().ShowWindow ("StartWindow");
	}
}

