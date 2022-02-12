using System;
using UnityEngine;




namespace TimeLines
{
    public class ActionArgs : System.Object
    {
        public ActionArgs()
        {

        }

        public enum Events
        {
            OnSearchRangeChange,
        }

        public BattleMember             Target { get; set; }

        public BattleMember             Source { get; set; }

        public Vector2                  TargetPos;
        public int                      SkillID;
        public float                    TimeScale;                  

        public Action                   OnActionFinishd;
        public Action<bool, int>        OnLaunchFinishd;
        public Action                   OnSearchRangeChange;

        private float                   _searchRange;
        public float SearchRange
        {
            get { return _searchRange; }
            set
            {
                _searchRange    = value;
                OnSearchRangeChange();
            }
        }

        public ActionArgs Clone()
        {
            return new ActionArgs
            {
                Target              = Target,
                TargetPos           = TargetPos,
                Source              = Source,
                SkillID             = SkillID,
                _searchRange        = _searchRange,
                OnActionFinishd     = OnActionFinishd,
                OnLaunchFinishd     = OnLaunchFinishd,
                OnSearchRangeChange = OnSearchRangeChange,
            };
        }
    }
}
