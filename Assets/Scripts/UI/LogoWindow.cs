using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;
using System.Threading;

/// <summary>
/// 登录界面
/// </summary>
public class LogoWindow : BaseWindow
{

	public GameObject           progressGo = null;
	public UIProgressBar        progressBar = null;
	public UILabel              progressLabel = null;
	public UILabel              loadingTip = null;

	private int                 progressChangeTimes = 0;
	private float               progressChangeValue = 0;
	private int                 trickTime = 0;
	private float               progressValue = 0;


	private float               targetProgress;
	private float               progressInterval;

	private static bool         hasInit = false;

	void Awake()
	{
		progressChangeTimes     = 50;
		progressChangeValue     = 1.0f / progressChangeTimes;
		trickTime               = Random.Range (99, progressChangeTimes - 1);
		progressBar.value       = 0;
		progressGo.SetActive (true);
	}

	void Start ()
	{
		InvokeRepeating ("UpdateProgress", progressChangeValue, progressChangeValue);
		SetProgress (0.1f);
		Invoke ("DelayStart", 0.5f);
	}


    ///----------------------------------------------------------------------------------------------------
    /// <summary>
    /// 更新游戏登录进度
    /// </summary> 
    ///----------------------------------------------------------------------------------------------------
    private void UpdateProgress()
    {
        if (progressValue >= targetProgress)
        {
            return;
        }

        progressValue   += 0.05f;
        progressValue   = Mathf.Clamp01(progressValue);
        progressBar.Set(progressValue);
        progressLabel.text = string.Format("{0}%", Mathf.RoundToInt(progressValue * 100));

        if( progressValue >= 1.0f )
        {
            CreateLocalPlayer();
        }
    }

	private void DelayStart ()
	{
		if (!hasInit)
		{
			Game.game.Init ();
			UISystem.Instance.RegistWindow ("LogoWindow", this);
			hasInit = true;
		}

        /// 请求版本信息
		/// Coroutine.Start (UpdateSystem.Get ().RequestVersion());
        /// 直接发送版本更新完成
        EventSystem.Instance.FireEvent(EventId.OnABGetVersions, 0);
	}

	public override bool Init ()
	{

        RegisterEvent(EventId.OnABGetVersions);
        RegisterEvent(EventId.OnABDownloadingFinished);

        /*
		RegisterEvent (EventId.RequestUserResult);
		RegisterEvent (EventId.CreateUserResult);
		
		RegisterEvent (EventId.OnABDownloadIndex);
		RegisterEvent (EventId.OnABDownloading);
		
		RegisterEvent (EventId.OnPackageUpdate);
		RegisterEvent (EventId.OnHttpNotice);
		RegisterEvent (EventId.OnSDKLoginResult);
        */
		return true;
	}

	public override void OnShow()
	{
		
	}

	public override void OnHide()
	{

	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
        ////if (eventId == EventId.RequestUserResult)
        ////{
        ////    // 请求玩家数据结果
        ////    NetMessage.ErrCode code = (NetMessage.ErrCode)args [0];
        ////    if (code == NetMessage.ErrCode.EC_Ok) 
        ////    {
        ////        HandleGetUserInfo ();
        ////    } 
        ////    else if (code == NetMessage.ErrCode.EC_NoExist) 
        ////    {
        ////        // 默认玩家
        ////        CreateDefaultUser ();
        ////    } 
        ////    else if (code == NetMessage.ErrCode.EC_NeedResume) 
        ////    {
        ////        // 需要恢复
        ////        UISystem.Get ().ShowWindow ("CommonDialogWindow");
        ////        EventSystem.Instance.FireEvent (EventId.OnCommonDialog, 1, "您有未完成的比赛，正在进入......");
        ////        NetSystem.Instance.helper.ReconnectResume (); // 此时根据结果来判断是走哪种恢复
        ////    }
        ////} 
        ////if (eventId == EventId.CreateUserResult) 
        ////{
        ////    // 创建玩家结果
        ////    NetMessage.ErrCode code = (NetMessage.ErrCode)args [0];
        ////    if (code == NetMessage.ErrCode.EC_Ok) 
        ////    {
        ////        HandleGetUserInfo ();
        ////    } else if (code == NetMessage.ErrCode.EC_NameExist) {
        ////        Tips.Make ("用户名重复!");
        ////    } else if (code == NetMessage.ErrCode.EC_InvalidName) {
        ////        Tips.Make ("用户名不合法!");
        ////    } else if (code == NetMessage.ErrCode.EC_AccountExist) {
        ////        Tips.Make ("账号重复！");
        ////    } else {
        ////        Tips.Make ("用户名错误！");
        ////    }
        ////}
        if (eventId == EventId.OnABGetVersions) 
        {
            SetProgress(0.5f);
            int versionCount = (int)args [0];
			loadingTip.text += "";
			if (versionCount == 0) 
            {
				
                EventSystem.Instance.FireEvent(EventId.OnABDownloadingFinished);
			} 
            else 
            {
				loadingTip.text     += string.Format ("\n接收到来自基地的{0}个导航数据更新信号", versionCount);
				progressInterval    = 0.5f / versionCount;
			}
		} 
        ////else if (eventId == EventId.OnPackageUpdate) 
        ////{
        ////    // 整包更新
        ////    TableVersion packageUpdateVersion = args [0] as TableVersion;
        ////    // 弹出大版本更新提示框
        ////    string desc = packageUpdateVersion.desc.Replace ("\\n", "\n");
        ////    UISystem.Instance.ShowWindow ("CommonDialogWindow");
        ////    EventSystem.Instance.FireEvent (EventId.OnCommonDialog, 4, desc
        ////        , new EventDelegate (() => {

        ////        Application.OpenURL (packageUpdateVersion.assetTableUrl);

        ////    }));

        ////} 
        //else if (eventId == EventId.OnHttpNotice) 
        //{
        //    // 获取http公告
        //    string notice = (string)args [0];
        //    if (string.IsNullOrEmpty (notice)) {
        //        // 没有公告，直接登录登录
        //        LocalPlayerLogin();
        //    } 
            //else 
            //{
            //    // 有公告，分析公告
            //    int type = 0;
            //    int.TryParse (notice.Substring (0,1), out type);
            //    string text = notice.Substring (2).Replace ("\\n", "\n");
            //    UISystem.Instance.ShowWindow ("CommonDialogWindow");
            //    if (type == 0) {
            //        EventSystem.Instance.FireEvent (EventId.OnCommonDialog, 1, text
            //            , new EventDelegate (() => {
            //                LoginSDK ();
            //            }));
            //    } else {
            //        EventSystem.Instance.FireEvent (EventId.OnCommonDialog, 4, text);
            //    }

            //}

		//}
        ////else if (eventId == EventId.OnABDownloadIndex) {
        ////    int index = (int)args [0];
        ////    int size = (int)args [1];
        ////    loadingTip.text += string.Format ("\n正在解码第{0}段数据，大小:{1}KB......", index + 1, size / 8000);
        ////    SetProgress (targetProgress + (index + 1) * progressInterval);

        ////} 
        ////else if (eventId == EventId.OnABDownloading) {
        ////    float progress = (float)args [0];
        ////    int bytes = (int)args [1];
        ////    Debug.Log ("progress : " + progress + "  已接收:" + bytes);

        ////} 
        else if (eventId == EventId.OnABDownloadingFinished) 
        {
			SetProgress (1f);
            //UpdateSystem.Get ().ReloadClient ();
            LocalPlayerLogin();
        } 
        //else if (eventId == EventId.OnSDKLoginResult) 
        //{
        //    LocalPlayerLogin();
        //}
	}

