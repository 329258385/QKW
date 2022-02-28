using System;
using UnityEngine;
using System.Collections.Generic;
using Solarmax;




/// <summary>
/// 本地玩家
/// </summary>
public class LocalPlayer : Singleton<LocalPlayer> 
{
	/// <summary>
	/// 本地保存的account
	/// </summary>
	private string                          localAccount;

	/// <summary>
	/// 玩家的数据
	/// </summary>
	public NetPlayer                        playerData = new NetPlayer();


	/// <summary>
	/// 账号被接管，被踢
	/// </summary>
	public bool                             isAccountTokenOver = false;

    /// <summary>
    /// 携带技能列表
    /// </summary>
    /// <value>The skills.</value>
    public List<TechniqueEntiy>            vecTechniques = new List<TechniqueEntiy>();

    /// <summary>
    /// 战内上阵英雄
    /// </summary>
    public List<Simpleheroconfig>          battleTeam = new List<Simpleheroconfig>();

    /// <summary>
    ///  从网络上配置角色的新本信息
    /// </summary>
    public void InitLocalPlayer()
    {
        /// 配置编队
        Simpleheroconfig hero   = new Simpleheroconfig();
        hero.heroID             = 3007;
        HeroConfig config       = HeroConfigProvider.Get().GetData(hero.heroID);
        hero.InitAttr(config);
        battleTeam.Add(hero);

        Simpleheroconfig hero1  = new Simpleheroconfig();
        hero1.heroID            = 3006;
        config                  = HeroConfigProvider.Get().GetData(hero1.heroID);
        hero1.InitAttr(config);
        battleTeam.Add(hero1);

        /*Simpleheroconfig hero2  = new Simpleheroconfig();
        hero2.heroID            = 3001;
        config                  = HeroConfigProvider.Get().GetData(hero2.heroID);
        hero2.InitAttr(config);
        battleTeam.Add(hero2);*/
    }


    private void InitPlayerTechnique( NetPlayer player )
    {
        /// 打击
        TechniqueEntiy technique = TechniqueEntiy.CreateTechnique(null, 101901);
        if (technique != null)
        {
            vecTechniques.Add(technique);
        }

        /// 干扰
        technique = TechniqueEntiy.CreateTechnique( null, 101301);
        if (technique != null)
        {
            vecTechniques.Add(technique);
        }

        /// 磁盾
        technique = TechniqueEntiy.CreateTechnique( null, 101401);
        if( technique != null )
        {
            vecTechniques.Add(technique);
        }


        /// 盾寻
        technique = TechniqueEntiy.CreateTechnique( null, 102001);
        if (technique != null)
        {
            vecTechniques.Add(technique);
        }

        /// 突防
        technique = TechniqueEntiy.CreateTechnique( null, 102201);
        if (technique != null)
        {
            vecTechniques.Add(technique);
        }

        /// 充能
        technique = TechniqueEntiy.CreateTechnique( null, 101501);
        if (technique != null)
        {
            vecTechniques.Add(technique);
        }
    }


    //private AircraftFormation FindAircraftFormation(int battleID)
    //{
    //    int nCount = aircraftArray.Count;
    //    for (int i = 0; i < nCount; i++ )
    //    {
    //        if (aircraftArray[i].id == battleID)
    //            return aircraftArray[i];
    //    }
    //    return null;
    //}

	/// <summary>
	/// 获取本地保存的账号
	/// </summary>
	/// <returns>The local account.</returns>
	public string GetLocalAccount()
	{
		if (string.IsNullOrEmpty (localAccount)) {
			localAccount = LocalAccountStorage.Get().account;
		}
		return localAccount;
	}

	/// <summary>
	/// 生成本地账号
	/// </summary>
	/// <returns>The local account.</returns>
	public string GenerateLocalAccount(bool forceChange = false)
	{
		
		localAccount = SystemInfo.deviceUniqueIdentifier;
		if (forceChange)
		{
			int rand = UnityEngine.Random.Range (0, 10000);
			localAccount = localAccount + "__force__" + rand.ToString();
		}
		LocalAccountStorage.Get().account = localAccount;
		LocalStorageSystem.Get ().NeedSaveToDisk ();
		return localAccount;
	}

}

