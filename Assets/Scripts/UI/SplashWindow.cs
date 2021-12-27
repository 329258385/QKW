using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Solarmax;

public class SplashWindow : BaseWindow 
{
	public TweenAlpha logoPage;
	public TweenAlpha warningPage;

	public override void OnShow ()
	{
	}

	/// <summary>
	/// 每次hidewindow均调用
	/// </summary>
	public override void OnHide ()
	{
		
	}

	/// <summary>
	/// 窗口事件响应处理方法
	/// </summary>
	/// <param name="eventId">Event identifier.</param>
	/// <param name="args">Arguments.</param>
	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		
	}

	// Use this for initialization
	void Awake () {

		logoPage.gameObject.SetActive (false);
		warningPage.gameObject.SetActive (false);

		Invoke ("ShowLogo", 0.5f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void ShowLogo ()
	{
		logoPage.gameObject.SetActive (true);
		logoPage.value = 0;
		logoPage.ResetToBeginning ();
		logoPage.from = 0;
		logoPage.to = 1;
		logoPage.duration = 0.5f;
		logoPage.enabled = true;
		logoPage.PlayForward ();

		Invoke ("HideLogo", 1.5f);
	}

	private void HideLogo ()
	{
		logoPage.gameObject.SetActive (true);
		logoPage.ResetToBeginning ();
		logoPage.from = 1;
		logoPage.to = 0;
		logoPage.duration = 0.5f;
		logoPage.enabled = true;
		logoPage.PlayForward ();

		Invoke ("ShowWarning", 0.8f);
	}

	private void ShowWarning ()
	{
		warningPage.gameObject.SetActive (true);
		warningPage.value = 0;
		warningPage.ResetToBeginning ();
		warningPage.from = 0;
		warningPage.to = 1;
		warningPage.duration = 0.5f;
		warningPage.enabled = true;
		warningPage.PlayForward ();

		Invoke ("HideWarning", 1.5f);
	}

	private void HideWarning ()
	{
		warningPage.gameObject.SetActive (true);
		warningPage.ResetToBeginning ();
		warningPage.from = 1;
		warningPage.to = 0;
		warningPage.duration = 0.5f;
		warningPage.enabled = true;
		warningPage.PlayForward ();

		Invoke ("Close", 0.8f);
	}

	private void Close ()
	{
		TweenAlpha ta = gameObject.GetComponent<TweenAlpha>();
		if (ta == null)
		{
			ta = gameObject.AddComponent<TweenAlpha>();
		}

		ta.ResetToBeginning();
		ta.from = 1;
		ta.to = 0;
		ta.duration = 0.5f;
		ta.SetOnFinished(() =>
			{
				GameObject.Destroy (gameObject);
			});

		UISystem.DirectShowPrefab ("UI/LogoWindow");
	}
}
