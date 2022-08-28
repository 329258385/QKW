using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Solarmax;

public partial class UISystem : Solarmax.Singleton<UISystem>, Lifecycle
{
	/// <summary>
	/// 需要管理的窗口
	/// </summary>
	private List<BaseWindow> mManagedWindows;

	/// <summary>
	/// 显示窗口栈
	/// </summary>
	private Stack<BaseWindow> mShowingStack;

	/// <summary>
	/// 窗口根节点
	/// </summary>
	#if !SERVER
	public UIRoot			mUIRoot;
	private UICamera		mUICamera;
	public Transform		mUIParent;
	#endif

	/// <summary>
	/// Initializes a new instance of the <see cref="UISystem"/> class.
	/// </summary>
	public UISystem ()
	{
		mManagedWindows		= new List<BaseWindow> ();
		mShowingStack		= new Stack<BaseWindow> ();

		#if !SERVER
		mUIRoot				= null;
		mUICamera			= null;
		#endif
	}

	/// <summary>
	/// 获得所需要的窗口对象，私有借口.
	/// </summary>
	/// <returns>The window.</returns>
	/// <param name="name">Name.</param>
	public BaseWindow GetWindow(string name)
	{
		BaseWindow bw = null;

		for (int i = 0; i < mManagedWindows.Count; ++i)
		{
			if (mManagedWindows [i] != null && mManagedWindows [i].GetName ().Equals(name))
			{
				bw = mManagedWindows [i];
			}
		}

		return bw;
	}

	/// <summary>
	/// 直接加入显示栈中
	/// </summary>
	/// <param name="bw">Bw.</param>
	private void PushShowingStack(BaseWindow bw)
	{
		if (bw == null)
			return;

		mShowingStack.Push (bw);
	}

	/// <summary>
	/// 从显示栈中清除这个界面
	/// tips: 可能界面不是最上层，需要处理
	/// </summary>
	/// <param name="bw">Bw.</param>
	private void PopShowingStack(BaseWindow bw)
	{
		if (bw == null)
			return;

		// 先判断，如果栈顶层的就是当前的，则直接pop
		if (mShowingStack.Peek () == bw)
		{
			mShowingStack.Pop ();
			return;
		}

		Stack<BaseWindow> temp = new Stack<BaseWindow>();
		BaseWindow b = null;
		for (int i = 0; i < mShowingStack.Count; ++i)
		{
			b = mShowingStack.Pop ();
			if (b != bw)
				temp.Push (b);
			else
				break;
		}
		for (int i = 0; i < temp.Count; ++i)
		{
			mShowingStack.Push (temp.Pop());
		}
		temp.Clear ();
	}

	/// <summary>
	/// 初始化
	/// </summary>
	public bool Init()
	{
		#if !SERVER
		GameObject uiRoot = UnityEngine.GameObject.Find("UI Root");
		if (uiRoot == null)
		{
			return false;
		}
		mUIRoot = uiRoot.GetComponent<UIRoot>();

		mUIParent		= GameObject.Find("UICanvas").transform;
		if(mUIParent == null )
        {
			return false;
        }

		GameObject go		= mUIRoot.transform.Find("Camera").gameObject;
		if( go != null )
        {
			mUICamera		= go.GetComponent<UICamera>();
        }
		#endif
		return true;
	}

	public void Tick(float interval)
	{
		
	}

	public void Destroy()
	{
		HideAllWindow ();

	}

	#if !SERVER
	public UIRoot GetNGUIRoot()
	{
		return mUIRoot;
	}
	#endif

	/// <summary>
	/// 判断窗口是否可见
	/// </summary>
	/// <returns><c>true</c> if this window is visible; otherwise, <c>false</c>.</returns>
	/// <param name="window">Window.</param>
	private bool IsWindowVisible(BaseWindow window)
	{
		return window != null && window.gameObject.activeSelf;
	}

	/// <summary>
	/// 判断窗口是否显示
	/// </summary>
	public bool IsWindowVisible(string name)
	{
		BaseWindow bw = GetWindow (name);

		return IsWindowVisible (bw);
	}

