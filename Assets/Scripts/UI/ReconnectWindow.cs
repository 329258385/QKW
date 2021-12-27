using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Solarmax;

public class ReconnectWindow : BaseWindow {

	public UILabel tips;

	private float waitBeginTime;

	public override bool Init ()
	{
		RegisterEvent (EventId.RequestUserResult);
		
		return true;
	}

	public override void OnShow ()
	{
		// 0.5s后开始重连
		Invoke ("Reconnect", 0.5f);
		waitBeginTime = Time.realtimeSinceStartup;

		tips.text = DictionaryDataProvider.GetValue (501);

		InvokeRepeating ("UpdateTips", 1.0f, 1.0f);
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.RequestUserResult) {
			// 重连后获取玩家数据

			NetMessage.ErrCode code = (NetMessage.ErrCode)args [0];
			if (code == NetMessage.ErrCode.EC_Ok) {

				// 此时发送一个重连OK的事件，用于界面刷新

				EventSystem.Instance.FireEvent (EventId.ReconnectResult);

				UISystem.Instance.HideWindow ("ReconnectWindow");

			} else if (code == NetMessage.ErrCode.EC_NoExist) {

				Tips.Make (Tips.TipsType.FlowUp, "code:EC_NoExist   error on this situation.", 1.0f);
			} else if (code == NetMessage.ErrCode.EC_NeedResume) {

				EventSystem.Instance.FireEvent (EventId.ReconnectResult);

				UISystem.Instance.HideWindow ("ReconnectWindow");
			}
		}
	}

	public override void OnHide ()
	{
		
	}

	public void UpdateTips ()
	{
		int seconds = Mathf.RoundToInt(Time.realtimeSinceStartup - waitBeginTime);
		tips.text = string.Format (DictionaryDataProvider.GetValue (502), seconds);
	}


	private void Reconnect ()
	{
		Coroutine.Start (Login ());
	}

	private IEnumerator Login ()
	{
		yield return NetSystem.Instance.helper.ConnectServer ();

		if (NetSystem.Instance.GetConnector ().GetConnectStatus () == ConnectionStatus.CONNECTED) {

			NetSystem.Instance.helper.RequestUser ();
		} else {

			yield return new WaitForSeconds (3.0f);
			Coroutine.Start (Login ());
		}
	}
}
