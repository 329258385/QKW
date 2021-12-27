using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Solarmax
{

    public interface IPacket
    {

        int GetPacketType();

        Byte[] GetData();
    }
}
