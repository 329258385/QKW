using System;
using Solarmax;

public class SingleClearWindow : BaseWindow
{
	public UIInput inputField;

	public override bool Init ()
	{
		RegisterEvent (EventId.OnRename);

		return true;
	}

	public override void OnShow ()
	{

	}
	
	public override void OnHide ()
	{

	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.OnRename)
		{
			NetMessage.ErrCode code = (NetMessage.ErrCode)args [0];

			if (code == NetMessage.ErrCode.EC_Ok) {
				// 成功
				LocalPlayer.Get ().playerData.name = inputField.value;

				// 进入PVP界面
				UISystem.Get ().HideWindow ("SingleClearWindow");
				EventSystem.Instance.FireEvent (EventId.OnRenameFinished);

			} else if (code == NetMessage.ErrCode.EC_NameExist) {
				Tips.Make ("用户名重复!");
			} else if (code == NetMessage.ErrCode.EC_InvalidName) {
				Tips.Make ("用户名不合法!");
			} else if (code == NetMessage.ErrCode.EC_AccountExist) {
				Tips.Make ("账号重复！");
			} else {
				Tips.Make ("用户名错误！");
			}

		}

	}

	public void OnEnterNameValueChanged()
	{
		string name = inputField.value.Trim ();
		name = name.Replace ('\r', ' ');
		name = name.Replace ('\t', ' ');
		name = name.Replace ('\n', ' ');

#if UNITY_EDITOR
		while (EncodingTextLength (name) > 12) {
#else
		while (EncodingTextLength (name) > 12) {
#endif
			name = name.Substring (0, name.Length - 1);
		}

		inputField.value = name;
	}

	private int EncodingTextLength(string s)
	{
		int ret = 0;
		byte[] b;
		for (int i = 0; i < s.Length; ++i) {
			#if UNITY_EDITOR
			b = System.Text.Encoding.UTF8.GetBytes (s.Substring (i, 1));
			#else
			b = System.Text.Encoding.UTF8.GetBytes (s.Substring (i, 1));
			#endif
			ret += (b.Length > 1 ? 2 : 1);
		}

		return ret;
	}

	public void OnRandNameClick()
	{
		inputField.value = AIManager.GetAIName(UnityEngine.Time.frameCount);
	}

	public void OnConfirmClick()
	{
		string name = inputField.value;
		if (string.IsNullOrEmpty (name)) {

			Tips.Make ("用户名不能为空!");

			return;
		}

		NetSystem.Instance.helper.ChangeName (name);
	}

	public void OnCloseClick()
	{
		Tips.Make ("这个按钮干啥活儿呢？");
	}
}
