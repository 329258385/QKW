using UnityEngine;
using System;
using MV = UnityEngine.Vector3;





/// <summary>
/// 战队队形
/// </summary>
public static class MyBattleOrder
{
    public const float      MAX_FORMATION_SPACE = 1.5f;
    public const int        MAX_FORMATION_ROW = 5;
    public const int        MAX_FORMATION_COL = 5;

    //// y = 5 是英雄的位置、y = 4 小兵的随意位置
    /// <summary>
    /// 行军队形
    /// </summary>
    public static MV[] MarchFormation = new MV[25]
    {
        new MV(-1, 0,-1), new MV( 1, 0,-1), new MV(-1, 0,-2), new MV( 1, 0,-2), new MV( -1, 0,-3),
        new MV( 1, 0,-3), new MV(-1, 0,-4), new MV( 1, 0,-4), new MV( 0, 0, 0), new MV(  0, 0, 0),
        new MV( 0, 0, 0), new MV( 0, 0, 0), new MV( 0, 0, 0), new MV( 0, 0, 0), new MV(  0, 0, 0),
        new MV( 0, 0, 0), new MV( 0, 0, 0), new MV( 0, 0, 0), new MV( 0, 0, 0), new MV(  0, 0, 0),
        new MV( 0, 0, 0), new MV( 0, 0, 0), new MV( 0, 0, 0), new MV( 0, 0, 0), new MV(  0, 0, 0),
    };


    /// <summary>
    /// 攻击队形
    /// </summary>
    public static MV[] BattleFormation = new MV[25]
    {
        new MV( 0, 0, 0), new MV( 0, 0, 0), new MV( 0, 0, 0), new MV( 0, 0, 0), new MV( 0, 0, 0),
        new MV( 0, 0, 0), new MV(-1, 0, 1), new MV( 0, 0, 1), new MV( 1, 0, 1), new MV( 0, 0, 0),
        new MV(-2, 0, 0), new MV(-1, 0, 0), new MV( 0, 5, 0), new MV( 1, 0, 0), new MV( 2, 0, 0),
        new MV(-2, 0,-1), new MV( 0, 4, 0), new MV( 0, 4,-1), new MV( 0, 4, 0), new MV( 2, 0, -1),
        new MV( 0, 4, 0), new MV( 0, 4, 0), new MV( 0, 4, 0), new MV( 0, 4, 0), new MV( 0, 4, 0),
    };

    /// <summary>
    /// 防御队形
    /// </summary>
    public static MV[] DefensiveFormation = new MV[25]
    {
        new MV(-2, 0, 2), new MV(-1, 0, 2), new MV( 0, 0, 0), new MV( 1, 0, 2), new MV( 2, 0, 2),
        new MV(-2, 0, 1), new MV(-1, 0, 1), new MV( 0, 0, 1), new MV( 1, 0, 1), new MV( 1, 0, 2),
        new MV( 0, 4, 0), new MV( 0, 4, 0), new MV( 0, 5, 0), new MV( 0, 4, 0), new MV( 0, 4, 0),
        new MV( 0, 4, 0), new MV( 0, 4, 0), new MV( 0, 4, 0), new MV( 0, 4, 0), new MV( 0, 4, 0),
        new MV( 0, 4, 0), new MV( 0, 4, 0), new MV( 0, 4, 0), new MV( 0, 4, 0), new MV( 0, 4, 0),
    };

    /// <summary>
    /// 包围队形
    /// </summary>
    public static MV[] SurroundFormation = new MV[25]
    {
        new MV(-2, 0, 4), new MV( 0, 0, 0), new MV( 0, 0, 0), new MV( 0, 0, 0), new MV( 2, 0, 4),
        new MV(-2, 0, 3), new MV(-1, 0, 3), new MV( 0, 0, 0), new MV( 1, 0, 3), new MV( 2, 0, 3),
        new MV( 0, 4, 0), new MV(-1, 0, 2), new MV( 0, 0, 2), new MV( 1, 0, 2), new MV( 0, 4, 0),
        new MV( 0, 4, 0), new MV( 0, 4, 0), new MV( 0, 0, 1), new MV( 0, 4, 0), new MV( 0, 4, 0),
        new MV( 0, 4, 0), new MV( 0, 4, 0), new MV( 0, 5, 0), new MV( 0, 4, 0), new MV( 0, 4, 0),
    };
}
