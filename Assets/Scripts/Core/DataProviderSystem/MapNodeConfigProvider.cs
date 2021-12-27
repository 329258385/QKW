using System;
using GameCore.Loader;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;




namespace Solarmax
{
	public class MapNodeConfig : ICfgEntry
    {
        public int          id;
        public string       type;
        public int          typeEnum;
        public int          sizeType;
        public float        size;
        public int          hp;
        public int          food;
        public int          createshipnum;
        public float        createship;
        public float        attackrange;
        public float        attackspeed;
        public float        attackpower;
        public float        nodesize;
        public string       perfab;
        public string       skills;

        public bool Load(XElement element)
        {
            id              = Convert.ToInt32(element.Attribute("id").Value);
            type            = element.Attribute("type").Value;
            typeEnum        = Convert.ToInt32(element.Attribute("typeEnum").Value);
            sizeType        = Convert.ToInt32(element.Attribute("sizeType").Value);
            size            = Convert.ToSingle(element.Attribute("size").Value);
            hp              = Convert.ToInt32(element.Attribute("hp").Value);
            food            = Convert.ToInt32(element.Attribute("food").Value);
            createshipnum   = Convert.ToInt32(element.Attribute("createshipnum").Value);
            createship      = Convert.ToSingle(element.Attribute("createship").Value);
            attackrange     = Convert.ToSingle(element.Attribute("attackrange").Value);
            attackspeed     = Convert.ToSingle(element.Attribute("attackspeed").Value);
            attackpower     = Convert.ToSingle(element.Attribute("attackpower").Value);
            nodesize        = Convert.ToSingle(element.Attribute("nodesize").Value);
            perfab          = element.Attribute("perfab").Value;
            skills          = element.Attribute("skills").Value;
            return true;
        }

    }
	public class MapNodeConfigProvider : Singleton<MapNodeConfigProvider>, IDataProvider
	{
		private List<MapNodeConfig> dataList = new List<MapNodeConfig>();
		public string Path()
		{
			return "/data/mapnode.xml";
		}

        public bool IsXML()
        {
            return true;
        }

		public void Load()
		{
            dataList.Clear();
            try
            {
                string url = UtilTools.GetStreamAssetsByPlatform(Path());
                if (string.IsNullOrEmpty(url))
                    return;

                WWW www = new WWW(url);
                while (!www.isDone) ;
                if (!string.IsNullOrEmpty(www.text))
                {
                    XDocument xmlDoc = XDocument.Parse(www.text);
                    var xElement = xmlDoc.Element("mapnodes");
                    if (xElement == null)
                        return;

                    var elements = xElement.Elements("mapnode");
                    foreach (var em in elements)
                    {
                        MapNodeConfig item = new MapNodeConfig();
                        if (item.Load(em))
                        {
                            dataList.Add(item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LoggerSystem.Instance.Error("data/Skillconfig.xml resource failed " + e.ToString());
            }
        }

		public bool Verify()
		{
			return true;
		}

		public List<MapNodeConfig> GetAllData()
		{
			return dataList;
		}

		public MapNodeConfig GetData(string type, int sizeType)
		{
			MapNodeConfig ret = null;
			for (int i = 0; i < dataList.Count; ++i)
			{
				if (dataList [i].type.Equals (type) && dataList[i].sizeType.Equals(sizeType))
					ret = dataList [i];
			}
			return ret;
		}

        public MapNodeConfig GetDataByType( int typeID )
        {
            MapNodeConfig ret = null;
            for (int i = 0; i < dataList.Count; ++i)
            {
                if (dataList[i].id == typeID )
                    ret = dataList[i];
            }
            return ret;
        }

        /// <summary>
        /// 根据类型星舰的配置，但是不支持 start 类型
        /// </summary>
        public MapNodeConfig GetData(string type )
        {
            MapNodeConfig ret = null;
            for (int i = 0; i < dataList.Count; ++i)
            {
                if (dataList[i].type.Equals(type))
                    ret = dataList[i];
            }
            return ret;
        }
	}
}
