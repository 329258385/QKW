using System;
using System.Collections.Generic;
namespace Solarmax
{
	public class NameConfig
	{
		public int id = 0;
		public System.String name = string.Empty;
	}
	public class NameConfigProvider : Singleton<NameConfigProvider>, IDataProvider
	{
		private List<NameConfig> dataList = new List<NameConfig>();
		public string Path()
		{
			return "data/name.txt";
		}

        public bool IsXML()
        {
            return false;
        }


		public void Load()
		{
			dataList.Clear ();
			
			NameConfig item = null;
			while (!FileReader.IsEnd())
			{
				FileReader.ReadLine();
				item = new NameConfig();
				item.id = FileReader.ReadInt ();
				item.name = FileReader.ReadString();
				dataList.Add(item);
			}
		}
		public bool Verify()
		{
			return true;
		}
		public List<NameConfig> GetAllData()
		{
			return dataList;
		}
		public NameConfig GetData(System.String name)
		{
			NameConfig ret = null;
			for (int i = 0; i < dataList.Count; ++i)
			{
				if (dataList [i].name.Equals (name)) {
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
