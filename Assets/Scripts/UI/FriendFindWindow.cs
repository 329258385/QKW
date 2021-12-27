using System;
using UnityEngine;
using Solarmax;

public class FriendFindWindow : BaseWindow
{
	public UILabel scoreLabel;
	public UILabel nameLabel;
    public UILabel guanzhu;
    public UILabel fensi;

	public UISprite icon;
	public UIButton followBtn;
	public UILabel followBtnLabel;

    public UILabel changci;
    public UILabel Mvp;
    public UILabel shenglv;


    private bool IsFromMainUI = false;
    private int curUserID = -1;
	private SimplePlayerData playerData = null;


    private int cellType = 0; //0  相关关注， 1 已关注；2 ；需要关注
	public override bool Init ()
	{
		RegisterEvent (EventId.OnFriendSearchResultShow);
		RegisterEvent (EventId.OnFriendFollowResult);

		return true;
	}

	public override void OnShow ()
	{

	}

	public override void OnHide ()
	{
        curUserID = -1;
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
      
		if (eventId == EventId.OnFriendSearchResultShow) {
			playerData      = args [0] as SimplePlayerData;
			//bool follow     = (bool)args [1];
            int myfollow    = (int)args[2];
            int myfensi     = (int)args[3];
            int mvpnum      = (int)args[4];
            int battlenum   = (int)args[5];
            IsFromMainUI   = (bool)args[6];
            curUserID       = playerData.userId;
			scoreLabel.text = playerData.score.ToString ();
			nameLabel.text  = playerData.name;
            guanzhu.text    = myfollow.ToString();
            fensi.text      = myfensi.ToString();
            if( battlenum <= 0 )
            {
                shenglv.text = "0";
                changci.text = "0";
            }
            else
            {
                changci.text    = battlenum.ToString(); ;
                Mvp.text        = mvpnum.ToString();
                double fValue   = mvpnum / (battlenum * 1.0) * 100.0f;
                shenglv.text    = Math.Round(fValue, 1).ToString() + "%";
            }
			icon.spriteName = playerData.icon;

            bool bfollow    = FriendDataHandler.Get().IsIsFollowEX(playerData.userId);
            bool bmyfollow  = FriendDataHandler.Get().IsMyFollowEX(playerData.userId);
            if (bfollow && bmyfollow) 
            {
                // 相关关注
                cellType = 0;
                followBtn.isEnabled = true;
                followBtnLabel.text = DictionaryDataProvider.GetValue(807);
			}
            else if (bmyfollow)
            {
                // 已经关注
                cellType = 1;
				followBtn.isEnabled = true;
                followBtnLabel.text = DictionaryDataProvider.GetValue(808);
			}
            else
            {
                // 需要关注
                cellType = 2;
                followBtn.isEnabled = true;
                followBtnLabel.text = DictionaryDataProvider.GetValue(806);
            }

            if( playerData.userId ==  LocalPlayer.Get().playerData.userId )
            {
                followBtn.gameObject.SetActive(false);
            }

		} 
        else if (eventId == EventId.OnFriendFollowResult) {
			// 关注好友结果
			int userId = (int)args [0];
			bool follow = (bool)args [1]; // 关注或者取消关注行动
			NetMessage.ErrCode errCode = (NetMessage.ErrCode)args [2];

			if (userId != playerData.userId)
				return;

			if (errCode == NetMessage.ErrCode.EC_Ok) {
				if (follow) {
					followBtn.isEnabled = false;
                    followBtnLabel.text = DictionaryDataProvider.GetValue(808);
				} else {
					followBtn.isEnabled = true;
                    followBtnLabel.text = DictionaryDataProvider.GetValue(806);
				}
			} else {
                string t = follow ? DictionaryDataProvider.GetValue(809) : DictionaryDataProvider.GetValue(810);
				t = string.Format ("{0} userId: {1} 失败！", t, userId);
				Tips.Make (Tips.TipsType.FlowUp, t, 1);
			}
		}
	}

	public void OnFollowClick()
	{
        if (cellType == 0 || cellType == 1)
        {
            UISystem.Instance.ShowWindow("CommonDialogWindow");
            string info = string.Format(DictionaryDataProvider.GetValue(812), nameLabel.text);
            EventSystem.Instance.FireEvent(EventId.OnCommonDialog, 3, info, new EventDelegate(FollowClick));
        }

        if (cellType == 2)
        {
            NetSystem.Instance.helper.FriendFollow(playerData.userId, true);
            UISystem.Get().HideWindow("FriendFindWindow");
            if (IsFromMainUI)
            {
                UISystem.Get().ShowWindow("CustomSelectWindowNew");
            }
        }
	}

    void FollowClick()
    {
        if (cellType == 0 || cellType == 1)
            NetSystem.Instance.helper.FriendFollow(playerData.userId, false);
        if (cellType == 2)
            NetSystem.Instance.helper.FriendFollow(playerData.userId, true);

        UISystem.Get().HideWindow("FriendFindWindow");
        if (IsFromMainUI)
        {
            UISystem.Get().ShowWindow("CustomSelectWindowNew");
        }
    }

	public void OnCloseClick()
	{
		UISystem.Get ().HideWindow ("FriendFindWindow");
        if (IsFromMainUI)
        {
            UISystem.Get().ShowWindow("CustomSelectWindowNew");
        }
        else
        {
            UISystem.Get().ShowWindow("FriendWindow");
        }
	}

    public void OnCustomPlayerHead()
    {

        int userID = LocalPlayer.Get().playerData.userId;
        if (userID == curUserID)
        {
            UISystem.Get().HideAllWindow();
            UISystem.Get().ShowWindow("SelectHeadWindow");
        }
       
    }
}

