using System;
using System.Collections.Generic;
namespace Solarmax
{
	public class ChestConfig
	{
		public System.Int32 id = 0;
		public System.Int32 type = 0;
		public System.String iconopen = string.Empty;
		public System.String icon = string.Empty;
		public System.Int32 ladderlevel = 0;
		public System.String name = string.Empty;
		public System.Int32 costtime = 0;
		public System.Int32 costdiamond = 0;
		public System.Int32 trophy = 0;
		public System.Int32 dropitem = 0;
		public System.Int32 dropcoin = 0;
		public System.Int32 mincoin = 0;
		public System.Int32 maxcoin = 0;
		public System.Int32 itemnum = 0;
		public System.String itemgather = string.Empty;
	}
	public class ChestConfigProvider : Singleton<ChestConfigProvider>, IDataProvider
	{
		private List<ChestConfig> dataList = new List<ChestConfig>();
		public string Path()
		{
			return "data/chest.txt";
		}

        public bool IsXML()
        {
            return false;
        }

		public void Load()
		{
			dataList.Clear ();
			
			ChestConfig item = null;
			while (!FileReader.IsEnd())
			{
				FileReader.ReadLine();
				item = new ChestConfig();
				item.id = FileReader.ReadInt();
				item.type = FileReader.ReadInt();
				item.iconopen = FileReader.ReadString();
				item.icon = FileReader.ReadString();
				item.ladderlevel = FileReader.ReadInt();
				item.name = FileReader.ReadString();
				item.costtime = FileReader.ReadInt();
				item.costdiamond = FileReader.ReadInt();
				item.trophy = FileReader.ReadInt();
				item.dropitem = FileReader.ReadInt();
				item.dropcoin = FileReader.ReadInt();
				item.mincoin = FileReader.ReadInt();
				item.maxcoin = FileReader.ReadInt();
				item.itemnum = FileReader.ReadInt();
				item.itemgather = FileReader.ReadString();
				dataList.Add(item);
			}
		}


		public bool Verify()
		{
			return true;
		}


		public List<ChestConfig> GetAllData()
		{
			return dataList;
		}


		public ChestConfig GetData(System.Int32 id)
		{
			ChestConfig ret = null;
			for (int i = 0; i < dataList.Count; ++i)
			{
				if (dataList [i].id.Equals (id)) {
					ret = dataList [i];
					break;
				}
			}
			return ret;
		}
	}
}
