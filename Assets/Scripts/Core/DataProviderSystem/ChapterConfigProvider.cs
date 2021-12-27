using System;
using System.Collections.Generic;

namespace Solarmax
{
	public class ChapterConfig
	{
		public string id;
		public int needStar;//需要星星的总数
		public string dependChapter;
		public string name;//名称
		public string describe;//描述
		public string starChart;//背景图片
	}

	public class ChapterConfigProvider : Singleton<ChapterConfigProvider>, IDataProvider
	{

		private List<ChapterConfig> dataList = new List<ChapterConfig>();

		public ChapterConfigProvider()
		{

		}

		public string Path()
		{
			return  "data/Chapter.txt";
		}

        public bool IsXML()
        {
            return false;
        }

		public void Load()
		{
			dataList.Clear ();

			ChapterConfig item = null;
			while (!FileReader.IsEnd())
			{
				FileReader.ReadLine();
				item = new ChapterConfig();
				item.id = FileReader.ReadString ("id");
				item.needStar = FileReader.ReadInt ("needstar");
				item.dependChapter = FileReader.ReadString ("dependchapter");
				item.name = FileReader.ReadString ("name");
				item.describe = FileReader.ReadString ("describe");
				item.starChart = FileReader.ReadString ("starchart");

				dataList.Add(item);
			}
		}

		public bool Verify()
		{
			return true;
		}
		public List<ChapterConfig> GetAllData()
		{
			return dataList;
		}
		public ChapterConfig GetData(string id)
		{
			ChapterConfig ret = null;
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
