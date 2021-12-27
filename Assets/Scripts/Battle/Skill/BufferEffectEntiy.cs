using System;
using System.Collections.Generic;
using UnityEngine;
using Solarmax;






/// <summary>
/// buffer 的具体效果对象实现
/// </summary>
public static class ApplyEffect
{
    /// <summary>
    /// 触发buff的地方设置
    /// </summary>
    public static void DoEffect(BattleMember sender, BattleMember target,CTagBufferConfig buffconfig)
    {
        int logicID = buffconfig.logicID;
        switch( logicID )
        {
            case 1:  //// 打击
                {
                    //sender.MainWeaponAttack();
                }
                break;

            case 2:  //// 突防
                {
                    if( sender.battleTeam != null )
                        sender.battleTeam.ClearTarget();
                    //sender.ShipPenetration();
                    CameraFollow.Instance.SetTarget(sender);
                    CameraFollow.Instance.SetCameraHeightAndDistance(7.5f, 35f );
                }
                break;

            case 3:  //// 撤退
                {
                    if (sender.battleTeam != null)
                        sender.battleTeam.ClearTarget();
                    //sender.ShipWithdraw();
                    CameraFollow.Instance.SetTarget(sender);
                    CameraFollow.Instance.SetCameraHeightAndDistance(7.5f, 35f);
                }
                break;

            default:
                break;
        }
    }


    /// --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 触发buff的地方设置
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------
    public static void UnDoEffect(BattleMember sender, BattleMember target, CTagBufferConfig buffconfig)
    {
        int logicID = buffconfig.logicID;
        switch (logicID)
        {
            case 1:  //// 打击
                {
                    //sender.MainWeaponAttack();
                }
                break;

            case 2:  //// 突防
                {
                    if (sender.battleTeam != null)
                        sender.battleTeam.ClearTarget();
                    //sender.ShipPenetration();
                    CameraFollow.Instance.SetTarget(sender);
                    CameraFollow.Instance.SetCameraHeightAndDistance(3.5f, 12.5f);
                }
                break;

            case 3:  //// 撤退
                {
                    if (sender.battleTeam != null)
                        sender.battleTeam.ClearTarget();
                    //sender.ShipWithdraw();
                    CameraFollow.Instance.SetTarget(sender);
                    CameraFollow.Instance.SetCameraHeightAndDistance(3.5f, 12.5f);
                }
                break;

            default:
                break;
        }
    }
}

