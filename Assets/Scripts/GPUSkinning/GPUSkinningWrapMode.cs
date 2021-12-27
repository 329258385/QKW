using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 动画播放模式
/// </summary>
public enum GPUSkinningWrapMode
{
    Once,
    Loop
}


/// <summary>
/// 对应unity
/// </summary>
public enum GPUSKinningCullingMode
{
    AlwaysAnimate,
    CullUpdateTransforms,
    CullCompletely
}


/// <summary>
/// 顶点受骨骼影响的数量
/// </summary>
public enum GPUSkinningQuality
{
    Bone1,
    Bone2,
    Bone4
}

/// <summary>
/// 
/// </summary>
public enum GPUSkinningShaderType
{
    Unlit,
    StandardSpecular,
    StandardMetallic
}