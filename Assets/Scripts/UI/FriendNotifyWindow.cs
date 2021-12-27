using System;
using Solarmax;

public class FriendNotifyWindow : BaseWindow
{
	public NetTexture icon;
	public UILabel notice;
	public UILabel score;
	public UIButton careBtn;
	public UIButton ignoreBtn;

	private SimplePlayerData data;

	public override bool Init ()
	{
		RegisterEvent (EventId.OnFriendNotifyResult);


		return true;
	}

	public override void OnShow()
	{
		gameObject.SetActive (false);
		Invoke ("AutoHide", 3);
	}

	public override void OnHide ()
	{

	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.OnFriendNotifyResult) {
			data = (SimplePlayerData) args [0];
			//bool follow = (bool) args [1];

			icon.picUrl = data.icon;
			notice.text = string.Format ("{0}关注了你", data.name);
			score.text = data.score.ToString ();

			gameObject.SetActive (true);
		}
	}

	public void OnCareClick()
	{
		NetSystem.Instance.helper.FriendFollow (data.userId, true);
		OnIgnoreClick ();
	}

	public void OnIgnoreClick()
	{
		CancelInvoke ("AutoHide");
		AutoHide ();
	}

	private void AutoHide()
	{
		UISystem.Get ().HideWindow ("FriendNotifyWindow");
	}
}

