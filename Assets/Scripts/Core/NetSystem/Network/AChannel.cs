using System;
using System.IO;
using System.Net;




public enum ChannelType
{
	Connect,
	Accept,
}



public abstract class AChannel
{
	public ChannelType              ChannelType { get; }
	public abstract MemoryStream    Stream { get; }
	public int                      Error { get; set; }

	public IPEndPoint               RemoteAddress { get; protected set; }
    public long                     Id { get; private set; }


    private Action<AChannel, int>   errorCallback;
	public event Action<AChannel, int> ErrorCallback
	{
		add
		{
			this.errorCallback += value;
		}
		remove
		{
			this.errorCallback -= value;
		}
	}
		
	private Action<MemoryStream>      readCallback;
	public event Action<MemoryStream> ReadCallback
	{
		add
		{
			this.readCallback += value;
		}
		remove
		{
			this.readCallback -= value;
		}
	}
		
	protected void OnRead(MemoryStream memoryStream)
	{
		this.readCallback.Invoke(memoryStream);
	}


	protected void OnError(int e)
	{
		this.Error = e;
		this.errorCallback?.Invoke(this, e);
	}


	protected AChannel( ChannelType channelType)
	{
		this.ChannelType = channelType;
	}

    public virtual void Dispose()
    {
    
    }

    public bool IsDisposed
    {
        get
        {
            return this.Id == -1;
        }
    }

    public abstract void        Start();
	public abstract void        Send(MemoryStream stream);
}