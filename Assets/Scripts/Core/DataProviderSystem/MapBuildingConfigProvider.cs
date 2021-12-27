using System;
using System.Collections.Generic;
namespace Solarmax
{
	public class MapBuildingConfig : ICloneable
	{
		public System.String id { get; set; }
		public System.String type { get; set; }
		public int size { get; set; }
		public float x { get; set; }
		public float y { get; set; }
		public int camption { get; set; }
		public string tag { get; set; }
		public int orbit { get; set; }
		public string orbitParam1 { get; set; }
		public string orbitParam2 { get; set; }
		public bool orbitClockWise { get; set; }
        public float fAngle { get; set; }
		public object Clone ()
		{
			MapBuildingConfig mbc = new MapBuildingConfig ();
			mbc.id = this.id;
			mbc.type = this.type;
			mbc.size = this.size;
			mbc.x = this.x;
			mbc.y = this.y;
			mbc.camption = this.camption;
			mbc.tag = this.tag;
			mbc.orbit = this.orbit;
			mbc.orbitParam1 = this.orbitParam1;
			mbc.orbitParam2 = this.orbitParam2;
			mbc.orbitClockWise = this.orbitClockWise;
            mbc.fAngle = this.fAngle;
			return mbc;
		}
	}
	public class MapBuildingConfigProvider : Singleton<MapBuildingConfigProvider>, IDataProvider
	{
		private Dictionary<string, MapBuildingConfig> dataDict = new Dictionary<string, MapBuildingConfig>();
		public string Path()
		{
			return "data/mapbuilding.txt";
		}

        public bool IsXML()
        {
            return false;
        }


		public void Load()
		{
			dataDict.Clear ();


			MapBuildingConfig item = null;
			while (!FileReader.IsEnd())
			{
				FileReader.ReadLine();
				item = new MapBuildingConfig();
				item.id = FileReader.ReadString();
				item.type = FileReader.ReadString();
				item.size = FileReader.ReadInt ();
				item.x = FileReader.ReadFloat ();
				item.y = FileReader.ReadFloat ();
				item.camption = FileReader.ReadInt ();
				item.tag = FileReader.ReadString ();
				item.orbit = FileReader.ReadInt ();
				item.orbitParam1 = FileReader.ReadString ();
				item.orbitParam2 = FileReader.ReadString ();
				item.orbitClockWise = FileReader.ReadBoolean ();
                item.fAngle = FileReader.ReadFloat();
				dataDict.Add (item.id, item);
			}
		}
		public bool Verify()
		{
			return true;
		}
		public Dictionary<string, MapBuildingConfig> GetAllData()
		{
			return dataDict;
		}
		public MapBuildingConfig GetData(System.String id)
		{
			MapBuildingConfig ret = null;
			dataDict.TryGetValue (id, out ret);
			return ret;
		}
		public void LoadExtraData ()
		{
			MapBuildingConfig item = null;
			while (!FileReader.IsEnd())
			{
				FileReader.ReadLine();
				item = new MapBuildingConfig();
				item.id = FileReader.ReadString();
				item.type = FileReader.ReadString();
				item.size = FileReader.ReadInt ();
				item.x = FileReader.ReadFloat ();
				item.y = FileReader.ReadFloat ();
				item.camption = FileReader.ReadInt ();
				item.tag = FileReader.ReadString ();
				item.orbit = FileReader.ReadInt ();
				item.orbitParam1 = FileReader.ReadString ();
				item.orbitParam2 = FileReader.ReadString ();
				item.orbitClockWise = FileReader.ReadBoolean ();

				if (dataDict.ContainsKey (item.id)) {
					dataDict [item.id] = item;
				} else {
					dataDict.Add (item.id, item);
				}
			}
		}
	}
}
