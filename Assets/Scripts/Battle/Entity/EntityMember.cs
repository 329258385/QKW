using UnityEngine;
using System;
using System.Collections;
using Plugin;
using System.Collections.Generic;
using KevinIglesias;

/// <summary>
/// 飞船状态
/// </summary>
public enum MemberState
{
	ORBIT,			// 环绕中
	PREJUMP1,		// 飞行预备中
	JUMPING,		// 飞行中
	MoveTo,
    MAX,
}

public class EntityMember : DisplayEntity
{
	/// <summary>
	/// 飞船状态
	/// </summary>
	/// <value>The state of the ship.</value>
	public MemberState          shipState { get; set; }

	/// <summary>
	/// 目标
	/// </summary>
	public BattleMember			target;

	/// <summary>
	/// 飞行目标
	/// </summary>
	/// <value>The target node.</value>
	public Node                 targetNode  { get; set; }
    
    /// <summary>
    /// 函数集
    /// </summary>
    RunLockStepLogic[]          handler { get; set;}

	/// <summary>
	/// 所属
	/// </summary>
	/// <value>The ship.</value>
	public  BattleMember			ship { get; set; }


	Vector3							targetNodePos;

	/// <summary>
	/// 修改皮肤属性
	/// </summary>
	private MaterialPropertyBlock	skinBlock = new MaterialPropertyBlock();

	public Vector3 TargetPos
	{
		set 
		{ 
			targetNodePos			= value;
		}
		get { return targetNodePos;  }
	}
    
