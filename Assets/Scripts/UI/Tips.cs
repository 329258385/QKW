using System;
using UnityEngine;
using System.Collections.Generic;

public class Tips : MonoBehaviour
{
	public enum TipsType : int
	{
		Default,         // 默认展示形式，直接弹出至中部
		FlowLeft,       // 系统公告形式，从右上飘到左上
		FlowUp,        // 功能通知形式，从下飘到上
		Bottom,          // 无法完成提示，显示在下方
		Debug,           // DEBUG提示，显示在下方，正式发布需要隐藏
	}

	private List<TipsData> tipsDataList = new List<TipsData>(); // 未展示的Toast数据, 需要循环使用(刚开始设计成可以抛弃的)
	private List<TipsItem> tipsItemList = new List<TipsItem>();       // 展示过的UIToast控件，需要循环使用
	public GameObject flowUpParent;
	public GameObject itemTemplate;
	private int tipsShowingCount = 0; // 正在显示的UIToast数量
	public int TipsShowMax = 10; // 最大同屏显示的UIToast数量
	public float TipsClearInterval = 20.0f; // UIToast清除的间隔

	private float clipWidth, clipHeight;

	private static Tips tips;

	private void Start()
	{
		tips = this;

		UIPanel p = gameObject.GetComponent<UIPanel>();
		clipWidth = p.width;
		clipHeight = p.height;

		Invoke ("UpdateDelete", TipsClearInterval);
	}

	private void Update()
	{
		UpdateData();
	}

	private void UpdateData()
	{
		// 超过最大显示个数则堆积缓冲
		if (tipsShowingCount >= TipsShowMax) return;

		// 一次只产生一条toast
		if (tipsDataList.Count == 0) return;

		TipsData data = null;
		for(int i = 0; i < tipsDataList.Count; ++i)
		{
			data = tipsDataList[i];
			if (data != null && data.IsNew())
			{
				if (data.IsImportantNotice())
				{
					// 判断是否有其他的flowleft在展示, 如果没有，则展示
					bool has = false;
					for(int j = 0; j < tipsDataList.Count; ++j)
					{
						if (tipsDataList[j].IsImportantNotice() && tipsDataList[j].IsShow())
						{
							has = true;
							break;
						}
					}
					if (!has)
					{
						MakeToast(data);
						break;
					}
				}
				else
				{
					MakeToast(data);
					break;
				}

			}
		}
	}

	private void UpdateDelete()
	{
		if (tipsItemList.Count > 0)
		{
			TipsItem ui = tipsItemList[tipsItemList.Count - 1];
			if (ui != null && ui.UnUse())
			{
				tipsItemList.Remove(ui);
				Destroy(ui.gameObject);
			}
		}

		Invoke ("UpdateDelete", TipsClearInterval);
	}

	public void ShowToast(TipsType type, string message, float seconds)
	{
		TipsData data = null;
		for (int i = 0; i < tipsDataList.Count; ++i)
		{
			if (tipsDataList[i] != null && tipsDataList[i].UnUse())
			{
				data = tipsDataList[i];
				break;
			}
		}

		if (data == null)
		{
			data = new TipsData();
			tipsDataList.Add(data);
		}

		data.Init(type, message, seconds);
	}

	private void MakeToast(TipsData data)
	{
		TipsItem ti = null;
		GameObject go = null;

		// 找到目前未使用的
		for (int i = 0; i < tipsItemList.Count; ++i)
		{
			if (tipsItemList[i].UnUse())
			{
				go = tipsItemList[i].gameObject;
				ti = tipsItemList[i];
			}
		}

		if (go == null && ti == null)
		{
			switch (data.mType)
			{
			case TipsType.FlowLeft:
			case TipsType.FlowUp:
				{
					go = NGUITools.AddChild(flowUpParent, itemTemplate);
				} break;
			default :
				{
					go = NGUITools.AddChild(flowUpParent, itemTemplate);
				} break;
			}

			if (go == null)
			{
				return;
			}

			ti = go.GetComponent<TipsItem>();
			tipsItemList.Add(ti);
		}

		switch (data.mType)
		{
		case TipsType.FlowLeft:
		case TipsType.FlowUp:
			{
				go.transform.SetParent(flowUpParent.transform);
			} break;
		default:
			{
				go.transform.SetParent(flowUpParent.transform);
			} break;
		}

		ti.Show(data, clipWidth, clipHeight, this);
	}

	public void OnShowUIToast(TipsType type)
	{
		++tipsShowingCount;
	}

	public void OnRecycleUIToast(TipsType type)
	{
		--tipsShowingCount;

		if (type == TipsType.FlowLeft)
		{
			bool has = false;
			for (int j = 0; j < tipsDataList.Count; ++j)
			{
				if (tipsDataList[j].IsImportantNotice() && tipsDataList[j].IsShow())
				{
					has = true;
					break;
				}
			}
		}
	}

	public static void Make(string message)
	{
		tips.ShowToast(Tips.TipsType.Default, message, 1.0f);
	}
	public static void Make(Tips.TipsType type, string message, float seconds)
	{
		tips.ShowToast (type, message, seconds);
	}
}

public class TipsData
{
	public enum Status : int
	{
		_NEW_,
		_SHOW_,
		_TIMEOUT_,
		_END_,
		_RECYCLE_,
	}

	public Tips.TipsType mType;
	public string mMessage;
	public float mSeconds;
	private Status mStatus;
	public TipsData()
	{
		mType = Tips.TipsType.Default;
		mMessage = "";
		mSeconds = 0;
		mStatus = Status._NEW_;
	}

	public TipsData(Tips.TipsType type, string message, float time)
	{
		mType = type;
		mMessage = message;
		mSeconds = time;
		mStatus = Status._NEW_;
	}
	public void Init(Tips.TipsType type, string message, float time)
	{
		mType = type;
		mMessage = message;
		mSeconds = time;
		mStatus = Status._NEW_;
	}

	public bool IsImportantNotice()
	{
		return mType == Tips.TipsType.FlowLeft;
	}

	public bool IsNew()
	{
		return mStatus == Status._NEW_;
	}
	public bool IsShow()
	{
		return mStatus == Status._SHOW_;
	}
	public bool IsEnd()
	{
		return mStatus == Status._END_ || mStatus == Status._TIMEOUT_;
	}
	public bool UnUse()
	{
		return mStatus == Status._RECYCLE_;
	}
	public void OnShow()
	{
		mStatus = Status._SHOW_;
	}
	public void OnTimeout()
	{
		mStatus = Status._TIMEOUT_;
	}

	public void OnEnd()
	{
		mStatus = Status._END_;
	}

	public void OnRecycle()
	{
		mStatus = Status._RECYCLE_;
	}
}



