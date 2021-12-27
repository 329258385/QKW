using System;
using UnityEngine;





/// <summary>
/// 飞船实验室
/// </summary>
public class LaboratoryWindow : BaseWindow
{
    /// <summary>
    /// 3D 界面部分
    /// </summary>
    private GameObject ui3D = null;
    public GameObject UIPerfab3D;
    //private City3DWindow ship3dWindow;


    /// <summary>
    /// 升级材料
    /// </summary>
    public GameObject[] armorSprite;
    public GameObject[] wenponSprite;
    public GameObject[] engineSprite;
    public GameObject[] upLevel;


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
        for( int i = 0; i < upLevel.Length; i++ )
        {
            upLevel[i].SetActive(false);
        }
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
    /// 战舰各部分升级接口--护甲
    /// </summary>
    /// ---------------------------------------------------------------------------------------------
    public void LevelUpArmor()
    {

    }

    /// ---------------------------------------------------------------------------------------------
    /// <summary>
    /// 战舰各部分升级接口--武器
    /// </summary>
    /// ---------------------------------------------------------------------------------------------
    public void LevelUpWeapon()
    {

    }

    /// ---------------------------------------------------------------------------------------------
    /// <summary>
    /// 战舰各部分升级接口--引擎
    /// </summary>
    /// ---------------------------------------------------------------------------------------------
    public void LevelUpEngine()
    {

    }

    public void OnBackClicked()
    {
        UISystem.Get().HideAllWindow();
        UISystem.Get().ShowWindow("StartWindow");
    }
}
