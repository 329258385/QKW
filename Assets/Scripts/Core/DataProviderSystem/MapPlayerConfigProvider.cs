using System;
using System.Collections.Generic;
namespace Solarmax
{
	public class MapPlayerConfig : ICloneable
	{
		public System.String		id { get; set; }
		public System.String		tag { get; set; }
		public System.Int32			ship { get; set; }
		public System.Int32			camption { get; set; }

		public object Clone ()
		{
			MapPlayerConfig mpc = new MapPlayerConfig ();
			mpc.id              = this.id;
			mpc.tag             = this.tag;
			mpc.ship            = this.ship;
			mpc.camption        = this.camption;
			return mpc;
		}
	}
	public class MapPlayerConfigProvider : Singleton<MapPlayerConfigProvider>, IDataProvider
	{
		private Dictionary<string, MapPlayerConfig> dataDict = new Dictionary<string, MapPlayerConfig>();
		public string Path()
		{
			return "data/mapplayer.txt";
		}

        public bool IsXML()
        {
            return false;
        }


		public void Load()
		{
			dataDict.Clear ();

			
			MapPlayerConfig item = null;
			while (!FileReader.IsEnd())
			{
				FileReader.ReadLine();
				item = new MapPlayerConfig();
				item.id = FileReader.ReadString();
				item.tag = FileReader.ReadString();
				item.ship = FileReader.ReadInt ();
				item.camption = FileReader.ReadInt ();

				dataDict.Add (item.id, item);
			}
		}
		public bool Verify()
		{
			return true;
		}
		public Dictionary<string, MapPlayerConfig> GetAllData()
		{
			return dataDict;
		}
		public MapPlayerConfig GetData(System.String id)
		{
			MapPlayerConfig ret = null;
			dataDict.TryGetValue (id, out ret);
			return ret;
		}
		public void LoadExtraData ()
		{
			MapPlayerConfig item = null;
			while (!FileReader.IsEnd())
			{
				FileReader.ReadLine();
				item = new MapPlayerConfig();
				item.id = FileReader.ReadString();
				item.tag = FileReader.ReadString();
				item.ship = FileReader.ReadInt ();
				item.camption = FileReader.ReadInt ();

				if (dataDict.ContainsKey (item.id)) {
					dataDict [item.id] = item;
				} else {
					dataDict.Add (item.id, item);
				}
			}
		}
	}
}
