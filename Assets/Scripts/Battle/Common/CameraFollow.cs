using System;
using UnityEngine;




public class CameraFollow : MonoBehaviour
{

    static public CameraFollow Instance = null;

    // Target to follow
    public Transform        target;
    public float            distance = 60.0f;
    public float            chaseHeight = 15.0f;

    public float            followDamping = 1f;
    public float            lookAtDamping = 4.0f;
    
  
    /// <summary>
    /// 相机的目标对象，可能是飞船，也可能是星球
    /// </summary>
    private bool            bIsFly = false;
    private BattleMember    followShip = null;
    private MemberState     shipState = MemberState.MAX;


    public void SetTarget(BattleMember targetShip )
    {
        if( targetShip != followShip )
        {
            followShip = targetShip;
            CaleShipFollowCamera();
        }

        if( targetShip == null )
        {
            gameObject.transform.position = new Vector3(0, 25, -60f);
            gameObject.transform.localEulerAngles = new Vector3(25f, 0, 0);
        }

        //if (followShip != null && (followShip.entity.shipState == ShipState.BATORBIT1 ||
        //    followShip.entity.shipState == ShipState.BATORBIT2) )

        //{
        //    distance    = 75f;
        //    chaseHeight = 35f;
        //    particleFollow.SetActive(false);
        //}
    }


    public void SetCameraHeightAndDistance(float fH, float fD)
    {
        distance    = fD;
        chaseHeight = fH;
    }

    /// <summary>
    /// 根据飞船的当前状态，更新相机的目标和跟随状态
    /// </summary>
    private void CaleShipFollowCamera()
    {
        //if (followShip == null )
        //    return;

        ///// 更新跃迁特效
        //if (shipState == ShipState.JUMPING && followShip.entity.shipState != ShipState.JUMPING )
        //{
        //    mTunnelEffect.gameObject.SetActive(false);
        //    mTunnelEffect.EndWarpTunnel(2.5f);
        //}

        //if( shipState != ShipState.JUMPING && followShip.entity.shipState == ShipState.JUMPING)
        //{
        //    mTunnelEffect.gameObject.SetActive(true);
        //    mTunnelEffect.StartWarpTunnel();
        //}

        //if ( followShip.entity.shipState == ShipState.JUMPING ||
        //    followShip.entity.shipState == ShipState.PREJUMP1 ||
        //    followShip.entity.shipState == ShipState.ORBIT ||
        //    followShip.entity.shipState == ShipState.PREJUMP2 )
        //{
        //    bIsFly      = true;
        //    distance    = 35f;
        //    chaseHeight = 7.5f;
        //    shipState   = followShip.entity.shipState;
        //}

        //else if (followShip.entity.shipState == ShipState.BATORBIT1 ||
        //    followShip.entity.shipState == ShipState.BATORBIT2 )
        //{
        //    bIsFly      = true;
        //    distance    = 75;
        //    chaseHeight = 35;
        //    shipState   = followShip.entity.shipState;
        //    particleFollow.SetActive(false);
        //}

        //else
        //{
        //    if ( followShip.currentNode == null )
        //        return;

        //    bIsFly      = false;
        //    distance    = 65;
        //    chaseHeight = 7.5f;
        //    shipState   = followShip.entity.shipState;
        //    target      = followShip.currentNode.GetGO().transform;
        //}
    }

    /// <summary>
    /// 判断状态是否切换了
    /// </summary>
    /// <returns></returns>
    private bool IsSwitchState( )
    {

        MemberState state = followShip.entity.shipState;
        if (state != shipState )
            return true;

        return false;
    }


    void Start()
    {
        Instance        = this;
    }


    /// <summary>
    /// 相机的心跳
    /// </summary>
    void FixedUpdate()
    {
        //if (followShip == null)
        //    return;

        //bool IsSwitch = IsSwitchState();
        //if (IsSwitch )
        //{
        //    CaleShipFollowCamera();
        //}
        //if (target == null) 
        //    return;

        //DoCamera();
    }

    void DoCamera()
    {
        //if (followShip.entity.shipState == ShipState.BATORBIT1 ||
        //    followShip.entity.shipState == ShipState.BATORBIT2)
        //{
        //    Vector3 newPos  = followShip.entity.mOrbitPosition;
        //    this.transform.LookAt(newPos);
        //    transform.position = Vector3.Lerp(transform.position, newPos - Vector3.forward * distance + Vector3.up * chaseHeight, Time.deltaTime * followDamping * 0.1f);
        //    return;
        //}

        //if (bIsFly)
        //{
        //    this.transform.LookAt(target.position);
        //    //transform.position = Vector3.Lerp(transform.position, target.position - target.right * distance + target.up * chaseHeight, Time.deltaTime * followDamping);
        //    transform.position = Vector3.Lerp(transform.position, target.position - target.forward * distance + target.up * chaseHeight, Time.deltaTime * 2.5f * followDamping);
        //}
        //else
        //{
        //    Quaternion _lookAt = target.rotation;
        //    transform.rotation = Quaternion.Lerp(transform.rotation, _lookAt, Time.deltaTime * lookAtDamping);

        //    Vector3 newPos     = target.position - target.forward * distance + target.up * chaseHeight;
        //    transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * followDamping);
        //}
    }
}