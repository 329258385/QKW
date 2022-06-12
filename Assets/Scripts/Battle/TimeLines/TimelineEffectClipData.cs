using System;
using UnityEngine;





public class KeyFrameClipData : ScriptableObject
{
    [SerializeField]
    public EffectType       effectType = EffectType.None;

    [HideInInspector]
    [SerializeField]
    public string           dataString = "";          // 数据保存字符串
    [SerializeField]
    public object           DataObj;                  // 数据对象、使用时转换类型
    [SerializeField]
    public bool             active = false;
    [SerializeField]
    public Transform        Target;


    /// ------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 对象的初始化接口
    /// </summary>
    /// ------------------------------------------------------------------------------------------------------------------------------
    public void Init()
    {
        if (active)
            return;

        if( effectType == EffectType.Particle )
        {

        }
        else if ( effectType == EffectType.Trajectory )
        {

        }
        else if ( effectType == EffectType.Camera )
        {

        }
        else if( effectType == EffectType.Animation )
        {

        }
        else if ( effectType == EffectType.Sound )
        {

        }
        else
        {

        }
        active = true;
    }

    /// ------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 出发的心跳接口
    /// </summary>
    /// ------------------------------------------------------------------------------------------------------------------------------
    public void OnUpdate( float time )
    {

    }

    /// ------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 数据重置接口
    /// </summary>
    /// ------------------------------------------------------------------------------------------------------------------------------
    public void Reset()
    {

    }

    /// ------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 对象的保存接口
    /// </summary>
    /// ------------------------------------------------------------------------------------------------------------------------------
    public void SaveDataObj( object obj )
    {

    }


    [SerializeField]
    private float               startTime;
    public float StartTime
    {
        get { return startTime; }
        set
        {
            startTime = value;
        }
    }

    [SerializeField]
    private float               playbackDuration;
    public float PlaybackDuration
    {
        get { return playbackDuration; }
        set
        {
            playbackDuration = value;
        }
    }

    [SerializeField]
    private float               effectDuration;
    public float EffectDuration
    {
        get { return effectDuration; }
        set
        {
            effectDuration = value;
        }
    }

    [SerializeField]
    private bool                looping;
    public bool Looping
    {
        get { return looping; }
        set { looping = value; }
    }

    [HideInInspector]
    [SerializeField]
    private GameObject          targetObject;
    public GameObject TargetObject
    {
        get
        {
            return targetObject;
        }
        set
        {
            targetObject = value;
        }
    }

    [SerializeField]
    private AnimationTrack      track;
    public AnimationTrack Track
    {
        get { return track; }
        set
        {
            track = value;
        }
    }


    public float EndTime
    {
        get { return startTime + playbackDuration; }
        private set {; }
    }


    public static bool IsClipNotRunning(float sequencerTime, KeyFrameClipData clipData)
    {
        return sequencerTime < clipData.StartTime;
    }

    public static bool IsClipRunning(float sequencerTime, KeyFrameClipData clipData)
    {
        return sequencerTime > clipData.StartTime && sequencerTime < clipData.EndTime;
    }

    public static bool IsClipFinished(float sequencerTime, KeyFrameClipData clipData)
    {
        return sequencerTime >= clipData.EndTime;
    }
}
