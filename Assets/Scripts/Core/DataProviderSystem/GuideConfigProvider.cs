using System;
using System.Collections.Generic;

namespace Solarmax
{

    /// <summary>
    /// 引导出发类型的类型
    /// </summary>
    public enum GuildCondition
    {
        GC_Level,           // 打开关卡
        GC_Ui,              // 打开界面
        GC_Auto,            // 上一个引导自动触发
    };


    public enum GuildEndEvent
    {
        Slide           = 0,              // 滑动
        touch           = 1,              // 单击
        rename          = 2,
        pickfreebox     = 3,
        receivebox      = 4,
        startpvp        = 5,
        unlockbox       = 6,
        openlockx       = 7,
        openbox         = 8,
        pickbox         = 9,
        getrace         = 10,
        useskill        = 11,
       
    };

    public enum BtnMoveType
    {
        BMT_Null,               // 不移动
        BMT_MoveTarget,         // 循环移动
        BMT_ResetMove,          // 从头开始移动
    };

  
    public enum Coordinates
    {
        CD_2D,
        CD_3D,
    };
     public class CTagGuideConfig
    {
        public int              ID;
        public GuildCondition   startCondition;
        public BtnMoveType      moveType;
        public int              windowHashCode;
        public string           window;
        public string           ctrlname;
        public float            srcX;
        public float            srcY;
        public float            dstX;
        public float            dstY;
        public float            angle;
        public float            duration;
        public Coordinates      coordsinates;         
        public int              nextID;
        public GuildEndEvent    endCondition;
    };


    public class GuideDataProvider : Singleton<GuideDataProvider>, IDataProvider
    {
       

        private Dictionary<int, CTagGuideConfig> mDataList = new Dictionary<int, CTagGuideConfig>();

        public GuideDataProvider()
        {

        }

        public string Path()
        {
            return "data/playerguide.txt";
        }

        public bool IsXML()
        {
            return false;
        }

        public void Load()
        {
			mDataList.Clear ();

            CTagGuideConfig item = null;
            while (!FileReader.IsEnd())
            {
                FileReader.ReadLine();
                item            = new CTagGuideConfig();
                item.ID         = FileReader.ReadInt( );
                item.startCondition = (GuildCondition)FileReader.ReadInt();

                string window   = FileReader.ReadString();
                string[] strSub = window.Split(',');
                item.window     = strSub[0];
                item.windowHashCode = strSub[0].GetHashCode();
                if (strSub.Length > 1)
                    item.ctrlname = strSub[1];
                else
                    item.ctrlname = string.Empty;
                item.srcX       = FileReader.ReadFloat();
                item.srcY       = FileReader.ReadFloat();
                item.dstX       = FileReader.ReadFloat();
                item.dstY       = FileReader.ReadFloat();
                item.angle      = FileReader.ReadFloat();
                item.duration   = FileReader.ReadFloat();


                item.coordsinates = (Coordinates)FileReader.ReadInt();
                item.moveType   = (BtnMoveType)FileReader.ReadInt();
                item.nextID     = FileReader.ReadInt();
                item.endCondition = (GuildEndEvent)FileReader.ReadInt();
                mDataList.Add(item.ID, item);
            }
        }

        public bool Verify()
        {
	        return true;
        }

        public CTagGuideConfig GetValue(int id)
		{
            CTagGuideConfig config;
            mDataList.TryGetValue( id, out config );
            return config;
		}

        public CTagGuideConfig GetGuideConfigByCondition( GuildCondition eCon, string strCondition, int nCurCompltedID )
        {
            int hash = strCondition.GetHashCode();
            foreach( var item in mDataList.Values )
            {
                if (item.startCondition == eCon && item.windowHashCode == hash && item.ID > nCurCompltedID )
                    return item;
            }
            return null;
		}
		public void LoadExtraData ()
		{

		}
    }
}
