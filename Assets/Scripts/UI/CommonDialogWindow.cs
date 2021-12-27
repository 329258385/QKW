using System;
using UnityEngine;

public class CommonDialogWindow : BaseWindow
{
	// 确认1
	public UIButton yesBtn1;
	public UILabel yesBtnLabel1;
	// 两种选择，确认取消
	public UIButton yesBtn2;
	public UILabel yesBtnLabel2;
	public UIButton noBtn2;
	public UILabel noBtnLabel2;
	// 提示字
	public UILabel tips;

	private int type;

	private EventDelegate onYes;
	private EventDelegate onNo;

	private int yesCountDown = 0; // 确认按钮倒计时

	public override bool Init ()
	{
		RegisterEvent (EventId.OnCommonDialog);

		return true;
	}

	public override void OnShow ()
	{
		this.gameObject.SetActive (false);
	}

	public override void OnHide ()
	{
		onYes = null;
		onNo = null;

		yesBtn1.gameObject.SetActive (false);
		yesBtn2.gameObject.SetActive (false);
		noBtn2.gameObject.SetActive (false);
	}

	private void InvokeYes2CountDown()
	{
		if (yesCountDown <= 0) {
			yesBtnLabel2.text = "确定";
			yesBtn2.isEnabled = true;
			CancelInvoke ("InvokeYes2CountDown");
			return;
		}
		
		yesBtnLabel2.text = string.Format ("确定({0})", yesCountDown);
		--yesCountDown;
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.OnCommonDialog) {
			type = (int)args [0];
			string str = (string)args [1];

			if (type == 1) {
				// 单个确定按钮
				if (args.Length > 2) {
					onYes = (EventDelegate)args [2];
				}
			} else if (type == 2) {
				// 两个按钮，确定，取消
				if (args.Length > 2) {
					onYes = (EventDelegate)args [2];
				}
				if (args.Length > 3) {
					onNo = (EventDelegate)args [3];
				}
			} else if (type == 3) {
				// 两个按钮，确定按钮带倒计时
				if (args.Length > 2) {
					onYes = (EventDelegate)args [2];
				}
				if (args.Length > 3) {
					onNo = (EventDelegate)args [3];
				}
				if (args.Length > 4) {
					yesCountDown = (int)args [4];
				}
			} else if (type == 4) {
				// 单确定按钮，点击确定不关闭窗口
				if (args.Length > 2) {
					onYes = (EventDelegate)args [2];
				}
			}

			// 设置信息
			SetInfo (type, str);
		}
	}

	private void SetInfo(int type, string str)
	{
		yesBtn1.gameObject.SetActive (false);
		yesBtn2.gameObject.SetActive (false);
		noBtn2.gameObject.SetActive (false);
		
		if (type == 1) {
			yesBtn1.gameObject.SetActive (true);
		} else if (type == 2) {
			yesBtn2.gameObject.SetActive (true);
			noBtn2.gameObject.SetActive (true);
		} else if (type == 3) {
			yesBtn2.gameObject.SetActive (true);
			noBtn2.gameObject.SetActive (true);
			if (yesCountDown > 0) {
				yesBtn2.isEnabled = false;
				InvokeRepeating ("InvokeYes2CountDown", 0, 1.0f);
			}
		} else if (type == 4) {
			yesBtn1.gameObject.SetActive (true);
		}

		tips.text = str;

		this.gameObject.SetActive (true);
	}

	public void OnYesClick()
	{
		if (onYes != null) {
			onYes.Execute ();
		}

		if (type != 4) {
			UISystem.Get ().HideWindow ("CommonDialogWindow");
		}
	}

	public void OnNoClick()
	{
		if (onNo != null) {
			onNo.Execute ();
		}

		UISystem.Get ().HideWindow ("CommonDialogWindow");
	}

	public void OnClose()
	{
		if (yesBtn2.gameObject.activeSelf && noBtn2.gameObject.activeSelf) {
			// 双向
			OnNoClick ();
		} else if (yesBtn1.gameObject.activeSelf) {
			OnYesClick ();
		}
	}
}

