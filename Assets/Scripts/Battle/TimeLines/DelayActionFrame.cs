using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TimeLines
{
    public class DelayActionFrame
    {
        public enum                 DelayType { Time, Frame }

        private float               Time;

        private List<int>           removeIndex = new List<int>();

        public struct ActionInfo
        {
            public DelayType        delayType;
            public int              delayFrame;
            public float            addTime;
            public float            delayTime;
            public System.Action    action;
            public Func<bool>       IsFnishFunc;
            public bool             forever;
            public void Restore()
            {
                delayType           = DelayType.Time;
                action              = null;
                IsFnishFunc         = null;
                delayFrame          = 0;
                addTime             = 0;
                delayTime           = 0;
                forever             = false;
            }
        }

        public List<ActionInfo>     actions = new List<ActionInfo>();

        public void AddDelayAction( float delayTme, Action action )
        {
            ActionInfo actionInfo   = new ActionInfo();
            actionInfo.delayType    = DelayType.Time;
            actionInfo.addTime      = Time;
            actionInfo.delayTime    = delayTme;
            actionInfo.action       = action;
            actionInfo.forever      = false;
            actions.Add(actionInfo);
        }

        protected void InvokeAction(System.Action action)
        {
            if ((action.Target == null ||action.Target.Equals(null)) && !action.Method.IsStatic)
            {
                return;
            }

            try
            {
                action();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public void Tick( float delta )
        {
            Time += delta;
            removeIndex.Clear();
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].forever)
                {
                    if (actions[i].IsFnishFunc != null)
                    {
                        if (actions[i].IsFnishFunc())
                        {
                            InvokeAction(actions[i].action);
                            removeIndex.Add(i);
                        }
                    }
                    else
                    {
                        InvokeAction(actions[i].action);
                    }

                }
                else
                {
                    if (actions[i].delayType == DelayType.Time)
                    {
                        if (Time > actions[i].addTime + actions[i].delayTime)
                        {
                            InvokeAction(actions[i].action);
                            removeIndex.Add(i);
                        }
                    }
                    else
                    {
                        ActionInfo actionInfo = actions[i];
                        actionInfo.delayFrame--;
                        actions[i] = actionInfo;
                        if (actions[i].delayFrame <= 0)
                        {
                            InvokeAction(actions[i].action);
                            removeIndex.Add(i);
                        }
                    }
                }
            }
        }
    }
}
