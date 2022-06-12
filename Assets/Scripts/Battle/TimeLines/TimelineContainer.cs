using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;






public class TimelineContainer : MonoBehaviour
{
    public static int Comparer( TimelineContainer a, TimelineContainer b )
    {
        return (a.index.CompareTo(b.index));
    }

    #region Member Variables
    [SerializeField]
    private Transform               affectedObject = null;

    [SerializeField]
    private string                  affectedObjectPath;

    [SerializeField]
    private int                     index = -1;

    [SerializeField]
    public ModelTargetType          modelType;
    #endregion

    /// <summary>
    /// 得到时间线容器作用对象
    /// </summary>
    public Transform                AffectedObject
    {
        get
        {
            if (affectedObject == null && affectedObjectPath != string.Empty)
            {
                var foundGameObject = GameObject.Find(affectedObjectPath);
                if (foundGameObject)
                {
                    affectedObject = foundGameObject.transform;
                    foreach (var timeline in Timelines)
                        timeline.LateBindAffectedObjectInScene(affectedObject);
                }
            }

            return affectedObject;
        }
        set
        {
            affectedObject = value;
        }
    }

    /// <summary>
    /// 关联的时间序列
    /// </summary>
    private TimeLineSequencer           sequence;
    public TimeLineSequencer            Sequence
    {
        get
        {
            return sequence;
        }
        set
        {
            sequence = value;
        }
    }

    /// <summary>
    /// 包含的所有的时间线
    /// </summary>
    private List<TimelineBase>          timelines = new List<TimelineBase>();
    public TimelineBase[]               Timelines
    {
        get
        {
            return timelines.ToArray();
        }
    }

    public int                          Index
    {
        get { return index; }
        set { index = value; }
    }

    public string                       AffectedObjectPath
    {
        get { return affectedObjectPath; }
        private set { affectedObjectPath = value; }
    }


    public TimelineBase AddNewTimeline(TimeLineType type)
    {
        TimelineBase timeline           = null;
        string name                     = Enum.GetName(typeof(TimeLineType), type);
        UnityEngine.Transform line      = transform.Find(name + "Timeline for " + affectedObject.name);
        if (line == null)
        {
            GameObject newTimeline      = new GameObject(name + "Timeline for " + affectedObject.name);
            newTimeline.transform.parent = transform;
            line                        = newTimeline.transform;
        }

        if( type == TimeLineType.Animation )
        {
            timeline                    = line.gameObject.AddComponent<TimelineAnimation>();
        }
        timelines.Add(timeline);
        return timeline;
    }

    public void ProcessTimelines(float sequencerTime, float playbackRate)
    {
        foreach (TimelineBase timeline in Timelines)
            timeline.Process(sequencerTime, playbackRate);
    }

    public void SkipTimelineTo(float sequencerTime)
    {
        foreach (TimelineBase timeline in Timelines)
            timeline.SkipTimelineTo(sequencerTime);
    }

    public void ManuallySetTime(float sequencerTime)
    {
        foreach (TimelineBase timeline in Timelines)
            timeline.ManuallySetTime(sequencerTime);
    }
    

    public void ResetCachedData()
    {
        sequence = null;
        timelines = null;
        foreach (var timeline in Timelines)
            timeline.ResetCachedData();
    }
}
