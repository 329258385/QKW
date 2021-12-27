using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;






/// <summary>
/// 特效管理
/// </summary>
public class EffectManager : Solarmax.Singleton<EffectManager> , Lifecycle2
{
	/// <summary>
	/// 粒子特效池子
	/// </summary>
	private EffectPool			particlePool { get; set; }
    
	/// <summary>
    /// 动画播放的速度
    /// </summary>
    public float                fPlayAniSpeed = 1.0f;



	public bool Init ()
	{
		particlePool			= new EffectPool();

        return true;
	}

	public void Tick (int frame, float interval)
	{
		particlePool.Tick (frame, interval);

    }

	public void Destroy ()
	{
		particlePool.Destroy ();
    }

	/// <summary>
	/// 预先加载一些特效
	/// </summary>
	public void PreloadEffect( )
	{
		
	}

	/// <summary>
	/// 播放爆炸特效
	/// </summary>
	public void AddBomber(Node node, Vector3 position )
	{
		if (BattleSystem.Instance.battleData.silent)
			return;

		VFXParticleNode effect      = particlePool.Alloc<VFXParticleNode>("vfx_bullet_04");
        effect.nameKey				= "vfx_bullet_04";
        effect.castShip             = null;
        effect.lifeTime             = 0;
        effect.InitEffectNode(position, Quaternion.identity );
	    AudioManger.Get ().PlayExlposion (position);
	}

    /// <summary>
	/// 添加激光射线
	/// </summary>
	public void AddLaserLine(Vector3 beginPos, Quaternion rotation)
    {
        if (BattleSystem.Instance.battleData.silent)
            return;

		VFXParticleNode effect      = particlePool.Alloc<VFXParticleNode>("vfx_bullet_04");
        effect.nameKey				= "vfx_bullet_04";
        effect.castShip             = null;
        effect.lifeTime             = 1.5f;
        effect.InitEffectNode(beginPos, rotation );
		AudioManger.Get().PlayLaser(beginPos);

	}

	/// <summary>
	/// 添加传送门脉冲特效
	/// </summary>
	public void AddWarpPulse(Node from, Node to, TEAM team )
	{

	}

	/// <summary>
	/// 添加传送门到达特效
	/// </summary>
	public void AddWarpArrive( Node from, Color color )
	{

	}

	/// <summary>
	/// 播放粒子特效，播放结束就自动销毁
	/// </summary>
	public void PlayParticleEffect( Vector3 startPosition, Quaternion rotation, string effectName, float Life = 1.0f, GameObject parent = null )
	{
		VFXParticleNode effect      = particlePool.Alloc<VFXParticleNode>(effectName);
        effect.nameKey              = effectName;
        effect.lifeTime             = Life;
        effect.parent               = parent;
        effect.InitEffectNode(startPosition, rotation );
    }

    /// <summary>
	/// 播放粒子特效，播放结束就自动销毁
	/// </summary>
	public VFXParticleNode PlayBufferEffect( Vector3 startPosition, string effectName , float life = 3.0f )
	{
		VFXParticleNode effect      = particlePool.Alloc<VFXParticleNode>(effectName);
        effect.nameKey				= effectName;
		effect.lifeTime				= life;
        effect.InitEffectNode(startPosition, Quaternion.identity );
        return effect;
    }
}
