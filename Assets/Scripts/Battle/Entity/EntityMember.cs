using UnityEngine;
using System;
using KevinIglesias;






public class EntityMember : DisplayEntity
{
	/// <summary>
	/// 缓存对象
	/// </summary>
	private Transform					tfcache;


	/// <summary>
	/// 所属
	/// </summary>
	public  BattleMember				ship	{ get; set; }

	/// <summary>
	/// 修改皮肤属性
	/// </summary>
	private MaterialPropertyBlock		skinBlock = new MaterialPropertyBlock();


	public static int DeathHashCode		= Animator.StringToHash("Death");
	public static int IsDeathHashCode	= Animator.StringToHash("IsDeath");
	public static int MoveHashCode		= Animator.StringToHash("Speed");
	public static int AckHashCode		= Animator.StringToHash("Ack");
	public static int IsBlockHashCode   = Animator.StringToHash("IsBlock");
	public static int IsHitHashCode		= Animator.StringToHash("IsHit");

	/// <summary>
	/// 初始化
	/// </summary>
	public EntityMember(string name, bool silent) : base(name, silent)
	{
		
	}

    public override bool Init ( )
	{
		base.Init ();
		if ( ship != null )
        {
			ship.EventGroup.AddEvent(typeof(BattleEvent));
			ship.EventGroup.addEventHandler((int)BattleEvent.SetPos,		OnSetPos);
			ship.EventGroup.addEventHandler((int)BattleEvent.MoveToTarget,	OnStartMove);
			ship.EventGroup.addEventHandler((int)BattleEvent.Stop,			OnStopMove);
			ship.EventGroup.addEventHandler((int)BattleEvent.Attack,		OnAttack); 
			ship.EventGroup.addEventHandler((int)BattleEvent.Die,			OnDie);
			ship.EventGroup.addEventHandler((int)BattleEvent.ALine,			OnALive);
		}
		return true;
	}


    /// ---------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 每帧都调用
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------------
	public override void Tick (int frame, float interval)
	{
		base.Tick (frame, interval);
    }


	/// -----------------------------------------------------------------------------------
	/// <summary>
	/// 回收
	/// </summary>
	/// -----------------------------------------------------------------------------------
	public void OnRecycle()
	{
		#if !SERVER
		if (go != null)
        {
            go.SetActive(false);
        }
		#endif
	}

	/// -----------------------------------------------------------------------------------
	/// <summary>
	/// 初始化3D对象
	/// </summary>
	/// -----------------------------------------------------------------------------------
	private ThrowProp		Throw;
	private BowLoadScript   bowLoadScript;
	protected override void InitGameObject ()
	{
		go					= CreateGameObject ();
		tfcache				= go.transform;
		go.transform.SetParent(Game.game.sceneRoot.transform);
		AniCtrl				= go.GetComponentInChildren<Animator>();
		Throw				= go.GetComponentInChildren<ThrowProp>();
		bowLoadScript		= go.GetComponentInChildren<BowLoadScript>();
		go.transform.localEulerAngles	= Vector3.zero;
		go.transform.localScale			= Vector3.one;
		go.gameObject.SetActive(false);
		ModifySkin();
	}


	/// -----------------------------------------------------------------------------------
	/// <summary>
	/// 修改角色蒙骗颜色
	/// </summary>
	/// -----------------------------------------------------------------------------------
	protected void ModifySkin()
    {
		SkinnedMeshRenderer[] skinrender = go.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach( var skin in skinrender )
        {
			skinBlock.SetColor("_Color01", ship.currentTeam.color1 );
			skinBlock.SetColor("_Color02", ship.currentTeam.color2 );
			skinBlock.SetColor("_Color03", ship.currentTeam.color3 );
			skinBlock.SetColor("_Color04", ship.currentTeam.color4 );
			skin.SetPropertyBlock(skinBlock);
        }
    }

	/// -----------------------------------------------------------------------------------
	/// <summary>
	/// 更新位置
	/// </summary>
	/// -----------------------------------------------------------------------------------
	public void OnSetPos(object sender, EventArgs e)
	{
		if( ship != null )
        {
			Vector3 newPos		= ship.GetPosition();
			tfcache.position	= newPos;
			Vector3 targetdir	= Vector3.Normalize( ship.targetPos - GetPosition());
			tfcache.rotation	= Quaternion.LookRotation(targetdir);
		}
	}

	/// -----------------------------------------------------------------------------------
	/// <summary>
	/// 播放移动动画
	/// </summary>
	/// -----------------------------------------------------------------------------------
	public void OnStartMove(object sender, EventArgs e)
	{
		if( ship != null )
        {
			float speed			= ship.GetAtt(ShipAttr.Speed);
			Vector3 targetdir   = Vector3.Normalize(ship.targetPos - GetPosition());
			tfcache.rotation	= Quaternion.LookRotation(targetdir);
			AniCtrl.SetFloat(MoveHashCode, speed);
		}
	}

	/// -----------------------------------------------------------------------------------
	/// <summary>
	/// 停止播放动画
	/// </summary>
	/// -----------------------------------------------------------------------------------
	public void OnStopMove(object sender, EventArgs e)
	{
		if (ship != null)
		{
			AniCtrl.SetFloat(MoveHashCode, 0f);
		}
	}

	/// -----------------------------------------------------------------------------------
	/// <summary>
	/// 开始播放攻击动画
	/// </summary>
	/// -----------------------------------------------------------------------------------
	public void OnAttack(object sender, EventArgs e)
	{
		AniCtrl.SetFloat(MoveHashCode, 0f);
		AniCtrl.SetTrigger(AckHashCode);
		AniCtrl.speed		= 1.0f;
		AniCtrl.SetInteger("Action", 1);
	}


	/// -----------------------------------------------------------------------------------
	/// <summary>
	/// 播放死亡动画
	/// </summary>
	/// -----------------------------------------------------------------------------------
	public void OnDie(object sender, EventArgs e)
	{
		AniCtrl.SetBool(IsDeathHashCode, true);
		AniCtrl.SetInteger(IsDeathHashCode, 0);
		if (go != null)
		{
			go.SetActive(false);
		}
	}

	/// -----------------------------------------------------------------------------------
	/// <summary>
	/// 播放重生动画
	/// </summary>
	/// -----------------------------------------------------------------------------------
	public void OnALive(object sender, EventArgs e)
	{
		AniCtrl.SetBool(IsDeathHashCode, false);
		AniCtrl.SetInteger(IsDeathHashCode, 0);
		if (go != null)
		{
			go.SetActive(true);
		}
	}
}
