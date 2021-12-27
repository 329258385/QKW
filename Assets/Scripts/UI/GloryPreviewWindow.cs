using System;
using UnityEngine;
using System.Collections.Generic;
using Solarmax;




/// <summary>
/// 段位对应地图和奖励界面GloryPreviewWindow
/// </summary>
public class GloryPreviewWindow : BaseWindow
{
	public UIScrollView         scrollView;
	public UIGrid               uiGrid;
	public GameObject           posTemplate;

    public UILabel              gloryName;
    public UILabel              gloryLevel;
	

	public override void OnShow ()
	{
		posTemplate.SetActive (false);
		// 显示所有阶

		uiGrid.transform.DestroyChildren ();
		List<LadderConfig> list = LadderConfigProvider.Instance.GetAllData ();
		for (int i = 0; i < list.Count; ++i)
        {
			AddElement (list[i]);
		}

		uiGrid.Reposition ();
		scrollView.ResetPosition ();
	}

	public override void OnHide()
	{
		
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{

	}

	private void AddElement(LadderConfig config)
	{
		GameObject go       = posTemplate;
		GameObject item     = null;
		UISprite icon       = null;
		UILabel money       = null;
		UILabel cardcount   = null;
		string[] drops      = config.itemgather.Split(',');

		go = NGUITools.AddChild (uiGrid.gameObject, go);
		go.SetActive (true);
        go.name     = "Loadder" + config.ladderlevel.ToString();

        GloryPreviewWindowCell cell = go.GetComponent<GloryPreviewWindowCell>();
        if( cell != null )
        {
            cell.SetInfo(config);
        }

        //Transform publicParent = go.transform.Find ("public");
        //Transform itemParent = go.transform.Find ("item");
        //UILabel lv = publicParent.Find ("name").GetComponent <UILabel> ();
        //UILabel name = publicParent.Find ("label").GetComponent <UILabel> ();
        //lv.text = string.Format ("{0}阶竞技场",config.ladderlevel);
        //name.text = config.laddername;

        //for (int i = 0; i < drops.Length; ++i) {
        //	int itemId = int.Parse (drops [i]);
        //	ChestConfig itemConfig = ChestConfigProvider.Instance.GetData (itemId);
        //	item = itemParent.Find (string.Format("card{0}",i)).gameObject;
        //	icon = item.transform.Find ("icon").GetComponent <UISprite> ();
        //	money = item.transform.Find ("money").GetComponent <UILabel> ();
        //	cardcount = item.transform.Find ("count").GetComponent <UILabel> ();
        //	if (!string.IsNullOrEmpty (itemConfig.icon))
        //		icon.spriteName = itemConfig.icon;

        //	money.text = string.Format ("{0}-{1}",itemConfig.mincoin,itemConfig.maxcoin);
        //	cardcount.text = itemConfig.itemnum.ToString ();
        //}
    }

    public void Update()
    {
        
    }

    public void OnCloseClick()
	{
		UISystem.Get ().HideWindow ("GloryPreviewWindow");
		UISystem.Get ().ShowWindow ("StartWindow");
	}
}
