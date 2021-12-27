using System;
using GameCore.Loader;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;






namespace Solarmax
{
    public class DictionaryConfig : ICfgEntry
    {
        public System.Int32 id      = 0;
        public System.String value  = string.Empty;
        public System.String icon   = string.Empty;
        public System.String desc   = string.Empty;

        public bool Load(XElement element)
        {
            id          = Convert.ToInt32(element.Attribute("id").Value);
            value       = element.Attribute("value").Value;
            return true;
        }
    }

    public class DictionaryDataProvider : Singleton<DictionaryDataProvider>, IDataProvider
    {
		private Dictionary<int, DictionaryConfig> dataList = new Dictionary<int, DictionaryConfig>();
        public DictionaryDataProvider()
        {

        }

        public string Path()
        {
            return "/data/dictionary.xml";
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
                    var xElement = xmlDoc.Element("dictionarys");
                    if (xElement == null)
                        return;


                    var elements = xElement.Elements("item");
                    foreach (var em in elements)
                    {
                        DictionaryConfig item = new DictionaryConfig();
                        if (item.Load(em))
                        {
                            dataList.Add(item.id, item );
                        }
                    }
                }
            }


            catch (Exception e)
            {
                LoggerSystem.Instance.Error("data/item.xml resource failed " + e.ToString());
            }
        }

        public bool Verify()
        {

	        return true;
        }

		public string GetData(int id)
		{
            DictionaryConfig config = null;
			if (dataList.TryGetValue (id, out config)) {
				return config.value;
			}

			return string.Empty;
		}

		public static string GetValue(int id)
		{
			return Instance.GetData(id);
		}

		public static string Format (int id, params object[] args)
		{
			string str = GetValue (id);
			return string.Format (str, args);
		}

    }
}
