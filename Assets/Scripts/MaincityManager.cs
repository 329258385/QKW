using UnityEngine;
using System;
using Solarmax;








/// <summary>
/// 太空基地管理器
/// </summary>



public enum BuildType
{
    Master          = 0,                   // 主基地
    traingrounds    = 1,                   // 舰队训练实验室
    Nuclearpower    = 2,                   // 兵营
    Department      = 3,                   // 实验室
    Radarstation    = 4,                   // 雷达实验室
    Superspace      = 5,                   // 超空间实验室
    Shiplaboratory  = 6,                   // 战舰实验室
    Storedenergy    = 7,                   // 核能实验室

    Max,
}


/// <summary>
/// 基地建筑物描述
/// </summary>
[Serializable]
public class CityBuildMent
{
    public GameObject       display;
    public GameObject       center;
    public GameObject       hud;
}



public class MaincityManager : MonoBehaviour
{

    public static MaincityManager Instance = null;

    /// <summary>
    /// 基地根节点
    /// </summary>
    public GameObject           baseRoot;

    /// <summary>
    /// 基地功能建筑物
    /// </summary>
    
    public CityBuildMent[]      funcBuild = null;


	void Start ()
	{
        Instance = this;
        for (int n = 0; n < funcBuild.Length; n++)
            funcBuild[n].hud.SetActive(false);
	}

	public void Init ()
	{
		
	}
    
    public void SetEnable( bool b )
    {
        baseRoot.SetActive(b);
        if( b == true )
        {
            baseRoot.transform.localPosition                 = Vector3.zero;
            //CameraFollow.Instance.target                     = null;
            //CameraFollow.Instance.transform.localEulerAngles = new Vector3( 25f, 0, 0 );
            //CameraFollow.Instance.transform.localPosition    = new Vector3(0, 25, -60f);
        }
    }
}
