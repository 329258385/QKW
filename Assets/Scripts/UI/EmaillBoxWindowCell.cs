using System;
using UnityEngine;
using Solarmax;

public class EmaillBoxWindowCell : MonoBehaviour
{
    public UISprite     icon;
	public UILabel      nameLabel;
	public UILabel      onlineLabel;
    public UILabel      descLabel;
	public UILabel      scoreLabel;
	public UIButton     careBtn;
	




	public SimplePlayerData playerData;

	
	/// <summary>
	/// 设置粉丝页信息
	/// </summary>
	/// <param name="data">Data.</param>
	/// <param name="cared">If set to <c>true</c> cared.</param>
	public void SetEamilBoxInfo(SimplePlayerData data )
	{
		playerData = data;

		icon.spriteName = playerData.icon;
		nameLabel.text = playerData.name;
		scoreLabel.text = playerData.score.ToString ();
        {
            onlineLabel.text = DictionaryDataProvider.GetValue(805); 
        }
		careBtn.gameObject.SetActive (true);
	}

	/// <summary>
	/// cell关注按钮点击
	/// </summary>
	public void OnCareClick()
	{
	    NetSystem.Instance.helper.FriendFollow (playerData.userId, true);
	}

	/// <summary>
	/// cell关注点击后的返回
	/// </summary>
	/// <param name="cared">If set to <c>true</c> cared.</param>
	public void OnCareResult(bool cared)
	{
		
	}
}

