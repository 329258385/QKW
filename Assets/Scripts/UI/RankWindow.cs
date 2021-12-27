using System;
using UnityEngine;
using System.Collections.Generic;
using Solarmax;

public class RankWindow : BaseWindow
{
	/// <summary>
	/// tabs
	/// </summary>
	public UILabel globalTab;
	public UILabel districtTab;
	public UILabel friendTab;
	/// <summary>
	/// The scroll view and grid.
	/// </summary>
	public UIScrollView scrollView;
	public UIGrid grid;
	/// <summary>
	/// The self rank. as scrollview item moudle
	/// </summary>
	public RankWindowCell template;
	public UILabel selfRankValue;

	private UILabel selectTab;

	private List<NetPlayer> rankList;

	void Awake()
	{
		rankList = new List<NetPlayer> ();

		globalTab.gameObject.GetComponent<UIEventListener> ().onClick = OnTabClick;
		districtTab.gameObject.GetComponent<UIEventListener> ().onClick = OnTabClick;
		friendTab.gameObject.GetComponent<UIEventListener> ().onClick = OnTabClick;

		scrollView.onShowMore = OnScrollViewShowMore;
		template.gameObject.SetActive (false);
	}

	public override bool Init ()
	{
		RegisterEvent (EventId.LoadRankList);

		return true;
	}

	public override void OnShow()
	{
		// 默认选择Global
		OnTabClick (globalTab.gameObject);
	}

	public override void OnHide()
	{

	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.LoadRankList)
		{
			List<NetPlayer> data = args [0] as List<NetPlayer>;
			int start = (int)args [1] + 1;
			int self = (int)args [2] + 1;
            NetPlayer pd = null;
			if (data == null)
				return;
			for (int i = 0; i < data.Count; ++i)
			{// 需要给scrollview增加这些接口
				GameObject go = NGUITools.AddChild(grid.gameObject, template.gameObject);
				go.SetActive (true);
				RankWindowCell cell = go.GetComponent<RankWindowCell>();
				cell.SetCell (start + i, data [i].name, data [i].score, data[i].icon );
			}

			grid.Reposition ();

			if (rankList.Count == 0) {
				// 第一次
				scrollView.ResetPosition ();
			}

			rankList.AddRange (data);

			// 设置自己
			selfRankValue.text = self.ToString ();
		}
	}

	/// <summary>
	/// Raises the tab click event.
	/// </summary>
	/// <param name="go">Go.</param>
	private void OnTabClick(GameObject go)
	{
		if (selectTab != null && go == selectTab.gameObject) {
			return;
		}

		Color unselectcolor = new Color (1, 1, 1, 0.3f);
		globalTab.color = unselectcolor;
		districtTab.color = unselectcolor;
		friendTab.color = unselectcolor;

		selectTab = go.GetComponent<UILabel>();

		selectTab.color = Color.white;

		// 删除
		rankList.Clear ();

		if (go == globalTab.gameObject) {//选中了globaltab
			NetSystem.Instance.helper.LoadRank (rankList.Count);
		} else if (go == districtTab.gameObject) {
			
		} else if (go == friendTab) {
			
		}

		grid.transform.DestroyChildren ();
	}

	public void OnCloseClick()
	{
		UISystem.Get ().HideWindow ("RankWindow");
		UISystem.Get ().ShowWindow ("CustomSelectWindowNew");
	}

	private void OnScrollViewShowMore()
	{
		NetSystem.Instance.helper.LoadRank (rankList.Count);
	}
}

