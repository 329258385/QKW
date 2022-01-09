using System;
using System.Collections.Generic;






namespace Solarmax
{
	public class MapConfig : ICloneable
	{
		public System.String		id { get; set; }
		public System.String		linetags { get; set; }
		public System.Int32			player_count { get; set; }
		public bool					vertical { get; set; }
		public string				audio { get; set; }
		public string				buildingIds { get; set; }
		public string				playerIds { get; set; }
        public int					reward { get; set; }

		public List<MapBuildingConfig>	builds = new List<MapBuildingConfig>();
		public List<MapPlayerConfig>	players = new List<MapPlayerConfig>();
		public List< List<string> >		lines = new List< List<string> > ();

		public object Clone ()
		{
			MapConfig mc			= new MapConfig ();
			mc.id					= this.id;
			mc.linetags				= this.linetags;
			mc.player_count			= this.player_count;
			mc.vertical				= this.vertical;
			mc.audio				= this.audio;
			mc.buildingIds			= this.buildingIds;
			mc.playerIds			= this.playerIds;

			foreach (var mbc in this.builds) 
			{
				mc.builds.Add (mbc.Clone () as MapBuildingConfig);
			}
			foreach (var mpc in this.players) 
			{
				mc.players.Add (mpc.Clone () as MapPlayerConfig);
			}
            reward = 10;

            mc.lines.AddRange (this.lines);

			return mc;
		}
	}
	public class MapConfigProvider : Singleton<MapConfigProvider>, IDataProvider
	{
		private Dictionary<string, MapConfig> dataDict = new Dictionary<string, MapConfig>();
		private Dictionary<string, MapConfig> dataDictExtra = new Dictionary<string, MapConfig>();
		public string Path()
		{
			return "data/map.txt";
		}

        public bool IsXML()
        {
            return false;
        }


		public void Load()
		{
			dataDict.Clear ();

			MapConfig item = null;
			while (!FileReader.IsEnd())
			{
				FileReader.ReadLine();
				item = new MapConfig();
				item.id = FileReader.ReadString();
				item.linetags = FileReader.ReadString ();
				item.player_count = FileReader.ReadInt ();
				item.vertical = FileReader.ReadBoolean ();
				item.audio = FileReader.ReadString ();
				item.buildingIds = FileReader.ReadString ();
				item.playerIds = FileReader.ReadString ();
                item.reward = 10;
				dataDict.Add (item.id, item);
			}

			Verify();
		}

		public bool Verify()
		{
			foreach (var map in dataDict.Values)
			{

				// 建筑物
				string[] buildingIdArray = map.buildingIds.Split(',');
				for (int i = 0; i < buildingIdArray.Length; ++i)
				{
					map.builds.Add(MapBuildingConfigProvider.Instance.GetData(string.Format("{0}_{1}", map.id, buildingIdArray[i])));
				}

				// 玩家
				string[] playerIdArray = map.playerIds.Split(',');
				for (int i = 0; i < playerIdArray.Length; ++i)
				{
					map.players.Add(MapPlayerConfigProvider.Instance.GetData(string.Format("{0}_{1}", map.id, playerIdArray[i])));
				}

				// 障碍物线
				if (!string.IsNullOrEmpty(map.linetags))
				{
					string[] a = map.linetags.Split(';');
					for (int i = 0; i < a.Length; ++i)
					{
						List<string> tags = new List<string>();
						string[] b = a[i].Split(',');
						for (int j = 0; j < b.Length; ++j)
						{
							tags.Add(b[j]);
						}
						map.lines.Add(tags);
					}
				}
			}

			return true;
		}


		public Dictionary<string, MapConfig> GetAllData()
		{
			return dataDict;
		}
		public MapConfig GetData(System.String id)
		{
			MapConfig ret = null;
			dataDict.TryGetValue (id, out ret);
			return ret;
		}

		/// <summary>
		/// 额外编辑所有地图
		/// </summary>
		public Dictionary<string, MapConfig> GetAllDataExtra()
		{
			return dataDictExtra;
		}
	}
}
