using System;
using System.Collections.Generic;






namespace Solarmax
{
    public class PacketFormat
    {
        public static byte[] PACKET_HEAD = { 99, 99 }; //{'c', 'c'};
        public int GetLength(int dataLength)
        {
            //return 2 + 4 + 4 + dataLength;
			return 8 + dataLength;
        }

        // 组装这个包
        public void GenerateBuffer(ref Byte[] dest, IPacket packet)
        {
            Byte[] data     = packet.GetData();
            int iLength     = GetLength(data.Length);
            dest            = new Byte[iLength];

            // 长度
			Byte[] bLength  = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(iLength - 4));
            Array.Copy(bLength, 0, dest, 0, 4);

            // 类型
            int iType       = packet.GetPacketType();
            Byte[] bType    = BitConverter.GetBytes(iType);
            Array.Copy(bType, 0, dest, 4, 4);

            // 数据
            Array.Copy(data, 0, dest, 8, data.Length);
        }

        //  检查当前缓冲区中是否包含一个包
        public bool CheckHavePacket(Byte[] buffer, int offset)
        {
			int length = BitConverter.ToInt32 (buffer, 0);
			length = System.Net.IPAddress.NetworkToHostOrder (length);

			if (length + 4 <= offset) {
				return true;
			}

            return false;
        }

        // 解码这个包
        public bool DecodePacket(Byte[] buffer, ref int packetLength, ref int packetType, ref Byte[] proto)
        {
            do
            {
				int length  = BitConverter.ToInt32(buffer, 0);
				length      = System.Net.IPAddress.NetworkToHostOrder (length);
				if (length < 0)
                    break;

                packetType  = BitConverter.ToInt32(buffer, 4);
                if (packetType < 0)
                    break;

				proto       = new Byte[length - 4];
				Array.Copy(buffer, 8, proto, 0, length - 4);
                if (null == proto)
                    break;

				packetLength = length + 4;
                return true;
            }
            while (false);

			LoggerSystem.Instance.Debug ("解包错误。。。。。。。。。。。。" + packetLength + "  " + packetType);
            return false;
        }
    }
}
