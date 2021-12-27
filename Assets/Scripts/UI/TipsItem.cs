using UnityEngine;
using System.Collections;

public class TipsItem : MonoBehaviour {

	public UILabel label;
	public UISprite background;

	public TipsData tipsData = null;

	private Vector3 bornPosition;
	private static int labelBornDepth;
	//private static int backgroundBornDepth;
	private float clipWidth = 0;
	private float clipHeight = 0;

	private Tips mRoot = null;


	void Awake()
	{
		bornPosition = this.gameObject.transform.localPosition;
		labelBornDepth = label.depth;
		//backgroundBornDepth = background.depth;
	}
	// Use this for initialization
	void Start () {
		//bornPosition = this.gameObject.transform.localPosition;
	}

	// Update is called once per frame
	void Update () {

		UpdateShow();

		// 根据自己的状态更新
		if (tipsData.IsEnd())
		{
			HideAndRecycle();
		}
	}

	public void Show(TipsData data, float cw, float ch, Tips root)
	{
		tipsData = data;
		clipWidth = cw;
		clipHeight = ch;
		mRoot = root;
		
		tipsData.OnShow();

		gameObject.SetActive(true);

		label.width = label.minWidth;
		label.height = label.minHeight;
		label.text = tipsData.mMessage;
		label.MakePixelPerfect ();
		label.gameObject.SetActive(true);
		background.SetDimensions(label.width + 150, background.height);
		background.gameObject.SetActive(true);

		background.depth = labelBornDepth++;
		label.depth = labelBornDepth++;

		gameObject.transform.localPosition = bornPosition;

		TweenPosition tp = this.gameObject.GetComponent<TweenPosition>();
		tp.ResetToBeginning();
		TweenAlpha ta = this.gameObject.GetComponent<TweenAlpha>();
		ta.ResetToBeginning();

		if (tipsData.mType == Tips.TipsType.FlowLeft)
		{
			background.gameObject.SetActive(false);

			float offset = label.width / 2 + clipWidth / 2;
			Vector3 posStart = new Vector3(bornPosition.x + offset, bornPosition.y, bornPosition.z);
			Vector3 posEnd = new Vector3(bornPosition.x - offset, bornPosition.y, bornPosition.z);
			gameObject.transform.localPosition = posStart;

			tp.from = posStart;
			tp.to = posEnd;
			tp.duration = (posEnd.x - posStart.x) / (clipWidth / 3f);
			tp.enabled = true;
			ta.to = 0.0f;
			ta.duration = 0.6f;
			ta.enabled = false;

		}
		else if (tipsData.mType == Tips.TipsType.FlowUp)
		{
			tp.from = bornPosition;
			tp.to = new Vector3(bornPosition.x, clipHeight / 2, bornPosition.z);
			tp.duration = 2.5f;
			tp.enabled = true;
			ta.to = 0.0f;
			ta.duration = 1.5f;
			ta.enabled = true;
		}

		mRoot.OnShowUIToast(tipsData.mType);
	}

	public void OnEnd()
	{
		tipsData.OnEnd();
	}

	public void OnTimeout()
	{
		tipsData.OnTimeout();
	}

	private void HideAndRecycle()
	{
		mRoot.OnRecycleUIToast(tipsData.mType);
		tipsData.OnRecycle();
		tipsData = null;
		this.gameObject.SetActive(false);
	}

	public bool UnUse()
	{
		return tipsData == null;
	}

	public void UpdateShow()
	{
		tipsData.mSeconds -= UnityEngine.Time.deltaTime;

		if (tipsData.mType == Tips.TipsType.Default || tipsData.mType == Tips.TipsType.Bottom)
		{
			if (tipsData.mSeconds <= 0)
			{
				OnTimeout();
			}
		}

	}
}
