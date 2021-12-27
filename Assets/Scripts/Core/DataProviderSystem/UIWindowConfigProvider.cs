using System;
using GameCore.Loader;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;






namespace Solarmax
{
    public class UIWindowConfig : ICfgEntry
    {
        public int              mID = -1;
        public string           mName = string.Empty;
        public string           mPrefabPath = string.Empty;
		public bool             mHideWithDestroy = true;

        public bool Load(XElement element)
        {
            mID                 = Convert.ToInt32(element.Attribute("id").Value);
            mName               = element.Attribute("Name").Value;
            mPrefabPath         = element.Attribute("Prefab").Value;
            string temp         = element.Attribute("HideWithDestroy").Value;
            if( temp.Equals("1"))
            {
                mHideWithDestroy = true;
            }
            else
            {
                mHideWithDestroy = false;
            }
            return true;
        }
    }

    public class UIWindowConfigProvider : Singleton<UIWindowConfigProvider>, IDataProvider
    {
        private List<UIWindowConfig> dataList = new List<UIWindowConfig>();

        public string Path()
        {
            return "/data/uiwindow.xml";
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
                    var xElement = xmlDoc.Element("windows");
                    if (xElement == null)
                        return;

                    var elements = xElement.Elements("item");
                    foreach (var em in elements)
                    {
                        UIWindowConfig item = new UIWindowConfig();
                        if (item.Load(em))
                        {
                            dataList.Add(item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LoggerSystem.Instance.Error("data/uiwindow.xml resource failed " + e.ToString());
            }
        }

        public bool Verify()
        {
//            foreach(var i in mDataList)
//	        {
//                LoggerSystem.Instance.Debug("UIWindow   " + i.mID + "  " + i.mName);
//	        }
	        return true;
        }

        public List<UIWindowConfig> GetAllData()
        {
            return dataList;
        }

		public UIWindowConfig GetData(string name)
		{
			UIWindowConfig ret = null;
			for (int i = 0; i < dataList.Count; ++i) {
				if (dataList[i].mName.Equals (name)) {
					ret = dataList[i];
					break;
				}
			}
			return ret;
		}
    }
}
