using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Solarmax;




public enum PlayerAttr
{
    Level,                  // 角色等级
    CityLevel,              // 基地等级
    ArsenalLevel,           // 科技等级
    LoboratoryLevel,        // 雷达等级
    ArmadaLevel,            // 舰队基地等级
    EngineLevel,            // 引擎室等级
}

/// <summary>
/// 队伍的属性枚举
/// </summary>
public enum TeamAttr
{
	PopulationMax,          // 人口上限
	Speed,                  // 移动速度
	Attack,                 // 攻击速度
	HpMax,                  // 最大兵数量上限
	CapturedSpeed,			// 掠夺速度
	OccupiedSpeed,			// 重建速度
	RebuildSpeed,			// 重建速度
	ProduceSpeed,			// 生产速度
    BeCapturedSpeed,		// 被掠夺速度
	HideFly,				// 隐身飞行
	
	MAX,
}




/// <summary>
/// 星球的属性枚举
/// </summary>
public enum NodeAttr
{
	Poplation = 0,      // 增加人口
	HpMax,              // 星球最大生命
	AttackRange,
	AttackSpeed,
	AttackPower,
	Ice,			    // 禁锢
	OccupiedSpeed,      // 占领速度
    ProduceSpeed,       // 生成速度
	MAX,
}



/// <summary>
/// 飞船属性枚举
/// </summary>
public enum ShipAttr
{
    Population = 0,     // 装载人口数量
    PopulationMax,      // 人口上限
    BaseHp,
    Hp,                 // 飞船的基础血量
    MaxHp,
    Armor,              // 飞船装甲值
    Speed,              // 移动速度
    AttackSpeed,        // 攻击速度
    AttackRange,        // 攻击范围
    WarningRange,       // 警戒范围
    AttackPower,        // 攻击强度
    MAX,
}


/// <summary>
/// 英雄属性
/// </summary>
public enum HeroAttr
{
	Population = 0,     // 装载人口数量
	PopulationMax,      // 人口上限
	Hp,                 // 飞船的基础血量
	MaxHp,
	Armor,              // 飞船装甲值
	Speed,              // 移动速度
	AttackSpeed,        // 攻击速度
	AttackRange,        // 攻击范围
	WarningRange,       // 警戒范围
	AttackPower,        // 攻击强度
	MAX,
}
