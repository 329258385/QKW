using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;

public class AssetManager : Singleton<AssetManager>
{
	const string SOUND_PATH     = "sounds/";
	const string SPRITE_PATH    = "Character/";
	const string EFFECT_PATH	= "Effect/";
	const string UI_PATH        = "UI/";

 
	Dictionary<string, Object> resources = new Dictionary<string, Object>();

	public void Init()
	{
		/////////////////////////////////////////
		
	}

	/// <summary>
	/// 加载战斗用资源
	/// </summary>
	public void LoadBattleResources()
	{
		/////////////////////////////////////////
		/// 建筑
        AddSprite ("Hero001");
		AddSprite ("Hero002");
		AddSprite ("Hero003");
		AddSprite ("Archer");
		AddSprite ("ShieldWarrior");
		AddSprite ("ThrowerDummy");
		AddSprite ("BloodSlider");


		/////////////////////////////////////////
		/// UI
		AddUI("UISlgCityOperater");
		AddUI("UINodeOperater");

        /////////////////////////////////////////
        /// 特效
        AddSprite ("Entity_Line");
		AddSprite ("Entity_Select");

        ///////////////////////////////////////
        /// 特效
        AddParticle("vfx_bullet_04");
		AddParticle("vfx_bullet_09");
		AddParticle("vfx_bullet_14");
		AddParticle("IonMarker");
		AddParticle("IonStrike");
		AddParticle("MagicMissileBarrage");
		AddParticle("SpiritBomb");
		AddParticle("StarRays");
		AddParticle("SummonStorm");
		AddParticle("Hit 4");
		AddParticle("Hit 9");
		AddParticle("Hit 14");

		////////////////////////////////////////
		/// 声音
		AddSound ("explosion01");
	}

	/// <summary>
	/// 释放战斗资源
	/// </summary>
	public void UnLoadBattleResources()
	{
		/////////////////////////////////////////
		/// 角色
		RemoveResources("Hero001");
		RemoveResources("Hero002");
		RemoveResources("Hero003");
		RemoveResources("Archer");
		RemoveResources("ShieldWarrior");
		RemoveResources("ThrowerDummy");
		RemoveResources("BloodSlider");

		/// UI 
		RemoveResources("UISlgCityOperater");
		RemoveResources("UINodeOperater");


		////////////////////////////////////////////
		/// 特效
		RemoveResources("vfx_bullet_04");
		RemoveResources("vfx_bullet_09");
		RemoveResources("vfx_bullet_14");
		RemoveResources("Hit 4");
		RemoveResources("Hit 9");
		RemoveResources("Hit 14");

		RemoveResources ("explosion01");
		
		Resources.UnloadUnusedAssets ();
		System.GC.Collect ();
	}

	
	/// <summary>
	/// 删除资源
	/// </summary>
	/// <param name="key">Key.</param>
	void RemoveResources(string key)
	{
		if (resources.ContainsKey (key)) {
			resources.Remove (key);
		}
	}

	/// <summary>
	/// 获取资源
	/// </summary>
	/// <returns>The resources.</returns>
	/// <param name="key">Key.</param>
	public Object GetResources(string key)
	{
		Object res = null;

		resources.TryGetValue (key, out res);

		return res;
	}

	/// <summary>
	/// 添加图片资源
	/// </summary>
	/// <param name="key">Key.</param>
	public void AddSprite(string key)
	{
		if (resources.ContainsKey (key))
			return;

		resources.Add (key, LoadResource(SPRITE_PATH + key));
	}

	/// <summary>
	/// 添加声音资源
	/// </summary>
	/// <param name="key">Key.</param>
	public void AddSound(string key)
	{
		if (resources.ContainsKey (key))
			return;

		resources.Add (key, LoadResource(SOUND_PATH + key));
	}

	/// <summary>
	/// 添加ui资源
	/// </summary>
	/// <param name="key">Key.</param>
	public void AddUI(string key)
	{
		if (resources.ContainsKey (key))
			return;

		resources.Add (key, LoadResource(UI_PATH + key));
	}

	/// <summary>
	/// 添加粒子资源
	/// </summary>
	/// <param name="key">Key.</param>
	public void AddParticle(string key)
	{
		if (resources.ContainsKey (key))
			return;

		resources.Add (key, LoadResource(EFFECT_PATH + key));
	}



	/// <summary>
	/// 读取资源
	/// </summary>
	/// <returns>The resource.</returns>
	/// <param name="path">Path.</param>
	public Object LoadResource(string path)
	{
		object asset = null;
		#if !SERVER
		if (asset == null)
		{
			asset = Resources.Load (path);
		}
		#endif
		
		return (Object)asset;
	}

	/// <summary>
	/// 加载streaming资源
	/// 仅仅是获取了正确的资源路径，仍然使用外部读取手段
	/// </summary>
	/// <param name="relativePath">Relative path.</param>
	public string LoadStramingAsset (string relativePath)
	{
		object asset = null;
		if (asset == null) {
			return DataProviderSystem.FormatDataProviderPath (relativePath);
		}

		return (string)asset;
	}
}
