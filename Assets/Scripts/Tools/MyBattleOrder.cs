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


    /// <summary>
    /// 行军队形
    /// </summary>
    public static MV[] MarchFormation = new MV[24]
    {
        new MV(-1, 0,  0),  new MV( 1, 0,  0),
        new MV(-1, 0, -1),  new MV( 1, 0, -1),
        new MV(-1, 0, -2),  new MV( 1, 0, -2), 
        new MV(-1, 0, -3),  new MV( 1, 0, -3),
        new MV(-1, 0, -4),  new MV( 1, 0, -4),
        new MV(-1, 0, -5),  new MV( 1, 0, -5), 
        new MV(-1, 0, -6),  new MV( 1, 0, -6), 
        new MV(-1, 0, -7),  new MV( 1, 0, -7),
        new MV(-1, 0, -8),  new MV( 1, 0, -8), 
        new MV(-1, 0, -9), new MV( 1, 0, -9),
        new MV(-1, 0, -10), new MV( 1, 0, -10), 
        new MV(-1, 0, -11), new MV( 1, 0, -11), 
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
