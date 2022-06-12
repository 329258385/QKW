using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;





[Serializable]
public class AnimationTrack : ScriptableObject
{
    [SerializeField]
    private List<KeyFrameClipData>  trackClipList = new List<KeyFrameClipData>();
    [SerializeField]
    private TimelineBase            limeLine;
    public TimelineBase             TimeLine
    {
        get { return limeLine; }
        set { limeLine = value; }
    }

    [SerializeField]
    private bool                    enable = true;
    public bool Enable
    {
        get { return enable; }
        set { enable = value; }
    }


    public List<KeyFrameClipData> TrackClips
    {
        get { return trackClipList; }
        private set { trackClipList = value; }
    }


    public void AddClip(KeyFrameClipData clipData)
    {
        if (trackClipList.Contains(clipData))
            throw new Exception("Track already contains Clip");
        clipData.Track = this;
        trackClipList.Add(clipData);
    }

    public void RemoveClip(KeyFrameClipData clipData)
    {
        if (!trackClipList.Contains(clipData))
            throw new Exception("Track doesn't contains Clip");

        trackClipList.Remove(clipData);
    }

    private void SortClips()
    {
        trackClipList = trackClipList.OrderBy(trackClip => trackClip.StartTime).ToList();
    }

    public void SetClipData(List<KeyFrameClipData> JAnimationClipData)
    {
        trackClipList = JAnimationClipData;
    }
}

