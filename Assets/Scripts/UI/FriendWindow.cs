using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Solarmax;

public class FriendWindow : BaseWindow
{
	public UITexture showQRTexture;
    public UILabel guanzhuLabel;
    public UILabel fensiLabel;

    public UILabel followerTab;
    public UILabel myfollowTab;

    private UILabel selectTab;
	/// <summary>
	/// The scroll view.
	/// </summary>
	public UIScrollView scrollView;
	public UIGrid grid;
	/// <summary>
	/// The info template. 用于列表项
	/// </summary>
	public GameObject infoTemplate;
	/// <summary>
	/// 输入框
	/// </summary>
	public GameObject inputPage;
	public UIInput inputComp;


	private List<SimplePlayerData> playerList;

   
    private float selectFollowerTabTime = 0;
	private void Awake ()
	{
        selectTab       = null;
		playerList      = new List<SimplePlayerData>();
        
		scrollView.onShowMore = OnRecommendFriendsShowMore;

        followerTab.gameObject.GetComponent<UIEventListener>().onClick += OnTabClick;
        myfollowTab.gameObject.GetComponent<UIEventListener>().onClick += OnTabClick;

	}

	public override bool Init ()
	{
		RegisterEvent (EventId.OnFriendLoadResult);
		RegisterEvent (EventId.OnFriendSearchResult);
        RegisterEvent (EventId.OnFriendFollowResult);
        RegisterEvent (EventId.OnFriendNotifyResult);
		return true;
	}

	public override void OnShow ()
	{
        guanzhuLabel.text   = string.Format("({0})", FriendDataHandler.Get().myFollowList.Count);
        fensiLabel.text     = string.Format("({0})", FriendDataHandler.Get().followerStatus.Count);
        OnTabClick(myfollowTab.gameObject);
		SetInfo ();
	}

