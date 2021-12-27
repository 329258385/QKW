using System;
using UnityEngine;
using System.Collections.Generic;
using Solarmax;

public class ReplayWindow : BaseWindow
{
	/// <summary>
	/// 分页
	/// </summary>
	public UILabel myTab;
	public UILabel hotTab;
	/// <summary>
	/// 信息模板
	/// </summary>
	public GameObject template;
	/// <summary>
	/// The scroll view.
	/// </summary>
	public UIScrollView scrollView;
	public UIGrid grid;

	/// <summary>
	/// 选中的标签
	/// </summary>
	private UILabel selectTab;
	private float selectHotTabTime;

	/// <summary>
	/// The report list.
	/// </summary>
	private List<BattleReportData> myReportList;
	private List<BattleReportData> hotReportList;

	/// <summary>
	/// The frames.
	/// </summary>
	private NetMessage.PbSCFrames frames;

	private Color selectColor = new Color(1, 1, 1, 1);
	private Color unselectColor = new Color(1, 1, 1, 0.5f);

	private void Awake()
	{
		selectTab = null;
		myReportList = new List<BattleReportData> ();
		hotReportList = new List<BattleReportData> ();

		myTab.gameObject.GetComponent<UIEventListener> ().onClick += OnTabClick;
		hotTab.gameObject.GetComponent<UIEventListener> ().onClick += OnTabClick;
		scrollView.onShowMore = OnReportShowMore;

	}

	public override bool Init ()
	{
		RegisterEvent (EventId.OnBattleReportLoad);
		RegisterEvent (EventId.OnBattleReportPlay);

		return true;
	}

	public override void OnShow ()
	{
		myReportList.Clear ();
		hotReportList.Clear ();

		// 展示自己
		OnTabClick (myTab.gameObject);
	}

	public override void OnHide ()
	{
		
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.OnBattleReportLoad) {
			bool self = (bool)args [0];
			//int start = (int)args [1];
			List<BattleReportData> data = args [2] as List<BattleReportData>;
			// 校验一下是不是当前页面的数据
			bool confirm = false;
			if (self) {
				confirm = selectTab == myTab;
			} else {
				confirm = selectTab == hotTab;
			}

			if (!confirm)
				return;

			RefreshScrollView (data, false);

		} else if (eventId == EventId.OnBattleReportPlay) {
			frames = args [0] as NetMessage.PbSCFrames;
			BattleSystem.Instance.replayManager.TryPlayRecord (frames);
		}
	}

	/// <summary>
	/// 刷新
	/// </summary>
	/// <param name="data">Data.</param>
	private void RefreshScrollView(List<BattleReportData> data, bool useOldData)
	{
		List<BattleReportData> datalist = null;
		if (selectTab == myTab) {
			datalist = myReportList;

		} else {
			datalist = hotReportList;
		}

		int beginIndex = 0;
		if (!useOldData)
			beginIndex = datalist.Count;

		for (int i = 0, max = data.Count; i < max; ++i) {

            //if (data [i].playerList [0].raceId <= 0)
            //    continue;

			// 填充
			GameObject go = NGUITools.AddChild (grid.gameObject, template);
			go.SetActive (true);
			ReplayWindowCell cell = go.GetComponent<ReplayWindowCell> ();
			cell.SetInfo (i + beginIndex, data [i], selectTab == myTab);
		}
		
		grid.Reposition ();


		if (datalist.Count == 0 || useOldData) {
			scrollView.ResetPosition ();
		}

		if (!useOldData) {
			datalist.AddRange (data);
		}
	}

	/// <summary>
	/// 标签点击
	/// </summary>
	private void OnTabClick(GameObject go)
	{
		if (selectTab != null && go == selectTab.gameObject)
		{
			return;
		}

		myTab.color = unselectColor;
		hotTab.color = unselectColor;

		if (go.name.Equals (myTab.gameObject.name)) {
			selectTab = myTab;
		} else {
			selectTab = hotTab;
		}
		selectTab.color = selectColor;

		grid.transform.DestroyChildren ();

		if (selectTab == hotTab) {
			if (Time.realtimeSinceStartup > selectHotTabTime + 3) {
				selectHotTabTime = Time.realtimeSinceStartup;
				hotReportList.Clear ();
				OnReportShowMore ();
			} else {
				RefreshScrollView (hotReportList, true);
			}
		} else {
			if (myReportList.Count == 0) {
				OnReportShowMore ();
			} else {
				RefreshScrollView (myReportList, true);
			}
		}
	}

	/// <summary>
	/// 列表更多
	/// </summary>
	private void OnReportShowMore()
	{
		if (selectTab == hotTab) {
			NetSystem.Instance.helper.BattleReportLoad (false, hotReportList.Count);
		} else {
			NetSystem.Instance.helper.BattleReportLoad (true, myReportList.Count);
		}
	}

	public void OnCloseClick()
	{
		UISystem.Get ().HideWindow ("ReplayWindow");
		UISystem.Get ().ShowWindow ("CustomSelectWindowNew");
	}
}

