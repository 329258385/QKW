using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;







public class TimelineAnimation : TimelineBase
{

    [SerializeField]
    private List<AnimationTrack>    animationsTracks = new List<AnimationTrack>();
    public List<AnimationTrack>     AnimationTracks
    {
        get { return animationsTracks; }
        private set { animationsTracks = value; }
    }


    public void AddTrack(AnimationTrack animationTrack)
    {
        animationTrack.TimeLine = this;
        animationsTracks.Add(animationTrack);
    }


    public void RemoveTrack(AnimationTrack animationTrack)
    {
        animationsTracks.Remove(animationTrack);
    }
}

