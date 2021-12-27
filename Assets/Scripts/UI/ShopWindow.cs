using System;
using UnityEngine;





/// <summary>
/// 飞船实验室
/// </summary>
public class ShopWindow : BaseWindow
{
    /// <summary>
    /// 3D 界面部分
    /// </summary>
    private GameObject      ui3D = null;
    public GameObject       UIPerfab3D;
    //private City3DWindow    ship3dWindow;



    private void Awake()
    {
        ui3D = GameObject.Instantiate(UIPerfab3D) as GameObject;
        if (ui3D != null)
        {
            //ship3dWindow = ui3D.GetComponent<City3DWindow>();
        }
    }


    public override bool Init()
    {

        return true;
    }

    public override void OnShow()
    {

    }

    /// <summary>
    /// 每次hidewindow均调用
    /// </summary>
    public override void OnHide()
    {
        UIPerfab3D = null;
        GameObject.DestroyImmediate(ui3D);
    }

    /// <summary>
    /// 窗口事件响应处理方法
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="args">Arguments.</param>
    public override void OnUIEventHandler(EventId eventId, params object[] args)
    {

    }


    /// ---------------------------------------------------------------------------------------------
    /// <summary>
    /// 返回按钮
    /// </summary>
    /// ---------------------------------------------------------------------------------------------
    public void OnBackClicked()
    {
        UISystem.Get().HideAllWindow();
        UISystem.Get().ShowWindow("StartWindow");
    }
}
