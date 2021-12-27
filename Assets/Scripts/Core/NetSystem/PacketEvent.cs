using System;
using System.Collections.Generic;







namespace Solarmax
{

    /// <summary>
    /// 到各个消息处理接口的参数
    /// <summary>
    public struct PacketEvent
    {
        private int     _id;
        private object  _data;

        //---------------------------------------------------------------------
        // 双参构造
        //---------------------------------------------------------------------
        public PacketEvent(int id, object data)
        {
            _id     = id;
            _data   = data;
        }

        public int id
        {
            get { return _id; }
        }

        public object Data
        {
            get { return _data; }
        }
    }
}
