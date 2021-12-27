/*
-----------------------------------------------------------------------------------------------------
    骨骼动画---记录动画关键帧的相关信息
    log:  add by ljp 2021--3-29
-----------------------------------------------------------------------------------------------------
*/
using System;
using UnityEngine;



[Serializable]
public class GPUSkinningFrame
{

    /// <summary>
    /// 没跟骨头的缩放、旋转和偏移信息
    /// </summary>
    [NonSerialized]
    public Matrix4x4[]          matrices = null;

    /// <summary>
    /// 根骨格位移、旋转
    /// </summary>
    [NonSerialized]
    public Quaternion           rootMotionDeltaPositionQ;
    [NonSerialized]
    public Quaternion           rootMotionDeltaRotation;
    [NonSerialized]
    public float                rootMotionDeltaPositionL;

    /// <summary>
    /// 逆矩阵，用来还原某些信息
    /// </summary>
    [NonSerialized]
    private bool                rootMotionInvInit = false;
    [NonSerialized]             
    private Matrix4x4           rootMotionInv;

    public Matrix4x4 RootMotionInv( int rootBoneIndex )
    {
        if(!rootMotionInvInit )
        {
            rootMotionInv       = matrices[rootBoneIndex].inverse;
            rootMotionInvInit   = true;
        }
        return rootMotionInv;
    }
}

