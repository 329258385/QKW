using System;
using UnityEngine;
using System.Collections;






public class GarageWindow : BaseWindow
{	

    /// <summary>
    /// 3D 界面部分
    /// </summary>
    private GameObject          ui3D = null;
    public GameObject           UIPerfab3D;
    //private City3DWindow        ship3dWindow;

    /// <summary>
    /// 飞船属性
    /// </summary>
    public UILabel[]            shipAttr;


	private void Awake()
	{
        ui3D = GameObject.Instantiate(UIPerfab3D) as GameObject;
        if (ui3D != null)
        {
            //ship3dWindow = ui3D.GetComponent<City3DWindow>();
        }
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
        UIPerfab3D = null;
        GameObject.Destroy(ui3D);
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		
	}

    public void OnBackClicked()
    {
        UISystem.Get().HideAllWindow();
        UISystem.Get().ShowWindow("StartWindow");
    }
}
