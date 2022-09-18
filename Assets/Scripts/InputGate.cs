using Solarmax;
using System;
using UnityEngine;


namespace Solarmax
{
    public class InputGate : Singleton<InputGate>, Lifecycle
    {
        private float       _axis;

        public bool Init()
        {
            //EventHandlerGroup.Get().AddEvent(typeof(EventTypeGroup));
            return true;
        }

        public void Destroy()
        {

        }

        public void Tick(float interval)
        {
            var axis    = Input.GetAxis("Mouse ScrollWheel");
            if (axis != 0)
            {
                _axis   = axis;
                //EventHandlerGroup.Get().fireEvent((int)EventTypeGroup.On2TouchMove, this, new EventArgs_SinVal<float>(_axis));
            }
            else
            {
                _axis   = 0;
            }
        }
    }
}
