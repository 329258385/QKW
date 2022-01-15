using UnityEngine;
using System.Collections;
using System.Collections.Generic;




public enum GlowType
{
	GlowNone,
	GlowOn,
	GlowOff,
}
/// <summary>
/// 节点攻击模块
/// </summary>
public partial class Node
{

	/// <summary>
	/// The attack time.
	/// </summary>
	float AttackTime = 0f;


	/// <summary>
	/// 攻击飞船
	/// </summary>
	/// <param name="dt">Dt.</param>
	protected void AttackToShip(int frame, float dt)
	{
		AttackTime += dt;
        if (AttackTime >= AttackSpeed)
        {
            AttackTime		-= AttackSpeed;
            Vector3 nodePos	 = GetPosition();
			float Range		 = GetAttackRage();
            Range *= Range;

			int nAttackPower = (int)AttackPower;
			for (int i = 1; i < (int)TEAM.TeamMax; i++)
            {
                if (team == (TEAM)i) 
				{
					continue;
				}

                // 增加隐星效果
				List<BattleMember> ships = nodeManager.sceneManager.shipManager.GetFlyShip ((TEAM)i);
				for(int j = 0; j < ships.Count; j++)
				{
					if (currentTeam.IsFriend (ships [j].currentTeam.groupID))
						break;
            
					float dis = (nodePos - ships [j].GetPosition ()).sqrMagnitude;
					if (dis <= Range) 
					{
						#if !SERVER
						if( AP != null )
                        {
							nodePos				= AP.position;
                        }
						else
                        {
							nodePos.y			= 4.5f;
						}

						//特效
						Vector3 fireDirection	= ships[j].GetPosition() - nodePos;
						EffectManager.Get ().AddLaserLine (nodePos, Quaternion.LookRotation(fireDirection.normalized) );
						AudioManger.Get().PlayLaser(GetPosition());
						#endif

						if( ships[j].ChangeAttr( ShipAttr.Hp, -nAttackPower) <= 0 )
							ships[j].Bomb(nodeType);
						return;
					}
				}
			}
		}
	}


	/// <summary>
	/// 是否需要显示攻击圈
	/// </summary>
	public virtual bool CanShowRange()
	{
        if (nodeType == NodeType.Castle || nodeType == NodeType.Tower)
			return true;

		return false;
	}

	/// <summary>
	/// 显示攻击范围线
	/// </summary>
	/// <param name="bShow">If set to <c>true</c> b show.</param>
	public void ShowRange(bool bShow)
	{
		#if !SERVER
		if (CanShowRange()) 
		{
			
		}
		#endif
	}
}
