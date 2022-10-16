using UnityEngine;
using System.Collections;
using Plugin;
using Solarmax;






/// <summary>
/// 实体父类
/// </summary>
public class DisplayEntity : Lifecycle2
{
	/// <summary>
	/// 名字
	/// </summary>
	/// <value>The tag.</value>
	public string           tag       { get; set; }

    /// <summary>
    /// 物体的外观
    /// </summary>
    public string           perfab    { get; set; }


	/// <summary>
	/// 实例
	/// </summary>
	/// <value>The go.</value>
	public GameObject       go { get; set; }

	/// <summary>
	/// 动画控制器
	/// </summary>
	public Animator			AniCtrl;

    /// <summary>
    /// 位置
    /// </summary>
    public Vector3          position = Vector3.zero;

	/// <summary>
	/// The euler angles.
	/// </summary>
	public Vector3          eulerAngles = Vector3.zero;


	/// <summary>
	/// slient模式
	/// </summary>
	protected bool          silent { get; set; }

    /// <summary>
    /// 外观颜色
    /// </summary>
    public Color            color = Color.white;

    /// <summary>
    /// 星球的节点类型
    /// </summary>
    public NodeType         nodeType = NodeType.None;
    protected float         fscale = 1.0f;

    /// <summary>
    /// 初始化
    /// </summary>
    public DisplayEntity(string name, bool silent)
	{
		tag         = name;
		this.silent = silent;
	}

	public virtual bool Init ( )
	{
		InitGameObject ();
		return true;
	}

	public virtual void Tick (int frame, float interval)
	{
		
	}

	public virtual void Destroy ()
	{
		silent = false;

		if(go != null)
			GameObject.Destroy (go);
		go = null;
	}


	/// <summary>
	/// 初始化对象
	/// </summary>
	protected virtual void InitGameObject()
	{
		go								= CreateGameObject ();
		go.transform.SetParent(Game.game.sceneRoot.transform);
		AniCtrl							= go.GetComponentInChildren<Animator>();
		go.SetActive(false);
	}

	/// <summary>
	/// 创建对象
	/// </summary>
	protected virtual GameObject CreateGameObject()
	{
        Object res = null;
        if (!string.IsNullOrEmpty(perfab))
        {
            res		= AssetManager.Get().GetResources(perfab);
            return	GameObject.Instantiate(res) as GameObject;
        }
		return null;
	}

	/// <summary>
	/// 设置位置
	/// </summary>
	/// <param name="pos">Position.</param>
	public void SetPosition(Vector3 pos)
	{
		this.position = pos;
		if (silent)
			return;

		if (go == null)
			return;
		go.transform.position = pos;
	}

	/// <summary>
	/// 获得坐标
	/// </summary>
	/// <returns>The position.</returns>
	public Vector3 GetPosition()
	{
		return this.position;
	}


	/// <summary>
	/// 设置转动角度
	/// </summary>
	/// <param name="r3">R3.</param>
    public void SetEulerAngles( Vector3 r3)
	{
		this.eulerAngles = r3;
		#if !SERVER
		if (silent)
			return;

		if (go != null)
		{
			go.transform.eulerAngles = eulerAngles;
		}
		#endif
	}

    public Vector3 GetEulerAngles()
    {
        if (go != null)
        {
            return go.transform.localEulerAngles;
        }

        return eulerAngles;
    }

    /// <summary>
	/// 设置缩放比例
	/// </summary>
	public virtual void SetScale(float scale)
    {
        fscale = scale;
        if (silent)
            return;


        if (go == null)
            return;
        go.transform.localScale = Vector3.one * scale;
    }

    /// <summary>
	/// 设置颜色
	/// </summary>
	/// <param name="color">Color.</param>
	public virtual void SetColor(Color color)
    {
        this.color = color;
    }

    /// <summary>
    /// 设置slient模式
    /// 忽略表现层
    /// </summary>
    public virtual void SilentMode(bool status)
    {
        bool bake   = silent;
        silent      = status;
    }


	/// <summary>
	/// 播放动画
	/// </summary>
	/// <param name="ani"></param>
	public void Play(string ani )
    {
		this.AniCtrl.Play(ani);
    }
}
