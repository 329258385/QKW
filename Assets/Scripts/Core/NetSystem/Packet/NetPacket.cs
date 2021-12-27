using System;
using System.Collections.Generic;
using System.IO;






namespace Solarmax
{
    public class NetPacket : IPacket
    {
        private int             mType;
        private Byte[]          mProtoBytes;


        public NetPacket(int type)
        {
            mType           = type;
            mProtoBytes     = null;
        }

        public int GetPacketType()
        {
            return (int)mType;
        }

        public Byte[] GetData()
        {
            return mProtoBytes;
        }

        public void EncodeProto<T>( T proto )
        {
            using (MemoryStream stream = new MemoryStream())
            {
                try
                {
                    ProtoBuf.Serializer.Serialize<T>( stream, proto );
                    Byte[] buffer       = stream.GetBuffer();
                    int    lenght       = (int)stream.Length;
                    mProtoBytes         = new byte[lenght];
                    Array.Copy(buffer, 0, mProtoBytes, 0, lenght);
                }

                catch ( Exception )
                {

                }
            }
        }
    }
}
