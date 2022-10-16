using Plugin;
using System;
using Unity.Mathematics;
using UnityEngine;


public partial class BattleMember
{          
    /// <summary>
	/// 目标建筑
	/// </summary>
	public Node                 targetNode { get; set; }

    /// <summary>
	/// 战斗目标
	/// </summary>
	public BattleMember         target;


    /// <summary>
    /// 目标位置
    /// </summary>
    public Vector3              targetPos;
    

    /// <summary>
    /// 是否瞬移
    /// </summary>
    bool                        warping;

    /// <summary>
	/// 飞船状态
	/// </summary>
	public MemberState          shipState { get; set; }

    /// <summary>
    /// 函数集
    /// </summary>
    RunLockStepLogic[]          handler { get; set; }


    /// ---------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 每帧都调用
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------------
    public void MoveUpdate(int frame, float interval)
    {
        handler[(int)shipState](frame, interval);
    }


    /// ---------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 移动到指定星球
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------------
    public void MoveTo(Node node, bool warp)
	{
		//添加到飞行列表, 瞬移暂时不加入飞行队列
        if ( !warp )
		    pool.AddFlyShip(this);

        targetNode                  = node;
        //是否瞬移
        warping                     = warp;
        float orbitDist             = 15f;
        Vector3 nodePos             = targetNode.GetPosition();
        float speed                 = GetAtt(ShipAttr.Speed);
        if (node.revoType != RevolutionType.RT_None && !warping)
        {
            //间距多少
            Vector3 position        = GetPosition();
            float eta               = Vector3.Distance(position, nodePos) / speed;
            Vector3 pos             = targetNode.GetNodeRunPosition(eta);
            pos.x                   = (float)Math.Round(pos.x, 2);
            pos.y                   = (float)Math.Round(pos.y, 2);
            pos.z                   = (float)Math.Round(pos.z, 2);
            nodePos                 = pos;
        }

        Vector3 moveDir             = GetPosition() - nodePos;
        moveDir.Normalize();
        nodePos                     += (moveDir * orbitDist);

        targetPos.x                 = (float)Math.Round(nodePos.x, 2);
        targetPos.y                 = (float)Math.Round(nodePos.y, 2);
        targetPos.z                 = (float)Math.Round(nodePos.z, 2);

        //mAgent.prefVelocity         = math.normalize(targetPos - mAgent.pos) * speed;
        //mAgent.navigationEnabled    = true;
        //mAgent.collisionEnabled     = true;
        //mAgent.maxSpeed = speed;

        //进入跳跃状态
        shipState                   = MemberState.PREJUMP1;
        this.EventGroup.fireEvent((int)BattleEvent.MoveToTarget, this, null);
    }


    /// ---------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 环绕
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------------
    void UpdateOrbit(int frame, float dt)
    {
        //if( mAgent != null )
        //{
        //    mAgent.maxSpeed         = 0.0f;
        //    mAgent.prefVelocity     = Unity.Mathematics.float3.zero;
        //    mAgent.collisionEnabled = false;
        //}
    }

    /// ---------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 暂时先定义小兵的战斗逻辑
    /// </summary>
    /// ---------------------------------------------------------------------------------------------------------
    public void UpdateBattle( int frame, float dt )
    {
        AotuBattle(frame, dt);
    }

    /// ---------------------------------------------------------------------------------------------------------
	/// <summary>
	/// 飞行中
	/// </summary>
    /// ---------------------------------------------------------------------------------------------------------
	void UpdateJumping(int frame, float dt)
    {
        //得到移动到的目标位置
        float movespeed             = GetAtt(ShipAttr.Speed);
        float moveDist              = (float)Math.Round(movespeed * dt, 2);
        Vector3 curPos              = GetPosition();
        //间距多少
        float dist                  = (float)Math.Round(Vector3.Distance(curPos, targetPos), 2);
        if (dist > moveDist)
        {
            if (targetNode.revoType != RevolutionType.RT_None)
            {
                float eta           = dist / GetAtt(ShipAttr.Speed);
                Vector3 nodePos     = targetNode.GetNodeRunPosition(eta);
                targetPos.x         = (float)Math.Round(nodePos.x, 2);
                targetPos.y         = (float)Math.Round(nodePos.y, 2);
                targetPos.z         = (float)Math.Round(nodePos.z, 2);
            }

            //mAgent.prefVelocity   = math.normalize(targetPos - mAgent.pos) * movespeed;
            SetPosition(Vector3.MoveTowards(curPos, targetPos, moveDist));
        }
        else
        {
            //到达
            SetPosition(targetPos);
            //进入星球
            EnterNode(targetNode);
            //进入环绕状态
            shipState               = MemberState.ORBIT;
        }
    }

    /// ---------------------------------------------------------------------------------------------------------
	/// <summary>
	/// 飞行状态1
	/// </summary>
	/// ---------------------------------------------------------------------------------------------------------
	private float                   mPreJump1Timer = 0f;
    void UpdatePreJump1(int frame, float dt)
    {
        //是否瞬移
        if (warping)
        {
            mPreJump1Timer += dt;
            if (mPreJump1Timer >= 1.0f)
            {
                mPreJump1Timer = 0;
                warping = false;

                battleTeam.DeliverTeam(new Vector3(targetPos.x, targetPos.y, targetPos.z), targetNode);
            }
            return;
        }
        shipState = MemberState.JUMPING;
    }


    /// <summary>
    /// 更新移动
    /// </summary>
    public bool UpdateMove(int frame, float dt)
    {
        float movespeed     = GetAtt(ShipAttr.Speed);
        float moveDist      = (float)Math.Round(movespeed * dt, 2);
        Vector3 curPos      = GetPosition();

        //间距多少
        float dist          = (float)Math.Round(Vector3.Distance(curPos, targetPos), 2);
        if (dist > moveDist)
        {
            SetPosition(Vector3.MoveTowards(curPos, targetPos, moveDist));
            return true;
        }
        else
        {
            SetPosition(targetPos);
            shipState       = MemberState.ORBIT;
            return false;
        }
    }


    public bool IsNeedMove()
    {
        Vector3 pos             = new Vector3(targetPos.x, targetPos.y, targetPos.z);
        float delt              = (pos - GetPosition()).magnitude;
        if (Mathf.Abs(delt) < 0.001f)
            return false;
        return true;
    }

    public void MoveToFly(Node node)
    {
        targetNode           = node;
        pool.AddFlyShip(this);
    }
}