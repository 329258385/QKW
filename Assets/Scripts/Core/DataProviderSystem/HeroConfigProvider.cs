using System;
using GameCore.Loader;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;





///////////////////////////////////////////////////////////////////////////////
/// Œ‰∆˜≈‰÷√±Ì

namespace Solarmax
{
    public class HeroConfig : ICfgEntry
	{
		public System.Int32  id     = 0;
        public System.Int32  maxHp  = 0;
        public System.Int32  damage = 0;
        public System.Int32  Arms   = 0;  // ±¯÷÷
        public System.Int32  speed  = 0;
        public System.Int32  attackspeed = 1;
        public System.Int32  attackrange = 1;
        public System.Int32  WarningRange = 1;

        /// <summary>
        /// –Ø¥¯±¯÷÷
        /// </summary>
        public System.Int32  ArmsType = 0;
        public System.String name   = string.Empty;
		public System.String icon   = string.Empty;
        public System.String perfab = string.Empty;
		public System.String desc   = string.Empty;

        public bool Load(XElement element)
        {
            id          = Convert.ToInt32(element.Attribute("id").Value);
            ArmsType    = Convert.ToInt32(element.Attribute("ArmsType").Value);
            maxHp       = Convert.ToInt32(element.Attribute("MaxHP").Value);
            damage      = Convert.ToInt32(element.Attribute("damage").Value);
            Arms        = Convert.ToInt32(element.Attribute("Arms").Value);
            speed       = Convert.ToInt32(element.Attribute("speed").Value);
            attackspeed = Convert.ToInt32(element.Attribute("attackspeed").Value);
            attackrange = Convert.ToInt32(element.Attribute("attackrange").Value);
            WarningRange = Convert.ToInt32(element.Attribute("WarningRange").Value);

            name        = element.Attribute("name").Value;
            icon        = element.Attribute("icon").Value;
            perfab      = element.Attribute("perfab").Value;
            desc        = element.Attribute("desc").Value;
            return true;
        }
	}


    public class HeroConfigProvider : Singleton<HeroConfigProvider>, IDataProvider
	{
        private List<HeroConfig> dataList = new List<HeroConfig>();
		public string Path()
		{
			return "/data/Heros.xml";
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
                    var xElement = xmlDoc.Element("Heros");
                    if (xElement == null)
                        return;


                    var elements = xElement.Elements("Hero");
                    foreach (var em in elements)
                    {
                        HeroConfig item = new HeroConfig();
                        if (item.Load(em))
                        {
                            dataList.Add(item);

                        }
                    }
                }
            }
            catch (Exception e)
            {
                LoggerSystem.Instance.Error("data/Weapon.xml resource failed " + e.ToString());
            }
		}
		public bool Verify()
		{
			return true;
		}

        public List<HeroConfig> GetAllData()
		{
			return dataList;
		}

        public HeroConfig GetData(System.Int32 id)
		{
            HeroConfig ret = null;
			for (int i = 0; i < dataList.Count; ++i)
			{
				if (dataList [i].id.Equals (id)) {
					ret = dataList [i];
					break;
				}
			}
			return ret;
		}
	}
}
