using UnityEngine;
using System;
using MV = UnityEngine.Vector3;





/// <summary>
/// ս�Ӷ���
/// </summary>
public static class MyBattleOrder
{
    public const float      MAX_FORMATION_SPACE = 1.5f;
    public const int        MAX_FORMATION_ROW = 5;
    public const int        MAX_FORMATION_COL = 5;

    //// y = 5 ��Ӣ�۵�λ�á�y = 4 С��������λ��
    /// <summary>
    /// �о�����
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
    /// ��������
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
    /// ��������
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
    /// ��Χ����
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