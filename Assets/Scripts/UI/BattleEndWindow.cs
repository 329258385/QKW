using UnityEngine;
using System.Collections;
using Solarmax;

public class BattleEndWindow : BaseWindow
{
	public SpriteRenderer lineSingleRenderer;
	public TweenAlpha lineSingle;
	public SpriteRenderer linePvpRenderer;
	public TweenAlpha linePvp;


	private bool haveResult;
	NetMessage.SCFinishBattle proto;

	private GameType gameType;

	public override bool Init ()
	{
		RegisterEvent (EventId.OnFinished);
		RegisterEvent (EventId.OnFinishedColor);

		return true;
	}

	public override void OnShow ()
	{
		gameType = BattleSystem.Instance.battleData.gameType;
		//if (gameType == GameType.Single || gameType == GameType.Guide || gameType == GameType.TestLevel || gameType == GameType.SingleLevel)
        {
            // 单机
            //Invoke ("FinishSingle", 3f);
            Invoke("FinishPvp", 3f);
            //linePvp.gameObject.SetActive (true);
		}
  //      else
  //      {
		//	// 非单机
		//	haveResult = false;
		//	Invoke ("FinishPvp", 3f);
		//	linePvp.gameObject.SetActive (true);
		//}
	}

	public override void OnHide ()
	{
		CancelInvoke ("FinishPvp");
		//CancelInvoke ("FinishSingle");
		lineSingle.gameObject.SetActive (false);
		linePvp.gameObject.SetActive (false);
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.OnFinished)
        {
			haveResult = true;
			if (args.Length > 0)
				proto = args [0] as NetMessage.SCFinishBattle;
		}
        else if (eventId == EventId.OnFinishedColor)
        {
			Color winColor = (Color)args [0];
            lineSingleRenderer.color = winColor;
			linePvpRenderer.color = winColor;
		}
	}

	public void FinishPvp()
	{
		if (haveResult)
        {
			UISystem.Get ().HideAllWindow ();
            UISystem.Get ().ShowWindow("ResultWindow");
            EventSystem.Instance.FireEvent (EventId.OnFinished, proto);
		}
        else
        {
			Invoke ("FinishPvp", 0.5f);
		}
	}

	//public void FinishSingle()
	//{
	//	UISystem.Get ().FadeBattle(false, new EventDelegate(()=>{

 //           BattleSystem.Instance.BeginFadeOut();
 //           if (BattleSystem.Instance.battleData.gameType == GameType.Single)
 //           {
 //               BattleSystem.Instance.Reset();
 //               UISystem.Get().HideAllWindow();
 //               UISystem.Get().ShowWindow("StartWindow");
 //           }
            

	//		if (BattleSystem.Instance.battleData.gameType == GameType.SingleLevel)
	//		{
	//			BattleSystem.Instance.Reset();
 //               UISystem.Get().HideAllWindow();
 //               UISystem.Get().ShowWindow("CustomSelectLevelWindow");
	//		}
	//	}));
	//}
}