	public override void OnHide ()
	{
		playerList.Clear ();
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		switch ((EventId)eventId) {
	    case EventId.OnFriendLoadResult:
            {
                // 数据列表
                List<SimplePlayerData> datas;
                if (selectTab == myfollowTab)
                    datas = FriendDataHandler.Get().myFollowList;
                else
                    datas = FriendDataHandler.Get().followerList;


                guanzhuLabel.text = string.Format("({0})", FriendDataHandler.Get().myFollowList.Count);
                fensiLabel.text = string.Format("({0})", FriendDataHandler.Get().followerStatus.Count);
                // 数据列表状态
                List<bool> followStatus;
                if (selectTab == myfollowTab)
                    followStatus = FriendDataHandler.Get().myFollowStatus;
                else
                    followStatus = FriendDataHandler.Get().followerStatus;
                grid.transform.DestroyChildren();
                RefreshScrollView(datas, followStatus, false);
            }
            break;
       
		case EventId.OnFriendSearchResult:
			{
				SimplePlayerData data = args [0] as SimplePlayerData;
				bool follow    = (bool)args [1];
                int myfollow   = (int)args[2];
                int myfensi    = (int)args[3];
                int mvp        = (int)args[4];
                int battlenum  = (int)args[5];
				// 打开弹出框
				if (data != null) {
					// 打开界面
                    // UISystem.Get().HideWindow("FriendWindow");
					UISystem.Get ().ShowWindow("FriendFindWindow");
                    EventSystem.Instance.FireEvent(EventId.OnFriendSearchResultShow, data, follow, myfollow, myfensi, mvp, battlenum, false );
				} else {
                    inputPage.SetActive(true);
                    inputComp.value = "";
					Tips.Make (Tips.TipsType.FlowUp, "未搜索到玩家", 1);
				}

			}
			break;

        case EventId.OnFriendFollowResult:
            {
                // 关注好友结果
                int userId  = (int)args[0];
                bool follow = (bool)args[1]; // 关注或者取消关注行动
                NetMessage.ErrCode errCode = (NetMessage.ErrCode)args[2];

                guanzhuLabel.text   = string.Format("({0})", FriendDataHandler.Get().myFollowList.Count);
                fensiLabel.text     = string.Format("({0})", FriendDataHandler.Get().followerStatus.Count);

                if (errCode == NetMessage.ErrCode.EC_Ok)
                {
                    Transform t = grid.transform.Find("infomation_" + userId);
                    FriendWindowCell cell = null;
                    if (t != null)
                    {
                        cell = t.gameObject.GetComponent<FriendWindowCell>();
                        cell.OnCareResult(follow);
                    }
                    else
                    {
                        Debug.Log("OnFriendFollowResult   找不到子节点");
                    }

                    if (selectTab == followerTab && t != null )
                    {
                        SimplePlayerData spd = null;
                        for (int i = 0; i < FriendDataHandler.Get().myFollowList.Count; ++i)
                        {
                            if (FriendDataHandler.Get().myFollowList[i].userId == userId)
                                spd = FriendDataHandler.Get().myFollowList[i];
                        }

                        bool IsXianghu = FriendDataHandler.Get().IsIsFollowEX(spd.userId);
                        cell.SetFollowerInfo(spd, IsXianghu);
                    }


                    if (selectTab == myfollowTab && follow == false)
                    {
                        SimplePlayerData spd = null;
                        for (int i = 0; i < FriendDataHandler.Get().myFollowList.Count; ++i)
                        {
                            if (FriendDataHandler.Get().myFollowList[i].userId == userId)
                                spd = FriendDataHandler.Get().myFollowList[i];
                        }

                        if (spd != null)
                        {
                            FriendDataHandler.Get().DelMyFollow(spd);
                        }

                        if (t != null)
                        {
                            grid.RemoveChild(t);
                            t.transform.SetParent(null);
                            Destroy(t.gameObject);
                            grid.Reposition();
                        }
                    }
                    else if (selectTab == myfollowTab && follow)
                    {
                        SimplePlayerData spd = null;
                        for (int i = 0; i < FriendDataHandler.Get().myFollowList.Count; ++i)
                        {
                            if (FriendDataHandler.Get().myFollowList[i].userId == userId)
                                spd = FriendDataHandler.Get().myFollowList[i];
                        }
                        GameObject go = null;
                        if (t == null)
                        {
                            go = NGUITools.AddChild(grid.gameObject, infoTemplate);
                            go.name = "infomation_" + spd.userId;
                            go.SetActive(true);
                        }
                        cell = go.GetComponent<FriendWindowCell>();
                        if (FriendDataHandler.Get().IsIsFollowEX(spd.userId))
                        {
                            cell.SetFollowerInfo(spd, true);
                        }
                        else
                        {
                            cell.SetFollowerInfo(spd, false);

                        }

                        grid.Reposition();
                    }
                }
                else
                {
                    string t = follow ? DictionaryDataProvider.GetValue(809) : DictionaryDataProvider.GetValue(810); 
                    t = string.Format("{0} userId: {1} 失败！", t, userId);
                    Tips.Make(Tips.TipsType.FlowUp, t, 1);
                }
            }
            break;

            case EventId.OnFriendNotifyResult:
            {
                // 取消和关注我
                guanzhuLabel.text   = string.Format("({0})", FriendDataHandler.Get().myFollowList.Count);
                fensiLabel.text     = string.Format("({0})", FriendDataHandler.Get().followerStatus.Count);

                SimplePlayerData data = args[0] as SimplePlayerData;
                bool follow           = (bool)args[1];
                int userId            = data.userId;

                Transform t = grid.transform.Find("infomation_" + userId);
                FriendWindowCell cell = null;
                if (t != null)
                {
                    cell = t.gameObject.GetComponent<FriendWindowCell>();
                    cell.OnCareResult(follow);
                }


                if (selectTab == myfollowTab )
                {
                    // 刷新关注列表显示
                   
                    if (t != null)
                    {
                        bool IsXianghu = FriendDataHandler.Get().IsIsFollowEX(data.userId);
                        cell.SetFollowerInfo(data, IsXianghu);
                    }
                }

                if (selectTab == followerTab && !follow )
                {
                    if (t != null)
                    {
                        grid.RemoveChild(t);
                        t.transform.SetParent(null);
                        Destroy(t.gameObject);
                        grid.Reposition();
                    }
                }
                else if( selectTab == followerTab && follow )
                {
                    GameObject go = null;
                    if (t == null)
                    {
                        go = NGUITools.AddChild(grid.gameObject, infoTemplate);
                        go.name = "infomation_" + data.userId;
                        go.SetActive(true);
                    }
                    cell = go.GetComponent<FriendWindowCell>();
                    if (FriendDataHandler.Get().IsMyFollowEX(data.userId))
                    {
                        cell.SetFollowerInfo(data, true);
                    }
                    else
                    {
                        cell.SetFollowerInfo(data, false);
                       
                    }

                    grid.Reposition();
                }
            }
            break;
		}
	}

