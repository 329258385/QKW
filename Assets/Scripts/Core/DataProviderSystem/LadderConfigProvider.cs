using System;
using GameCore.Loader;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;






namespace Solarmax
{
	public class LadderConfig : ICfgEntry
	{
		public System.Int32     ladderlevel = 0;
		public System.String    itemgather = string.Empty;
		public System.Int32     points = 0;
		public System.String    laddername = string.Empty;
		public System.String    icon = string.Empty;
        public System.String    level = string.Empty;
        public int              winmaxcoin = 0;
		public int              winmincoin = 0;


        public bool Load( XElement element)
        {
            ladderlevel     = Convert.ToInt32(element.Attribute("ladderlevel").Value);
            laddername      = element.Attribute("laddername").Value;
            icon            = element.Attribute("icon").Value;
            level           = element.Attribute("showlevel").Value;

            itemgather      = element.Attribute("itemgather").Value;
            points          = Convert.ToInt32(element.Attribute("points").Value);
            winmaxcoin      = Convert.ToInt32(element.Attribute("winmaxcoin").Value);
            winmincoin      = Convert.ToInt32(element.Attribute("winmincoin").Value);

            return true;
        }
    }
	public class LadderConfigProvider : Singleton<LadderConfigProvider>, IDataProvider
	{
		private List<LadderConfig> dataList = new List<LadderConfig>();
		public string Path()
		{
			return "/data/ladder.xml";
		}

        public bool IsXML()
        {
            return true;
        }


		public void Load()
		{
			dataList.Clear ();
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
                    var xElement = xmlDoc.Element("ladders");
                    if (xElement == null)
                        return;


                    var elements = xElement.Elements("ladder");
                    foreach (var em in elements)
                    {
                        LadderConfig item = new LadderConfig();
                        if (item.Load(em))
                        {
                            dataList.Add(item);

                        }
                    }
                }
            }
            catch (Exception e)
            {
                LoggerSystem.Instance.Error("data/WarShip.xml resource failed " + e.ToString());
            }
        }


		public bool Verify()
		{
			return true;
		}


		public List<LadderConfig> GetAllData()
		{
			return dataList;
		}


		public LadderConfig GetData(System.Int32 ladderlevel)
		{
			LadderConfig ret = null;
			for (int i = 0; i < dataList.Count; ++i)
			{
				if (dataList [i].ladderlevel.Equals (ladderlevel)) {
					ret = dataList [i];
					break;
				}
			}
			return ret;
		}
	}
}
