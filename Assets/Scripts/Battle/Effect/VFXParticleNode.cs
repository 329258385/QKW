using UnityEngine;
using System.Collections;







/// <summary>
/// 技能特效，由通用AniEffect改装而来
/// </summary>
public class VFXParticleNode : EffectNode
{

	/// <summary>
	/// 关心实例
	/// </summary>
	/// <value>The hoodEntity.</value>
    public GameObject       parent;


    public BattleMember		castShip;


    /// <summary>
    /// 特效开始时间
    /// </summary>
    /// <value>The start time.</value>
    public ParticleSystem[] particle = null;

	/// <summary>
	/// 生命周期
	/// </summary>
	/// <value>The effectName.</value>
	public float            lifeTime { get; set;}


	public override void Tick (int frame, float interval)
	{
        if (!isActive)
            return;

        UpdateEffect (frame, interval);
	}


    /// <summary>
    /// 释放
    /// </summary>
	public override void Destroy ()
	{
		base.Destroy ();
	}


    /// <summary>
    /// 回收
    /// </summary>
	public override void OnRecycle ()
	{
        for (int i = 0; i < particle.Length; i++)
            particle[i] = null;

        particle	= null;
        lifeTime	= 0;
        GameObject.DestroyImmediate(gameObject);
		base.OnRecycle ();
	}

	/// <summary>
	/// 初始化特效
	/// </summary>
    public override void InitEffectNode( Vector3 position, Quaternion rotation )
	{
		if (string.IsNullOrEmpty (nameKey)) 
		{
			Debug.Log ("InitEffectNode is null");
			return;
		}

		//初始化光环
		if (gameObject == null)
		{
			UnityEngine.Object res = AssetManager.Get().GetResources(nameKey);
			gameObject = GameObject.Instantiate(res, position, rotation) as GameObject;
			particle = gameObject.GetComponentsInChildren<UnityEngine.ParticleSystem>();
		}
        if( parent != null )
            gameObject.transform.parent = parent.transform;

        gameObject.SetActive(true);
        for( int i = 0; i < particle.Length; i++)
            particle[i].Play(true);
        isActive = true;
	}

	/// <summary>
	/// 心跳
	/// </summary>
	/// <param name="frame">Frame.</param>
	/// <param name="dt">Dt.</param>
	void UpdateEffect(int frame, float dt)
	{
		if (!isActive)
			return;

		if (lifeTime > 0)
        {
			lifeTime -= dt;
			if (lifeTime <= 0)
            {
				isActive = false;
				Recycle ( this );
			}
		}
	}
}