	bool                        warping;

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
        shipState								= MemberState.ORBIT;
        handler									= new RunLockStepLogic[(int)MemberState.MAX];
        handler[(int)MemberState.ORBIT]			= UpdateOrbit;
        handler[(int)MemberState.PREJUMP1]		= UpdatePreJump1;
        handler[(int)MemberState.JUMPING]		= UpdateJumping;
		handler[(int)MemberState.MoveTo]		= UpdateMoveTo;

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
        handler[(int)shipState](frame, interval);
    }


    public void OnALive()
    {
        if (go != null)
        {
            go.SetActive(true);
        }
    }

    public void OnRecycle()
	{
		shipState = MemberState.ORBIT;
		#if !SERVER
		if (go != null)
        {
            go.SetActive(false);
        }
		#endif
	}

	public void ClearTarget()
    {
		target = null;
	}

	/// <summary>
	/// 初始化对象
	/// </summary>
	private ThrowProp		Throw;
	private BowLoadScript   bowLoadScript;
	protected override void InitGameObject ()
	{
		go			= CreateGameObject ();
		go.transform.SetParent(Game.game.sceneRoot.transform);
		AniCtrl		= go.GetComponentInChildren<Animator>();
		Throw		= go.GetComponentInChildren<ThrowProp>();
		bowLoadScript = go.GetComponentInChildren<BowLoadScript>();
		go.transform.localEulerAngles	= Vector3.zero;
		go.transform.localScale			= Vector3.one;
		go.gameObject.SetActive(false);
		ModifySkin();
	}

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

    /// ---------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 环绕
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------------
    void UpdateOrbit(int frame, float dt)
	{
		
	}


	/// ---------------------------------------------------------------------------------------------------------
	/// <summary>
	/// 飞行状态1
	/// </summary>
	/// ---------------------------------------------------------------------------------------------------------
	private float mPreJump1Timer = 0f;
	void UpdatePreJump1(int frame, float dt)
	{
		if (ship.unitType == BattleMember.BattleUnitType.Member)
			return;

		//是否瞬移
		if (warping)
		{
			mPreJump1Timer			+= dt;
			if (mPreJump1Timer >= 1.0f)
			{
				mPreJump1Timer		= 0;
				warping				= false;
				ship.battleTeam.DeliverTeam(targetNodePos, targetNode);
			}
			return;
		}
		shipState		= MemberState.JUMPING;
	}


    /// ---------------------------------------------------------------------------------------------------------
	/// <summary>
	/// 飞行中
	/// </summary>
    /// ---------------------------------------------------------------------------------------------------------
	void UpdateJumping(int frame, float dt)
	{
        //得到移动到的目标位置
        Vector3 targetPos   = targetNodePos;
        float movespeed     = ship.GetAtt(ShipAttr.Speed);
        float moveDist      = (float)Math.Round(movespeed * dt, 2);
		Vector3 curPos      = GetPosition();
		AniCtrl.SetFloat(MoveHashCode, movespeed);

		//间距多少
		float dist			= (float)Math.Round(Vector3.Distance(curPos, targetPos), 2);
		if (dist > moveDist)
		{
			if (targetNode.revoType != RevolutionType.RT_None)
			{
                float eta       = dist / ship.GetAtt(ShipAttr.Speed);
				Vector3 nodePos = targetNode.GetNodeRunPosition(eta);
				nodePos.x       = (float)Math.Round (nodePos.x, 2);
				nodePos.y       = (float)Math.Round (nodePos.y, 2);
				nodePos.z       = (float)Math.Round (nodePos.z, 2);
				targetNodePos   = nodePos;
				targetPos       = targetNodePos;
			}

			SetPosition(Vector3.MoveTowards(curPos, targetPos, moveDist));
		}
		else
		{
			//到达
			SetPosition(targetPos);
			//进入星球
			ship.EnterNode(targetNode);
			//进入环绕状态
            shipState = MemberState.ORBIT;
			StartAttackAction();
		}
	}


	public void UpdateMoveTo(int frame, float dt)
	{
		float movespeed = ship.GetAtt(ShipAttr.Speed);
		float moveDist = (float)Math.Round(movespeed * dt, 2);
		Vector3 curPos = GetPosition();
		AniCtrl.SetFloat(MoveHashCode, movespeed);


		//间距多少
		go.transform.rotation = Quaternion.LookRotation(TargetPos - curPos);
		float dist = (float)Math.Round(Vector3.Distance(curPos, TargetPos), 2);
		if (dist > moveDist)
		{
			SetPosition(Vector3.MoveTowards(curPos, TargetPos, moveDist));
		}
		else
		{
			SetPosition(TargetPos);
			shipState = MemberState.ORBIT;
		}
	}

	public bool UpdateMove(int frame, float dt )
    {
		if ( !IsNeedMove() )
			return false;

		float movespeed		= ship.GetAtt(ShipAttr.Speed);
		float moveDist		= (float)Math.Round(movespeed * dt, 2);
		Vector3 curPos		= GetPosition();
		AniCtrl.SetFloat(MoveHashCode, movespeed);

		//间距多少
		go.transform.rotation	= Quaternion.LookRotation(TargetPos - curPos);
		float dist				= (float)Math.Round(Vector3.Distance(curPos, TargetPos), 2);
		if (dist > moveDist)
		{
			SetPosition(Vector3.MoveTowards(curPos, TargetPos, moveDist));
			return true;
		}
		else
        {
			SetPosition(TargetPos);
			shipState		= MemberState.ORBIT;
			return false;
		}
	}

	public bool IsNeedMove()
    {
		float delt = (TargetPos - position).magnitude;
		if (Mathf.Abs(delt) < 0.001f)
			return false;
		return true;
    }


	/// <summary>
	/// 暂时先定义小兵的战斗逻辑
	/// </summary>
	public void UpdateBattle( bool atkCity = false )
    {
		AniCtrl.SetFloat(MoveHashCode, 0f);
		Quaternion _lookAt = Quaternion.identity;
		if (target != null && !atkCity )
        {
			var animatorInfo = AniCtrl.GetCurrentAnimatorStateInfo(0);
			if (animatorInfo.normalizedTime > 1.0f)
			{
				AniCtrl.SetTrigger(AckHashCode);
			}

			_lookAt		= Quaternion.LookRotation(target.GetPosition() - GetPosition());
			if (Throw != null)
			{
				Throw.targetPos			= target.GetPosition();
				Throw.targetPos.y		+= 1.0f;
			}

			if (bowLoadScript != null)
			{
				bowLoadScript.targetPos = target.GetPosition();
				bowLoadScript.targetPos.y += 1.0f;
			}

			go.transform.rotation = _lookAt;
			ship.SetAtt(ShipAttr.AttackSpeed, BattleSystem.Instance.battleData.rand.Range(30, 60));
		}
		else
        {
			if( targetNode != null )
            {
				var animatorInfo = AniCtrl.GetCurrentAnimatorStateInfo(0);
				if (animatorInfo.normalizedTime > 1.0f)
				{
					AniCtrl.SetTrigger(AckHashCode);
				}

				_lookAt = Quaternion.LookRotation(targetNode.GetPosition() - GetPosition());
				if (Throw != null)
				{
					Throw.targetPos		= targetNode.GetPosition();
					Throw.targetPos.y += 1.0f;
				}

				if (bowLoadScript != null)
				{
					bowLoadScript.targetPos = targetNode.GetPosition();
					bowLoadScript.targetPos.y += 1.0f;
				}

				go.transform.rotation = _lookAt;
				ship.SetAtt(ShipAttr.AttackSpeed, BattleSystem.Instance.battleData.rand.Range(30, 60));
			}
        }
		
	}

	/// ---------------------------------------------------------------------------------------------------------
	/// <summary>
	/// 开始移动
	/// </summary>
	/// ---------------------------------------------------------------------------------------------------------
	public void MoveTo(Node node, bool warp)
	{
		targetNode			= node;
		//是否瞬移
		warping				= warp;

		float orbitDist;
		orbitDist			= 20f;
		targetNodePos		= targetNode.GetPosition();
		if (node.revoType != RevolutionType.RT_None && !warping)
		{
            //间距多少
            Vector3 position = GetPosition();
            float speed     = ship.GetAtt(ShipAttr.Speed);
            float eta       = Vector3.Distance(position, targetNodePos) / speed;
			Vector3 nodePos = targetNode.GetNodeRunPosition(eta);
			nodePos.x       = (float)Math.Round (nodePos.x, 2);
			nodePos.y       = (float)Math.Round (nodePos.y, 2);
			nodePos.z       = (float)Math.Round (nodePos.z, 2);
			targetNodePos   = nodePos;
		}

        Vector3 moveDir     = GetPosition() - targetNodePos;
		moveDir.Normalize();
		targetNodePos       += (moveDir * orbitDist);

        targetNodePos.x     = (float)Math.Round(targetNodePos.x, 2);
        targetNodePos.y     = (float)Math.Round(targetNodePos.y, 2);
        targetNodePos.z     = (float)Math.Round(targetNodePos.z, 2);

		Quaternion _lookAt  = Quaternion.LookRotation(targetNodePos - position);
		go.transform.rotation = _lookAt;

		//进入跳跃状态
		shipState           = MemberState.PREJUMP1;
	}


	public void PriorityEncircleCity()
	{
		if (targetNode == null) return;
		float orbitDist;
		orbitDist			= targetNode.GetWidth() * 30f;
		targetNodePos		= targetNode.GetPosition();
		
		Vector3 moveDir		= GetPosition() - targetNodePos;
		moveDir.Normalize();
		targetNodePos		+= (moveDir * orbitDist);

		targetNodePos.x		= (float)Math.Round(targetNodePos.x, 2);
		targetNodePos.y		= (float)Math.Round(targetNodePos.y, 2);
		targetNodePos.z		= (float)Math.Round(targetNodePos.z, 2);
	}

	public void StartAttackAction()
    {
		AniCtrl.SetFloat(MoveHashCode, 0f);
	}
}
