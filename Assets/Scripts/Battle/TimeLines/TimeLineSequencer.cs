using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;




public class TimeLineSequencer : MonoBehaviour
{
    #region Member Variables
    /// <summary>
    /// 当前运行时间
    /// </summary>
    [SerializeField]
    private float                       runningTime = 0.0f;

    /// <summary>
    /// 播放速率
    /// </summary>
    [SerializeField]
    private float                       playbackRate = 1.0f;

    /// <summary>
    /// 时间序列总长度 周期
    /// </summary>
    [SerializeField]
    private float                       duration = 10.0f;

    /// <summary>
    /// 播放方式 循环还是往返
    /// </summary>
    [SerializeField]
    private bool                        isLoopingSequence = false;

    [SerializeField]
    private bool                        isPingPongingSequence = false;

    private bool                        playing = false;


    /// <summary>
    /// 首次运行
    /// </summary>
    private bool                        isFreshPlayback = true;
    private float                       previousTime = -1.0f;
    private float                       minPlaybackRate = -100.0f;
    private float                       maxPlaybackRate = 100.0f;
    #endregion


    // 开发属性
    #region Properties
    public float                Duration
    {
        get
        {
            return duration;
        }
        set
        {
            duration = value;
            if (duration <= 0.0f)
                duration = 0.1f;
        }
    }

    public static float SequenceUpdateRate
    {
        get { return 0.005f * Time.timeScale; }
    }

    /// <summary>
    /// 是否在播放
    /// </summary>
    public bool                 IsPlaying
    {
        get { return playing; }
    }
    /// <summary>
    /// 循环播放
    /// </summary>
    public bool                 IsLopping
    {
        get { return isLoopingSequence; }
        set { isLoopingSequence = value; }
    }

    /// <summary>
    /// 往返播放
    /// </summary>
    public bool                 IsPingPonging
    {
        get { return isPingPongingSequence; }
        set { isPingPongingSequence = value; }
    }

    /// <summary>
    /// 播放是否结束
    /// </summary>
    public bool                 IsComplete
    {
        get { return (!IsPlaying && RunningTime >= Duration); }
        set {; }
    }

    public float                MinPlaybackRate
    {
        get { return minPlaybackRate; }
    }

    public float                MaxPlaybackRate
    {
        get { return maxPlaybackRate; }
    }

    public float                PlaybackRate
    {
        get { return playbackRate; }
        set { playbackRate = Mathf.Clamp(value, MinPlaybackRate, MaxPlaybackRate); }
    }


    public float                RunningTime
    {
        get { return runningTime; }
        set
        {
            runningTime = value;
            if (runningTime <= 0.0f)
                runningTime = 0.0f;

            if (runningTime > duration)
                runningTime = duration;

            if (isFreshPlayback)
            {
                foreach (TimelineContainer timelineContainer in TimelineContainers)
                {
                    foreach (TimelineBase timeline in timelineContainer.Timelines)
                        timeline.StartTimeline();
                }
                isFreshPlayback = false;
            }

            foreach (TimelineContainer timelineContainer in TimelineContainers)
            {
                timelineContainer.ManuallySetTime(RunningTime);
                timelineContainer.ProcessTimelines(RunningTime, PlaybackRate);
            }
        }
    }
    #endregion


    private TimelineContainer[] timelineContainers;
    public TimelineContainer[] TimelineContainers
    {
        get
        {
            if (timelineContainers == null)
                timelineContainers = GetComponentsInChildren<TimelineContainer>();
            return timelineContainers;
        }
    }

    public int TimelineContainerCount
    {
        get
        {
            if (timelineContainers == null)
                return 0;
            return TimelineContainers.Length;
        }
    }

    public TimelineContainer[] SortedTimelineContainers
    {
        get
        {
            var timelineContainers = TimelineContainers;
            Array.Sort(timelineContainers, TimelineContainer.Comparer);
            return timelineContainers;
        }
    }

    public void Play()
    {
        // Start or resume our playback.
        if (isFreshPlayback)
        {
            foreach (TimelineContainer timelineContainer in TimelineContainers)
            {
                foreach (TimelineBase timeline in timelineContainer.Timelines)
                {
                    timeline.StartTimeline();
                }
            }
            isFreshPlayback = false;
        }
        else
        {
            foreach (TimelineContainer timelineContainer in TimelineContainers)
            {
                foreach (TimelineBase timeline in timelineContainer.Timelines)
                {
                    timeline.ResumeTimeline();
                }
            }
        }

        playing             = true;
        previousTime        = Time.time;
    }

    public void Pause( )
    {
        playing = false;
        foreach (TimelineContainer timelineContainer in TimelineContainers)
        {
            foreach (TimelineBase timeline in timelineContainer.Timelines)
            {
                timeline.PauseTimeline();
            }
        }
    }

    public void Stop()
    {
        foreach (TimelineContainer timelineContainer in TimelineContainers)
        {
            foreach (TimelineBase timeline in timelineContainer.Timelines)
            {
                if (timeline != null && timeline.AffectedObject != null)
                    timeline.StopTimeline();
            }
        }

        isFreshPlayback = true;
        playing         = false;
        runningTime     = 0.0f;
    }

    private void End()
    {
        if (isLoopingSequence || isPingPongingSequence)
            return;

        foreach (TimelineContainer timelineContainer in TimelineContainers)
        {
            foreach (TimelineBase timeline in timelineContainer.Timelines)
            {
                if (timeline.AffectedObject != null)
                    timeline.EndTimeline();
            }
        }
    }


    public TimelineBase[] SortedTimelines
    {
        get
        {
            List<TimelineBase> list = new List<TimelineBase>();
            foreach (TimelineContainer contain in TimelineContainers)
            {
                list.AddRange(contain.Timelines);
            }
            return list.ToArray();
        }
    }


    #region ExtensionRegion
    /// <summary>
    /// 创建时间线容器
    /// </summary>
    public TimelineContainer CreateNewTimelineContainer(Transform affectedObject)
    {
        GameObject newTimelineContainerGO   = new GameObject("TimelineContainer " + affectedObject.name);
        newTimelineContainerGO.transform.parent = transform;

        TimelineContainer Container         = newTimelineContainerGO.AddComponent<TimelineContainer>();
        Container.AffectedObject            = affectedObject;
        Container.Sequence                  = this;

        int highestIndex                    = 0;
        foreach (TimelineContainer ourTimelineContainer in TimelineContainers)
        {
            ourTimelineContainer.Index      = highestIndex;
            if( ourTimelineContainer != null && ourTimelineContainer.Timelines.Length <= 0 )
            {
                ourTimelineContainer.AddNewTimeline( TimeLineType.Animation );
            }
            highestIndex++;
        }

        return Container;
    }
    #endregion
}

