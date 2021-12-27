using System;
using System.Collections.Generic;
using System.IO;
using NetMessage;
using Solarmax;





/// <summary>
/// 消息到消息處理
/// </summary>
public delegate void MessageHandler(int msgId, PacketEvent message);
public class PacketHandler
{
	public int              iPacketType;
	public MessageHandler   mHandler;

	public int GetPacketType()
	{
		return iPacketType;
	}

	public bool OnPacketHandler(byte[] data)
	{

        using (MemoryStream stream = new MemoryStream(data, 0, data.Length))
        {
            PacketEvent proto = new PacketEvent(iPacketType, stream);
            mHandler(iPacketType, proto);
        }
		return true;
	}
}




public class PacketHandlerManager
{
    private readonly Dictionary<ushort, object> opcodeTypes = new Dictionary<ushort, object>();
	private Dictionary<int, PacketHandler>      mHandlerDict = null;
    public PacketHandlerManager()
    {
		mHandlerDict = new Dictionary<int, PacketHandler> ();
    }

    public bool Init()
    {

        return true;
    }

    public void Tick(float interval)
    {
        // don't need
    }

    public void Destroy()
    {
        mHandlerDict.Clear();
    }

	public void RegisterHandler(int packetType, MessageHandler handler)
	{
		if (null != handler)
		{
            PacketHandler packethandler     = new PacketHandler();
			packethandler.iPacketType       = packetType;
			packethandler.mHandler          = handler;
			mHandlerDict.Add (packetType, packethandler);
		}
	}

    public bool DispatchHandler(int type, byte[] data)
    {
        if (data != null && mHandlerDict.ContainsKey(type))
        {
            PacketHandler handler = mHandlerDict[type];
            if (null != handler)
     		{
				return handler.OnPacketHandler (data);
            }
        }

        return false;
    }


    public object GetInstance(ushort opcode)
    {
        return this.opcodeTypes[opcode];
    }
}
