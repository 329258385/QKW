using System;
using UnityEngine;
using Solarmax;

public class FriendWindowCell : MonoBehaviour
{
    public UISprite icon;
	public UILabel nameLabel;
	public UISprite onlineBg;
	public UILabel onlineLabel;
	public UILabel scoreLabel;
	public UIButton careBtn;
	public UILabel careBtnLabel;
	//public UILabel emptyLabel;
	//public GameObject onlineMark;

	public SimplePlayerData playerData;

	private int cellType = 0; //1，推荐信息；2，粉丝页；3，我的关注页；

	/// <summary>
	/// 设置推荐信息，myfollow：true，表示已关注
	/// </summary>
	/// <param name="data">Data.</param>
	/// <param name="myFollow">If set to <c>true</c> my follow.</param>
	public void SetRecommendInfo(SimplePlayerData data, bool cared)
	{
		cellType = 1;
		playerData = data;

		icon.spriteName = playerData.icon;
		nameLabel.text = playerData.name;

		{
			onlineBg.spriteName = "gray";
            onlineLabel.text = DictionaryDataProvider.GetValue(805);
		}

		scoreLabel.text = playerData.score.ToString ();

		if (cared) {
			careBtn.isEnabled = false;
            careBtnLabel.text = DictionaryDataProvider.GetValue(807);
		} else {
			careBtn.isEnabled = true;
            careBtnLabel.text = DictionaryDataProvider.GetValue(806);
		}

	}

	/// <summary>
	/// 设置粉丝页信息
	/// </summary>
	/// <param name="data">Data.</param>
	/// <param name="cared">If set to <c>true</c> cared.</param>
	public void SetFollowerInfo(SimplePlayerData data, bool cared)
	{
		cellType = 2;
		playerData = data;

		icon.spriteName = playerData.icon;
		nameLabel.text = playerData.name;
		scoreLabel.text = playerData.score.ToString ();

        {
            onlineBg.spriteName = "gray";
            onlineLabel.text = DictionaryDataProvider.GetValue(805); 
        }
		careBtn.gameObject.SetActive (true);
		careBtnLabel.gameObject.SetActive (true);
		if (cared) {
			careBtn.isEnabled = false;
            careBtnLabel.text = DictionaryDataProvider.GetValue(807); 
		} else {
			careBtn.isEnabled = true;
            careBtnLabel.text = DictionaryDataProvider.GetValue(806); 
		}
		//onlineMark.SetActive (playerData.online);
	}

	/// <summary>
	/// 设置我的关注页信息
	/// </summary>
	/// <param name="data">Data.</param>
	public void SetMyFollowInfo(SimplePlayerData data, bool cared )
	{
		cellType = 3;
		playerData = data;

		icon.spriteName = playerData.icon;
		nameLabel.text = playerData.name;
		scoreLabel.text = playerData.score.ToString ();
		careBtn.isEnabled = true;
        {
            onlineBg.spriteName = "gray";
            onlineLabel.text = DictionaryDataProvider.GetValue(805); 
        }
        if (cared)
        {
            careBtnLabel.text = DictionaryDataProvider.GetValue(807); 
        }
        else
        {
            careBtnLabel.text = DictionaryDataProvider.GetValue(808); 
        }
		//onlineMark.SetActive (playerData.online);
	}

	/// <summary>
	/// cell关注按钮点击
	/// </summary>
	public void OnCareClick()
	{
        /*
		if (cellType == 1) {
			NetSystem.Instance.helper.FriendFollow (playerData.userId, true);
		} else if (cellType == 2) {
			NetSystem.Instance.helper.FriendFollow (playerData.userId, true);
		} else if (cellType == 3) {
			NetSystem.Instance.helper.FriendFollow (playerData.userId, false);
		}
        */
	}


    public void OnShowPlayerInfoClick()
    {
        if (playerData != null)
        {
            NetSystem.Instance.helper.FriendSearch("", playerData.userId);
        }
    }


	/// <summary>
	/// cell关注点击后的返回
	/// </summary>
	/// <param name="cared">If set to <c>true</c> cared.</param>
	public void OnCareResult(bool cared)
	{
		if (cellType == 1) {
			if (cared) {
				careBtn.isEnabled = false;
                careBtnLabel.text = DictionaryDataProvider.GetValue(808); 
			}
		} else if (cellType == 2) {
			if (cared) {
				careBtn.isEnabled = false;
                careBtnLabel.text = DictionaryDataProvider.GetValue(808); 
			}
		} else if (cellType == 3) {
			if (!cared) {
				careBtn.isEnabled = false;
                careBtnLabel.text = DictionaryDataProvider.GetValue(811); 
			}
		}
	}
}

