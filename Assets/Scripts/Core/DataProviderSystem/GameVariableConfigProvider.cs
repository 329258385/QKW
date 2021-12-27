using System;
using GameCore.Loader;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;








namespace Solarmax
{
	public class GameVariableConfig : ICfgEntry
    {
		public System.Int32     id = 0;
		public string           value = string.Empty;
		public string           desc = string.Empty;


        public bool Load(XElement element)
        {
            id                  = Convert.ToInt32(element.Attribute("id").Value);
            value               = element.Attribute("value").Value;
            desc                = element.Attribute("desc").Value;
            return true;
        }
    }



	public class GameVariableConfigProvider : Singleton<GameVariableConfigProvider>, IDataProvider
	{
		private List<GameVariableConfig> dataList = new List<GameVariableConfig>();
		public string Path()
		{
			return "/data/gameconfig.xml";
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
                    var xElement = xmlDoc.Element("gameconfigs");
                    if (xElement == null)
                        return;


                    var elements = xElement.Elements("gameconfig");
                    foreach (var em in elements)
                    {
                        GameVariableConfig item = new GameVariableConfig();
                        if (item.Load(em))
                        {
                            dataList.Add(item);

                        }
                    }
                }
            }


            catch (Exception e)
            {
                LoggerSystem.Instance.Error("data/gameconfig.xml resource failed " + e.ToString());
            }
        }

        public bool IsXML()
        {
            return true;
        }


		public bool Verify()
		{
			return true;
		}

		public List<GameVariableConfig> GetAllData()
		{
			return dataList;
		}

		public string GetData(System.Int32 id)
		{
			GameVariableConfig ret = null;
			for (int i = 0; i < dataList.Count; ++i)
			{
				if (dataList [i].id.Equals (id)) {
					ret = dataList [i];
					break;
				}
			}
			return ret.value;
		}
	}
}