	private void SetProgress(float progress)
	{
		targetProgress = progress;
	}

	
	/// <summary>
	/// 请求离线公告
	/// </summary>
    ////private void RequestOfflineNotice()
    ////{
    ////    EventSystem.Instance.FireEvent(EventId.OnHttpNotice, "");

    ////    //Coroutine.Start (UpdateSystem.Instance.RequestHttpNotice ());
    ////}

	/// <summary>
	/// 登录sdk
	/// </summary>
	
    ////private IEnumerator LoginServer()
    ////{
    ////    yield return new WaitForSeconds (0.2f);
    ////    loadingTip.text += "\n开启曲率引擎";
    ////    // 预加载资源
    ////    AssetManager.Get().LoadBattleResources();
    ////    SetProgress (0.8f);

    ////    yield return new WaitForSeconds (0.2f);
    ////    yield return NetSystem.Instance.helper.ConnectServer ();

    ////    if (NetSystem.Instance.GetConnector ().GetConnectStatus () == ConnectionStatus.CONNECTED) 
    ////    {
    ////        SetProgress (1.0f);
    ////        yield return new WaitForSeconds (0.2f);
    ////        loadingTip.text += "\n抵达致远星";
    ////        yield return new WaitForSeconds (0.2f);
    ////        CheckUser ();
    ////    } 
    ////    else 
    ////    {
    ////        yield return new WaitForSeconds (3.0f);
    ////        Coroutine.Start (LoginServer ());
    ////    }
    ////}


    private void LocalPlayerLogin()
    {

        // 预加载资源
        AssetManager.Get().LoadBattleResources();
        
        SetProgress(1.0f);
        loadingTip.text += "\n抵达致远星";
        Thread.Sleep(1000);
    }

	/// <summary>
	/// 检查当前有没有用户
	/// 	没有，输入用户名；有，login
	/// </summary>
    //private void CheckUser()
    //{
    //    NetSystem.Instance.helper.RequestUser ();
    //}

	/// <summary>
	/// 创建默认玩家
	/// </summary>
    //private void CreateDefaultUser()
    //{
    //    int index       = Random.Range (0, 10);
    //    string icon     = SelectIconWindow.GetIcon (index);
    //    NetSystem.Instance.helper.CreateUser ("", icon);
    //}

	/// <summary>
	/// 获得玩家信息后处理
	/// </summary>
    ////private void HandleGetUserInfo()
    ////{
    ////    // 进入单机关卡
    ////    UISystem.Get().ShowWindow("CustomSelectWindowNew");
    ////    UISystem.Get().FadeOutWindow ("LogoWindow");
    ////    return;
    ////}

    public void CreateLocalPlayer()
    {
        string account = LocalPlayer.Get().GetLocalAccount();
        if (string.IsNullOrEmpty(account))
        {
            account = LocalPlayer.Get().GenerateLocalAccount();
        }

        // 创建默认玩家
        int index = Random.Range(0, 10);
        NetSystem.Instance.helper.CreateUser("", "");

        // 这样子就把数据赋值到内存中吧，放在BattleManager中存着先
        LocalPlayer.Get().playerData.Init("");
        LocalPlayer.Get().InitLocalPlayer();
        LocalPlayer.Get().isAccountTokenOver = false;


        // 进入单机关卡
        UISystem.Get().ShowWindow("LobbyWindow");
    }
}

