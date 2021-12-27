using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Solarmax;

public class ChapterWindow : BaseWindow
{
	public UIScrollView scrollView;
	public UIGrid grid;
	public GameObject template;

	public override bool Init ()
	{
		RegisterEvent (EventId.UpdateChapterWindow);
		RegisterEvent (EventId.OpenSelectLevelWindow);
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
		if (eventId == EventId.UpdateChapterWindow) {
			Coroutine.Start (SetPage ());
		} else if (eventId == EventId.OpenSelectLevelWindow) {

			UISystem.Get().HideAllWindow();
			UISystem.Get().ShowWindow("CustomSelectLevelWindow");
		}
	}

	private IEnumerator SetPage ()
	{
		grid.transform.DestroyChildren ();
		List<ChapterInfo> allChapters = LevelDataHandler.Instance.chapterList;
		for (int i = 0, max = allChapters.Count; i < max; ++i) {
			GameObject go = NGUITools.AddChild (grid.gameObject, template);
			go.SetActive (true);
			ChapterWindowCell cell = go.GetComponent<ChapterWindowCell> ();
			cell.SetInfo (allChapters [i]);
			if (i / 3 > 0) {
				grid.Reposition ();

				scrollView.ResetPosition ();
				yield return 1;
			}
		}
		grid.Reposition ();
		scrollView.ResetPosition ();
	}

	public void OnCloseClick ()
	{
		UISystem.Instance.HideWindow ("ChapterWindow");
		UISystem.Instance.ShowWindow ("CustomSelectWindowNew");
	}
}

