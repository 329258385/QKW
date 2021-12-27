using System;
using UnityEngine;
using Solarmax;

public class CreateRoomWindow : BaseWindow
{
	public GameObject upGo;
	public GameObject centerGo;
	public GameObject numPage;
	public UILabel[] numPageValues;

	private int numPageEnterIndex = 0;

	private string lastEnterRoomId = string.Empty;

	public override bool Init ()
	{
		RegisterEvent (EventId.OnStartMatchResult);

		return true;
	}
	
	public override void OnShow ()
	{
		upGo.SetActive (true);
		centerGo.SetActive (true);
		numPage.SetActive (false);
	}

	public override void OnHide ()
	{
		
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.OnStartMatchResult) {
			NetMessage.ErrCode code = (NetMessage.ErrCode)args [0];
			if (code == NetMessage.ErrCode.EC_Ok) {
				// 关闭当前页，打开等待页
				UISystem.Instance.HideWindow ("CreateRoomWindow");
				UISystem.Instance.ShowWindow ("RoomWaitWindow");

			} else if (code == NetMessage.ErrCode.EC_MatchIsFull) {
				Tips.Make (Tips.TipsType.FlowUp, DictionaryDataProvider.GetValue (902), 1.0f);
			} else if (code == NetMessage.ErrCode.EC_MatchIsMember) {
				Tips.Make (Tips.TipsType.FlowUp, DictionaryDataProvider.GetValue (903), 1.0f);
			} else if (code == NetMessage.ErrCode.EC_NotInMatch) {
				Tips.Make (Tips.TipsType.FlowUp, DictionaryDataProvider.GetValue (904), 1.0f);
			} else if (code == NetMessage.ErrCode.EC_NotMaster) {
				Tips.Make (Tips.TipsType.FlowUp, DictionaryDataProvider.GetValue (905), 1.0f);
			} else if (code == NetMessage.ErrCode.EC_RoomNotExist) {
				Tips.Make (Tips.TipsType.FlowUp, DictionaryDataProvider.GetValue (906), 1.0f);
			} else {
				Tips.Make (Tips.TipsType.FlowUp, DictionaryDataProvider.Format (901, code), 1.0f);
			}
		}
	}

	public void OnFourClick ()
	{
		NetSystem.Instance.helper.StartMatchReq (NetMessage.MatchType.MT_Room, string.Empty, false);
	}

	public void OnOneClick ()
	{
		Tips.Make (Tips.TipsType.FlowUp, DictionaryDataProvider.GetValue(602), 1.0f);
		return;

		//NetSystem.Instance.helper.StartMatchReq (NetMessage.MatchType.MT_Room, string.Empty, false);
	}

	public void OnTwoClick ()
	{
		Tips.Make (Tips.TipsType.FlowUp, DictionaryDataProvider.GetValue(602), 1.0f);
		return;

		//NetSystem.Instance.helper.StartMatchReq (NetMessage.MatchType.MT_Room, string.Empty, false);
	}

	public void OnAddClick ()
	{
		// 打开输入框
		numPage.SetActive (true);
		numPageEnterIndex = 0;
	}

	public void OnNumPadClick ()
	{
		string name = UIButton.current.gameObject.name;
		int num = 0;
		if (!int.TryParse (name, out num)) {
			return;
		}

		if (num >= 0) {

			if (numPageEnterIndex < 4) {
				numPageValues [numPageEnterIndex].text = name;
				++numPageEnterIndex;
			}
		} else {
			// delete
			if (numPageEnterIndex > 0){

				--numPageEnterIndex;
				numPageValues [numPageEnterIndex].text = string.Empty;
			}
		}

		if (numPageEnterIndex >= 4) {
			// 加入房间

			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			for (int i = 0; i < numPageValues.Length; ++i) {
				sb.Append (numPageValues [i].text);
			}
			string roomId = sb.ToString ();

			if (lastEnterRoomId.Equals (roomId)) {
				return;
			}
			lastEnterRoomId = roomId;

			NetSystem.Instance.helper.StartMatchReq (NetMessage.MatchType.MT_Room, roomId, false);
		}
	}

	public void OnNumPageClose ()
	{
		numPage.SetActive (false);
		upGo.SetActive (true);
		centerGo.SetActive (true);
	}

	public void OnBackClick ()
	{
		UISystem.Instance.HideWindow ("CreateRoomWindow");
		UISystem.Instance.ShowWindow ("CustomSelectWindowNew");
	}
}

