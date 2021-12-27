using System;
using GameCore.Loader;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;





namespace Solarmax
{
    
    public enum TARGETTYPE
    {
        Ship,
        Node,
    }


    public class CTagBufferConfig : ICfgEntry
    {
		public int              buffId;
		public string           name;
        public string           icon;
        public string           desc;
		
	
		public TARGETTYPE       targetType;
        public int              logicID;
        public int              order;

        public float            lastTime;       
		public float            coodown;        // 特效CD
        public int              interval;       // 触发效果间隔时间转帧数

		public string           effectId;
		public string           arg0;
		public string           arg1;

        public bool Load( XElement element)
        {
            buffId          = Convert.ToInt32(element.Attribute("id").Value);

            logicID         = Convert.ToInt32(element.Attribute("logicId").Value);
            order           = Convert.ToInt32(element.Attribute("order").Value);
            targetType      = (TARGETTYPE)Convert.ToInt32(element.Attribute("targetType").Value);

            lastTime        = Convert.ToSingle(element.Attribute("lastTime").Value);
            coodown         = Convert.ToSingle(element.Attribute("coodown").Value);
            interval        = Convert.ToInt32(element.Attribute("actInterval").Value);

            effectId        = element.Attribute("effectId").Value;
            arg0            = element.Attribute("arg0").Value;
            arg1            = element.Attribute("arg1").Value;

            name            = element.Attribute("name").Value;
            icon            = element.Attribute("icon").Value;
            desc            = element.Attribute("desc").Value;

            return true;
        }
	}

	public class SkillBufferConfigProvider : Singleton<SkillBufferConfigProvider>, IDataProvider
	{

		private List<CTagBufferConfig> dataList = new List<CTagBufferConfig>();
		public SkillBufferConfigProvider()
		{

		}

		public string Path()
		{
			return "/data/SkillBufferConfig.xml";
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
                        CTagBufferConfig item = new CTagBufferConfig();
                        if (item.Load(em))
                        {
                            dataList.Add(item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LoggerSystem.Instance.Error("data/SkillBufferConfig.xml resource failed " + e.ToString());
            }
		}

		public bool Verify()
		{
			return true;
		}


		public CTagBufferConfig GetData(int buffid)
		{
            CTagBufferConfig ret = null;
			for (int i = 0; i < dataList.Count; ++i)
			{
				if (dataList [i].buffId == buffid)
				{
					ret = dataList [i];
					break;
				}
			}
			return ret;
		}
	}
}
