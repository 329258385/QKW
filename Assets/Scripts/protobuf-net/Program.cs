using System;
using System.IO;

namespace ProtoBuf
{
    /// <summary>
    /// 朋友数据
    /// </summary>
    [ProtoContract]    
    public class FriendData
    {
        /// <summary>
        /// 数据库流水ID
        /// </summary>
        [ProtoMember(1)]        
        public int DbID;

        /// <summary>
        /// 对方的ID
        /// </summary>
        [ProtoMember(2)]           
        public int OtherRoleID;

        /// <summary>
        /// 对方的名称
        /// </summary>
        [ProtoMember(3)]
        public string OtherRoleName;

        /// <summary>
        /// 对方的等级
        /// </summary>
        [ProtoMember(4)]
        public int OtherLevel;

        /// <summary>
        /// 对方的职业
        /// </summary>
        [ProtoMember(5)]
        public int Occupation;

        /// <summary>
        /// 对方的在线状态
        /// </summary>
        [ProtoMember(6)]
        public int OnlineState;

        /// <summary>
        /// 所在的地图编号
        /// </summary>
        [ProtoMember(7)]
        public string Position;

        /// <summary>
        /// 朋友数据类型, 0: 好友 1:黑名单 2: 敌人
        /// </summary>
        [ProtoMember(8)]           
        public int FriendType;
    }
	
	public class Program
	{
		public Program ()
		{
		}
		
		public static void Main (string[] args)
		{
			try
            {
                byte[] bytesCmd = null;
				
				FriendData data = new FriendData();
				data.DbID = 11111;
              			
			    MemoryStream ms = new MemoryStream();
                Serializer.Serialize<FriendData>(ms, data);
                bytesCmd = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(bytesCmd, 0, bytesCmd.Length);
                ms.Dispose();
                ms = null;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
		}
	}
}

