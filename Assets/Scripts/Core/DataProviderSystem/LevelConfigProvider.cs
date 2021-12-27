using System;
using System.Collections.Generic;

namespace Solarmax
{
	public class LevelConfig
	{
		public string id;
		public string dependLevel;
		public string chapter;
		public string map;
		public int mainLine;
		public int easyAI;
		public int generalAI;
		public int hardAI;
	}

	public class LevelConfigConfigProvider : Singleton<LevelConfigConfigProvider>, IDataProvider
	{

		private List<LevelConfig> dataList = new List<LevelConfig>();

		public LevelConfigConfigProvider()
		{

		}

		public string Path()
		{
			return "data/Level.txt";
		}

        public bool IsXML()
        {
            return false;
        }


		public void Load()
		{
			dataList.Clear ();

			LevelConfig item = null;
			while (!FileReader.IsEnd())
			{
				FileReader.ReadLine();
				item = new LevelConfig();
				item.id = FileReader.ReadString ("id");
				item.dependLevel = FileReader.ReadString ("dependlevel");
				item.chapter = FileReader.ReadString ("chapter");
				item.map = FileReader.ReadString ("map");
				item.mainLine = FileReader.ReadInt ("mianline");
				item.easyAI = FileReader.ReadInt ("easyAI");
				item.generalAI = FileReader.ReadInt ("generalAI");
				item.hardAI = FileReader.ReadInt ("hardAI");

				dataList.Add(item);
			}
		}

		public bool Verify()
		{
			return true;
		}
		public List<LevelConfig> GetAllData()
		{
			return dataList;
		}
		public LevelConfig GetData(string id)
		{
			LevelConfig ret = null;
			for (int i = 0; i < dataList.Count; ++i)
			{
				if (dataList [i].id == id)
				{
					ret = dataList [i];
					break;
				}
			}
			return ret;
		}
		public void LoadExtraData ()
		{

		}
	}
}
