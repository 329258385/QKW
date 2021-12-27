using System;
using GameCore.Loader;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;








namespace Solarmax
{

    /// <summary>
    /// 道具使用的状态， 任何状态、 飞行中的
    /// </summary>
    public enum USESTATS
    {
        all,
        fly,
        idle,
    }


	public class ArticleConfig : ICfgEntry
    {
		public System.Int32     typeID  = 0;
		public System.String    name    = string.Empty;
		public System.String    icon    = string.Empty;
		public System.String    desc    = string.Empty;
        public System.Int32     fun     = 0;
        public System.Int32     overMax = 999;
        public System.Int32     cd      = 10;
        public bool Load( XElement element)
        {
            typeID          = Convert.ToInt32(element.Attribute("id").Value);
            name            = element.Attribute("name").Value;
            icon            = element.Attribute("icon").Value;
            desc            = element.Attribute("desc").Value;
            fun             = Convert.ToInt32(element.Attribute("function").Value);
            overMax         = Convert.ToInt32(element.Attribute("over").Value);
            return true;
        }
	}



	public class ArticleConfigProvider : Singleton<ArticleConfigProvider>, IDataProvider
	{
		private List<ArticleConfig> dataList = new List<ArticleConfig>();
		public string Path()
		{
			return "/data/item.xml";
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
                    var xElement = xmlDoc.Element("items");
                    if (xElement == null)
                        return;


                    var elements = xElement.Elements("item");
                    foreach (var em in elements)
                    {
                        ArticleConfig item = new ArticleConfig();
                        if (item.Load(em))
                        {
                            dataList.Add(item);

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


		public List<ArticleConfig> GetAllData()
		{
			return dataList;
		}


		public ArticleConfig GetData(System.Int32 id)
		{
            ArticleConfig ret = null;
			for (int i = 0; i < dataList.Count; ++i)
			{
				if (dataList [i].typeID.Equals (id)) {
					ret = dataList [i];
					break;
				}
			}
			return ret;
		}
	}
}
