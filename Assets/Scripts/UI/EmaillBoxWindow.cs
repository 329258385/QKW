using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;






public class EmaillBoxWindow : BaseWindow
{

    /// <summary>
    /// The info template. 用于列表项
    /// </summary>
    public GameObject infoTemplate;


    /// <summary>
    /// The scroll view.
    /// </summary>
    public UIScrollView scrollView;
    public UIGrid grid;


    private List<SimplePlayerData>  emaillboxList;
    private void Awake()
    {
        emaillboxList = new List<SimplePlayerData>();
    }


	public override bool Init ()
	{
		
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
      
		
	}

	public void OnCloseClick()
	{
        UISystem.Get().HideWindow("EmaillBoxWindow");
        UISystem.Get().ShowWindow("FriendWindow");
	}

    private void RefreshScrollView( List<SimplePlayerData> data )
    {
        grid.transform.DestroyChildren();
        for (int i = 0; i < data.Count; ++i)
        {
            GameObject go = NGUITools.AddChild(grid.gameObject, infoTemplate);
            go.name = "infomation_" + data[i].userId;
            go.SetActive(true);
            EmaillBoxWindowCell cell = go.GetComponent<EmaillBoxWindowCell>();
            cell.SetEamilBoxInfo(data[i]);
          
        }

        grid.Reposition();
        scrollView.ResetPosition();
    }
}

