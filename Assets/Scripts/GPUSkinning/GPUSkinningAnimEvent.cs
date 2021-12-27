/*
-----------------------------------------------------------------------------------------------------
    骨骼动画---记录动画关键帧的事件
    log:  add by ljp 2021--3-29
-----------------------------------------------------------------------------------------------------
*/
using System;



[Serializable]
public class GPUSkinningAnimEvent : System.IComparable<GPUSkinningAnimEvent>
{
    /// <summary>
    /// 帧号、
    /// </summary>
    public int          frameIndex = 0;
    public int          eventId = 0;

    public int CompareTo(GPUSkinningAnimEvent other )
    {
        return frameIndex > other.frameIndex ? -1 : 1;
    }
}
