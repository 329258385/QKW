using System;
using UnityEngine;
using Solarmax;

public class SelectIconWindow : BaseWindow
{
	public GameObject[] icons;
	//public Animator windowAni;

	public Color unselectColor = new Color(1, 1, 1, 0.6f);
	public Color selectColor1 = new Color(0.196f, 0.929f, 0.525f, 1.0f);

	private GameObject selectGo = null;
	private int selectIndex = -1;

	public void Awake()
	{
		//绑定
		for (int i = 0; i < icons.Length; ++i) {
			GameObject go = icons [i];
			go.GetComponent<UIEventListener> ().onClick += OnIconClick;
		}
	}

	public override void OnShow()
	{
		// 全部取消选择
		for (int i = 0; i < icons.Length; ++i)
		{
			GameObject go = icons [i];
			go.transform.Find ("block").GetComponent<UISprite> ().color = unselectColor;
		}

		// 显示当前的
		string icon = LocalPlayer.Get().playerData.icon;
		if (!icon.StartsWith ("http")) {

            int len     = icon.LastIndexOf('_');
            string i    = icon.Substring(len + 1);
			int index   = int.Parse(i) - 1;
			Select (index);
		}

		//windowAni.Play ("SelectHeadWindow_in");
	}


	public override void OnHide()
	{
		
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		
	}

	private void Select (int index)
	{
		if (selectGo != null)
		{
			//selectGo.GetComponent<UITexture> ().color = unselectColor;
			selectGo.transform.Find ("block").GetComponent<UISprite> ().color = unselectColor;
		}

		selectGo = icons [index];
        selectGo.transform.Find("block").GetComponent<UISprite>().color = selectColor1;

		selectIndex = index;
	}

	private void OnIconClick(GameObject go)
	{
        int len = go.name.LastIndexOf('_');
        string i = go.name.Substring(len + 1);
		int index = int.Parse (i) - 1;


        AudioManger.Get().PlayEffect("onClick");
		Select (index);
	}

	public void OnCloseClick()
	{
		if (selectIndex != -1)
		{
			string icon = GetIcon (selectIndex);
            if (!LocalPlayer.Get().playerData.icon.Equals(icon))
			{
				// 发送网络信息，告诉服务器切换了头像
                LocalPlayer.Get().playerData.icon = icon;
                NetSystem.Instance.helper.ChangeIcon(icon);
			}
		}

        UISystem.Get().HideWindow("SelectHeadWindow");
        UISystem.Get().ShowWindow("CustomSelectWindowNew");
	}

	/// <summary>
	/// 获取头像，0~5
	/// </summary>
	/// <returns>The icon.</returns>
	/// <param name="id">Identifier.</param>
	public static string GetIcon(int index)
	{
		return string.Format ("select_head_{0}", index + 1);
	}
}