	/// <summary>
	/// 显示窗口
	/// tips：检查是否是隐藏
	/// </summary>
	/// <param name="name">Name.</param>
	public void ShowWindow(string name)
	{
		#if !SERVER
		// 判断是否是隐藏，如果隐藏，则设置显示，如果已经显示，则不操作
		BaseWindow bw = GetWindow(name);
		if (bw != null)
		{
			if (!IsWindowVisible (bw)) {
				bw.gameObject.SetActive (true);
				bw.OnShow ();
				PushShowingStack (bw);
			}

			// cancel 掉fade流程
			TweenAlpha ta = bw.gameObject.GetComponent<TweenAlpha>();
			if (ta != null) {
				ta.ResetToBeginning ();
				ta.value = 1.0f;
				ta.enabled = false;
			}
			return;
		}

		// 获得外部配置数据
		UIWindowConfig config = UIWindowConfigProvider.Instance.GetData (name);
		if (config == null || string.IsNullOrEmpty (config.mPrefabPath))
		{
			Debug.LogErrorFormat ("UISystem_ShowWindow : {0}'s prefab config is null!", name);
			return;
		}

		// 加载prefab，实例化，挂载至root点
		//GameObject prefab = Resources.Load(config.mPrefabPath) as GameObject;
		GameObject prefab = AssetManager.Get ().LoadResource (config.mPrefabPath) as GameObject;
		if (null == prefab)
		{
			Debug.LogErrorFormat ("UISystem_ShowWindow : {0}'s prefab cannot Load!", name);
			return;
		}

		prefab = UnityTools.AddChild(mUICamera.gameObject, prefab);
		bw	   = prefab.GetComponent<BaseWindow>();
		if (bw == null)
		{
			Debug.LogErrorFormat ("UISystem_ShowWindow : {0} can't get BaseWindow component from prefab!", name);
			return;
		}
		bw.SetConfigData (config);
		if (!bw.Init ())
		{
			Debug.LogErrorFormat ("UISystem_ShowWindow : {0}'s BaseWindow init failed!", name);
			return;
		}

		// 加入管理队列和栈
        mManagedWindows.Add(bw);
		bw.OnShow();
		PushShowingStack (bw);
		#endif
	}

	/// <summary>
	/// 隐藏窗口
	/// tips：判断配置数据是否需要直接删除
	/// </summary>
	/// <param name="name">Name.</param>
	public void HideWindow(string name)
	{
		BaseWindow bw = GetWindow (name);
		if (!IsWindowVisible (bw))
		{
			Debug.LogFormat ("UISystem_HideWindow : {0} don't showed or invisible, don't need hide.", name);
			return;
		}

		// 调用
		bw.OnHide ();

		// 先从显示栈中删了 //未预防在destroy后不知道是否还存在对象，移到前面处理 update at 2016-7-11
		PopShowingStack(bw);

		// 判断配置数据是否直接删除
		if (bw.GetConfigData ().mHideWithDestroy)
		{
			// 删除
			bw.Release ();
			mManagedWindows.Remove (bw);
			GameObject.Destroy (bw.gameObject);
		}
		else
		{
			// 隐藏
			bw.gameObject.SetActive (false);
		}

		//Resources.UnloadUnusedAssets ();
		//System.GC.Collect ();
	}

	public void RegistWindow (string name, BaseWindow bw)
	{

		UIWindowConfig config = UIWindowConfigProvider.Instance.GetData (name);

		bw.SetConfigData (config);
		bw.Init ();

		// 加入管理队列和栈
		mManagedWindows.Add(bw);
		bw.OnShow();
		PushShowingStack (bw);
	}

    /// <summary>
    /// 淡入淡出窗口
    /// </summary>
    public void FadeOutWindow2(string name)
    {
		#if !SERVER
        BaseWindow bw = GetWindow(name);
        if (bw == null)
            return;

        TweenAlpha ta = bw.gameObject.GetComponent<TweenAlpha>();
		float a = 1;
		float time = 0.25f;
		if (ta == null) {
			ta = bw.gameObject.AddComponent<TweenAlpha> ();
		} else {
			a = ta.value;
			time *= a;
		}
        ta.ResetToBeginning();
        ta.from = a;
        ta.to = 0;
		ta.duration = time;
        ta.SetOnFinished(() =>
		{
            UISystem.Get().HideWindow(name);
        });
        ta.Play(true);

		#endif
    }


    /// <summary>
	/// 关闭所有窗口
	/// </summary>
	public void HideAllWindow()
	{
		BaseWindow bw = null;
		for (int i = mManagedWindows.Count - 1; i >= 0; -- i)
		{
			if (i < mManagedWindows.Count) {
				bw = mManagedWindows [i];
				if (bw != null && IsWindowVisible (bw))
				{
					HideWindow (bw.GetName ());
				}
			}
		}

	}


	/// <summary>
	/// 窗口系统中注册事件时的统一回调方法
	/// </summary>
	/// <param name="eventId">Event identifier.</param>
	/// <param name="window">Window.</param>
	/// <param name="args">Arguments.</param>
	public void OnEventHandler(int eventId, object window, params object[] args)
	{
		// 如果窗口存在，就派发事件，不管显示还是隐藏
		BaseWindow bw = GetWindow ((string)window);
		if (bw != null)
		{
			bw.OnUIEventHandler ((EventId)eventId, args);
		}
	}

	/// <summary>
	/// Shows the dialog window.
	/// </summary>
	/// <param name="type">Type.</param>
	/// <param name="tipLog">Tip log.</param>
	public void ShowDialogWindow(int type, string tipLog)
	{
		ShowWindow("CommonDialogWindow");
		EventSystem.Instance.FireEvent (EventId.OnCommonDialog, type, tipLog);
	}

	/// <summary>
	/// 直接加载这个窗口
	/// </summary>
	/// <param name="path">Path.</param>
	public static void DirectShowPrefab (string path)
	{
		GameObject prefab = Resources.Load(path) as GameObject;
		GameObject uicamera = GameObject.Find ("UI Root/Camera");
		prefab = UnityTools.AddChild (uicamera, prefab);
	}
}