	private void SetInfo()
	{

	}

	private void SearchFriend(string text)
	{
		if (string.IsNullOrEmpty (text)) {
			return;
		}
		Debug.Log ("二维码扫描结果：" + text);
		int id = 0;
		if (int.TryParse (text, out id)) {

			UISystem.Get ().HideWindow ("ScanWindow");

			NetSystem.Instance.helper.FriendSearch("", id);
		}
	}

	/// <summary>
	/// 点击输入
	/// </summary>
	public void OnInputClick()
	{
		inputPage.SetActive (true);
		inputComp.SendMessage ("OnPress", true, SendMessageOptions.RequireReceiver);
	}

	public void OnInputOKClick()
	{
		string text = inputComp.value;
		if (string.IsNullOrEmpty (text)) {
			return;
		}

        //inputPage.SetActive(false);
		NetSystem.Instance.helper.FriendSearch (text, 0);
	}

	/// <summary>
	/// 扫一扫
	/// </summary>
	public void OnScanClick()
	{
        //Tips.Make(Tips.TipsType.FlowUp, DictionaryDataProvider.GetValue(602), 1.0f);
        //return;
		//打开scan界面
		Coroutine.Start(DoScan());
	}

	private IEnumerator DoScan()
	{
		yield return Application.RequestUserAuthorization (UserAuthorization.WebCam);
		if (Application.HasUserAuthorization (UserAuthorization.WebCam)) {
            UISystem.Get().HideAllWindow();
			UISystem.Get ().ShowWindow ("ScanWindow");
		} else {
			Tips.Make (Tips.TipsType.FlowUp, "没有摄像头权限", 1.0f);
		}
	}

	/// <summary>
	/// 摇一摇
	/// </summary>
	public void OnShakeClick()
	{
		
	}

	/// <summary>
	/// 信息
	/// </summary>
	public void OnTopMessageClick()
	{
		
	}

	/// <summary>
	/// 关闭界面
	/// </summary>
	public void OnCloseClick()
	{
		UISystem.Get ().HideWindow ("FriendWindow");
        UISystem.Get().ShowWindow("CustomSelectWindowNew");
	}

	private void OnRecommendFriendsShowMore()
	{
		//NetSystem.Instance.helper.FriendRecommend (playerList.Count);
        if (selectTab == followerTab)
        {
            NetSystem.Instance.helper.FriendLoad(FriendDataHandler.Get().followerList.Count, false);
        }
        else if (selectTab == myfollowTab)
        {
            NetSystem.Instance.helper.FriendLoad(FriendDataHandler.Get().myFollowList.Count, true);
        }
	}


    
    private void OnTabClick(GameObject go)
    {
        if (selectTab != null && go == selectTab.gameObject)
        {
            return;
        }

        Color unselectcolor = new Color(1, 1, 1, 0.3f);
        followerTab.color = unselectcolor;
        myfollowTab.color = unselectcolor;

        selectTab = go.GetComponent<UILabel>();
        selectTab.color = Color.white;
        grid.transform.DestroyChildren();
        if (selectTab == followerTab)
        {
            RefreshScrollView(FriendDataHandler.Get().followerList, FriendDataHandler.Get().followerStatus, true);
        }
        else
        {
            RefreshScrollView(FriendDataHandler.Get().myFollowList, FriendDataHandler.Get().myFollowStatus, true);
        }
        AudioManger.Get().PlayEffect("onClick");
    }

    private void RefreshScrollView(List<SimplePlayerData> data, List<bool> status, bool useOldData)
    {

        for (int i = 0; i < data.Count; ++i)
        {
            GameObject go = NGUITools.AddChild(grid.gameObject, infoTemplate);
            go.name = "infomation_" + data[i].userId;
            go.SetActive(true);
            FriendWindowCell cell = go.GetComponent<FriendWindowCell>();
            if (selectTab == myfollowTab)
            {
                cell.SetMyFollowInfo(data[i], status[i]);
            }
            else
            {
                cell.SetFollowerInfo(data[i], status[i] );
            }
        }

        grid.Reposition();
        scrollView.ResetPosition();
    }
}

