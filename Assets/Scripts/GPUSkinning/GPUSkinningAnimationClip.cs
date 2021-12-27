/*
-----------------------------------------------------------------------------------------------------
    骨骼动画---记录动画
    log:  add by ljp 2021--3-29
-----------------------------------------------------------------------------------------------------
*/
using System;
using UnityEngine;




[Serializable]
public class GPUSkinningAnimationClip
{
    /// <summary>
    /// 动画名称、长度、采样率
    /// </summary>
    public string                   name = null;
    public float                    length = 0.0f;
    public int                      fps = 0;

    /// <summary>
    /// 动画播放模式
    /// </summary>
    public GPUSkinningWrapMode      wrapMode = GPUSkinningWrapMode.Once;

    public int                      pixelSegmentation = 0;
    public bool                     rootMotionEnabled = false;
    public bool                     individualDifferenceEnabled = false;

    /// <summary>
    /// 动画关键帧、事件
    /// </summary>
    public GPUSkinningFrame[]       frames = null;

    [NonSerialized]
    private int nameHash = -1;

    public bool IsName( string sName )
    {
        if (name.Equals(sName))
            return true;
        else
            return false;
    }

    public bool IsName( int code )
    {
        if( nameHash == -1 )
        {
            nameHash = Animator.StringToHash(name);
        }

        return code == nameHash;
    }

    public int HashName()
    {
        if (nameHash == -1)
        {
            nameHash = Animator.StringToHash(name);
        }
        return nameHash;
    }
}

