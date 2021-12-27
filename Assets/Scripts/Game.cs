using UnityEngine;
using System;
using Solarmax;

public class Game : MonoBehaviour
{
    /// <summary>
    /// 场景UI根节点
    /// </summary>
    public GameObject           canvasRoot;

	/// <summary>
	/// 场景跟节点
	/// </summary>
	public GameObject			sceneRoot;

    /// <summary>
    /// 进入pause时间
    /// </summary>
	private float               appPauseBeginTime;


    /// <summary>
    /// 初始化是否结束
    /// </summary>
	public bool                 initFinished = false;

    
	public static Game          game;



	void Awake()
	{
		// 文件位置
		if (Application.isEditor)
		{
			Framework.Instance.SetWritableRootDir(Application.temporaryCachePath);
			Framework.Instance.SetStreamAssetsRootDir(Application.streamingAssetsPath);
		}
		else
		{
			Framework.Instance.SetWritableRootDir(Application.temporaryCachePath);
			Framework.Instance.SetStreamAssetsRootDir(Application.streamingAssetsPath);
		}

		// console输出
		LoggerSystem.Instance.SetConsoleLogger(new Solarmax.Logger(UnityEngine.Debug.Log));
		initFinished    = false;
		game            = this;
	}

	void Start ()
	{
		//表初始化
		UISystem.DirectShowPrefab ("UI/SplashWindow");
	}

	public void Init ()
	{
		// 初始化
		if (Framework.Instance.Init())
		{
			LoggerSystem.Instance.Info("系统启动！");
		}
		else
		{
			LoggerSystem.Instance.Error("系统启动失败！");
		}

		AsyncInitMsg();
		AudioManger.Get().Init();
        initFinished = true;
	}

	/// <summary>
	/// 异步加载msg，否则在第一次使用message时会延时很高
	/// </summary>
	private void AsyncInitMsg()
	{
		AsyncThread at = new AsyncThread ((arg) => {
			System.Threading.Thread.Sleep (100);
			//NetMessage.RegisterAllExtensions (null);
		});
		at.Start ();
	}

	void FixedUpdate()
	{
		if (initFinished) 
        {
			Framework.Instance.Tick(Time.deltaTime);
		}
	}

	void OnDestroy()
	{
		if (!initFinished)
			return;

		if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor) {
			Framework.Instance.Destroy ();
		} else {
			NetSystem.Instance.Destroy ();
		}

		LoggerSystem.Instance.Info("系统关闭！");
	}

	/// <summary>
	/// 应用进入暂停
	/// </summary>
	/// <param name="pauseStatus">If set to <c>true</c> pause status.</param>
	void OnApplicationPause(bool pauseStatus)
	{
		if (!initFinished)
			return;

		LoggerSystem.Instance.Info("OnApplicationPause " + pauseStatus);

		// 需要对断线进行处理
		if (pauseStatus) 
        {
			appPauseBeginTime = Time.realtimeSinceStartup;
		} 
        else 
        {
			// pvp模式时才重连，单机不管
			if (Time.realtimeSinceStartup - appPauseBeginTime >= 10) 
            {
				// 主动断开连接
				if (NetSystem.Instance.GetConnector ().GetConnectStatus () == ConnectionStatus.CONNECTED) {

					Debug.Log ("在后台超过10s，主动断开连接");
					NetSystem.Instance.Close();
				} 
                else 
                {
					// 先屏蔽掉
					//NetSystem.Instance.DisConnectedCallback ();
				}
			}
		}
	}
}
