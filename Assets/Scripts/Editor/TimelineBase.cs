using UnityEngine;
using System;
using System.Collections;






public enum TimeLineType
{
    Animation       = 0,
    Particle        = 1,
    Sound           = 2,
    Transform       = 3,
    Event           = 4,
    Trajectory      = 5,
    CameraAction    = 6,
    Effect          = 7
}

public enum ModelTargetType
{
    Player = 0,
    NPC = 1
}


public class TimelineBase
{

    public static int Comparer(TimelineBase a, TimelineBase b)
    {
        return (a.LineType().CompareTo(b.LineType()));
    }

    /// <summary>
    /// 时间线的容器对象
    /// </summary>
    private TimelineContainer timelineContainer;
    public TimelineContainer TimelineContainer
    {
        get
        {
            return timelineContainer;
        }
    }

    /// <summary>
    ///  时间序列对象
    /// </summary>
    public TimeLineSequencer Sequence
    {
        get { return null; }
    }

    [SerializeField]
    private bool            shouldRenderGizmos = true;
    
    /// <summary>
    /// Should this timeline render it's gizmos.
    /// </summary>
    public bool ShouldRenderGizmos
    {
        get { return shouldRenderGizmos; }
        set { shouldRenderGizmos = value; }
    }

    public virtual TimeLineType LineType()
    {
        return TimeLineType.Animation;
    }

    /// <summary>
    ///  Stops.
    /// </summary>
    public virtual void StopTimeline() {; }

    /// <summary>
    ///  Starts.
    /// </summary>
    public virtual void StartTimeline() {; }

    /// <summary>
    /// Ends.
    /// </summary>
    public virtual void EndTimeline() {; }

    /// <summary>
    /// Pauses.
    /// </summary>
    public virtual void PauseTimeline() {; }

    /// <summary>
    ///  Resumed.
    /// </summary>
    public virtual void ResumeTimeline() {; }

    /// <summary>
    /// Skips.
    /// </summary>
    public virtual void SkipTimelineTo(float time) {; }

    /// <summary>
    ///  processes. This should happen during regular playback and when scrubbing
    /// </summary>
    public virtual void Process(float sequencerTime, float playbackRate) {; }

    /// <summary>
    ///  has it's time manually set.
    /// </summary>
    public virtual void ManuallySetTime(float sequencerTime) {; }

    public virtual void ResetCachedData()
    {
        
    }

    /// <summary>
    /// Implement custom logic here if you need to do something special when uSequencer finds a missing AffectedObject in the scene (prefab instantiaton, with late binding).
    /// </summary>

    public virtual void LateBindAffectedObjectInScene(Transform newAffectedObject) {; }
}
