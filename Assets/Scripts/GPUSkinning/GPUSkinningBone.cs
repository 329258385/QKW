/*
-----------------------------------------------------------------------------------------------------
    骨骼动画---记录骨骼的相关信息
    log:  add by ljp 2021--3-29
-----------------------------------------------------------------------------------------------------
*/
using System;
using UnityEngine;




[Serializable]
public class GPUSkinningBone
{
    [NonSerialized]
    public Transform        transform = null;
    [NonSerialized]
    public Matrix4x4        bindpose;

    public int              parentBoneIndex = -1;
    public int[]            childrenBonesIndices = null;

    [NonSerialized]
    public Matrix4x4        animationMatrix;
    public string           name = null;
    public string           guid = null;
    public bool             isExposed = false;

    [NonSerialized]
    private bool            bindposeInvInit = false;
    [NonSerialized]
    private Matrix4x4       bindposeInv;

    public Matrix4x4 BindposeInv
    {
        get
        {
            if (!bindposeInvInit )
            {
                bindposeInv     = bindpose.inverse;
                bindposeInvInit = true;
            }
            return bindposeInv;
        }
    }
}
