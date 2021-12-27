using System;
using UnityEngine;
using Solarmax;

public class Ping : MonoBehaviour
{
	/// <summary>
	/// 状态图片
	/// </summary>
	public UISprite pingPic;
	/// <summary>
	/// 显示字的label
	/// </summary>
	public UILabel pingValue;

	private float appPauseBeginTime;

	private void Start()
	{

		pingValue.gameObject.SetActive (false);
		UIEventListener eventListener = pingPic.gameObject.GetComponent<UIEventListener> ();
		eventListener.onClick = OnClickPic;

		Invoke ("RegistEvent", 0.5f);

		SetPicAndValue ();
	}

	void RegistEvent()
	{
		EventSystem.Instance.RegisterEvent (EventId.NetworkStatus, this, null, OnEventHandler);
		EventSystem.Instance.RegisterEvent (EventId.PingRefresh, this, null, OnEventHandler);
	}

	/// <summary>
	/// 事件处理
	/// </summary>
	private void OnEventHandler(int eventId, object data, params object[] args)
	{
		if (eventId == (int)EventId.NetworkStatus) {
			bool status = (bool)args [0];

			SetPicAndValue ();

			if (!status) {
				pingValue.text = DictionaryDataProvider.GetValue (503);
			}

		} else if (eventId == (int)EventId.PingRefresh) {
			
			SetPicAndValue ();
		}
	}

	public void OnClickPic(GameObject go)
	{
		if (pingValue.gameObject.activeSelf) {
			CancelInvoke ("AutoHidePingValue");
		}

		SetPicAndValue ();

		if (NetSystem.Instance.GetConnector ().GetConnectStatus () != ConnectionStatus.CONNECTED) {
			pingValue.text = DictionaryDataProvider.GetValue (503);
			pingValue.color = Color.red;
		}


		pingValue.gameObject.SetActive (true);

		Invoke ("AutoHidePingValue", 3.0f);
	}

	public void AutoHidePingValue()
	{
		pingValue.gameObject.SetActive (false);
	}

	private void SetPicAndValue ()
	{
		string pic;
		Color color;
		NetSystem.Instance.ping.GetNetPic (out pic, out color);

		pingPic.spriteName = pic;
		float time = NetSystem.Instance.ping.lastPingTime;
		pingValue.text = string.Format (DictionaryDataProvider.GetValue(504), Mathf.RoundToInt(time));
		pingValue.color = color;
	}
}

