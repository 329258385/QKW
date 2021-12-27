/*
-----------------------------------------------------------------------------------------------------
    骨骼动画---记录动画
    log:  add by ljp 2021--3-29
-----------------------------------------------------------------------------------------------------
*/
using System;
using UnityEngine;




public class GPUSkinningAnimation : ScriptableObject
{
    public string               guid = null;
    public string               name = null;
    public int                  rootBoneIndex = 0;
    public Bounds               bounds;

    public GPUSkinningAnimationClip[] clips = null;
    public GPUSkinningBone[]          bones = null;

    public int                  textureWidth = 0;
    public int                  textureHeight = 0;
    public float                sphereRadius = 1.0f;

    
    public bool HasAnimationClip( int code )
    {
        foreach( var item in clips )
        {
            if (item.IsName(code))
                return true;
        }

        return false;
    }
}

