using System;
using GameCore.Loader;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;








namespace Solarmax
{

 
	public class BuildUpgradeConfig : ICfgEntry
    {
        public System.Int32     id           = 0;
		public System.Int32     buildType    = 0;
        public System.Int32     level        = 0;

        public System.Int32     condition    = 0;
        public System.Int32     needTime     = 0;
        public System.Int32     needMoney    = 0;
        public System.Int32     needDiamond  = 0;
        public System.Int32     needItem     = 0;
        public System.Int32     needCount    = 0;


        public System.String    name    = string.Empty;
		public System.String    icon    = string.Empty;
		public System.String    desc    = string.Empty;

        public bool Load( XElement element)
        {
            id              = Convert.ToInt32(element.Attribute("id").Value);
            buildType       = Convert.ToInt32(element.Attribute("buildType").Value);
            level           = Convert.ToInt32(element.Attribute("level").Value);

            condition       = Convert.ToInt32(element.Attribute("condition").Value);
            needTime        = Convert.ToInt32(element.Attribute("needTime").Value);
            needMoney       = Convert.ToInt32(element.Attribute("needMoney").Value);
            needDiamond     = Convert.ToInt32(element.Attribute("needDiamond").Value);
            needItem        = Convert.ToInt32(element.Attribute("needItem").Value);
            needCount       = Convert.ToInt32(element.Attribute("needCount").Value);

            name            = element.Attribute("name").Value;
            icon            = element.Attribute("icon").Value;
            desc            = element.Attribute("desc").Value;

            return true;
        }
	}


    /// <summary>
    /// 物品功能
    /// </summary>
    public enum ArticleFunction
    {

    }


	public class BuildUpgradeConfigProvider : Singleton<BuildUpgradeConfigProvider>, IDataProvider
	{
		private List<BuildUpgradeConfig> dataList = new List<BuildUpgradeConfig>();
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
                    var xElement = xmlDoc.Element("buildconfigs");
                    if (xElement == null)
                        return;


                    var elements = xElement.Elements("buildconfig");
                    foreach (var em in elements)
                    {
                        BuildUpgradeConfig item = new BuildUpgradeConfig();
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


		public List<BuildUpgradeConfig> GetAllData()
		{
			return dataList;
		}


		public BuildUpgradeConfig GetData(System.Int32 buildType )
		{
            BuildUpgradeConfig ret = null;
			for (int i = 0; i < dataList.Count; ++i)
			{
				if (dataList [i].buildType == buildType ) {
					ret = dataList [i];
					break;
				}
			}
			return ret;
		}
	}
}
