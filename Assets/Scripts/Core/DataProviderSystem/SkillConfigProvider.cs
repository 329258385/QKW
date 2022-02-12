using System;
using GameCore.Loader;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;





/// <summary>
/// 技能属性相关的定义
/// </summary>
namespace Solarmax
{		

    /// <summary>
    /// buffer 选择目标类型
    /// </summary>
    public enum TargetType
    {
       Null,
       Self,        // 自己战舰
       SelfTeam,    // 自己战队

       Eneny,       // 敌方战舰
       EnenyeTeam,  // 敌方战队
    }


    public enum CastType
    {
        Null,
        Item,               // 消耗道具
        Engine,             // 消耗能量
    }

    public class SkillConfig : ICfgEntry
    {
		public int              id;
        public int              type;                       // 0 道具攻击， 1 buffer 攻击
        public TargetType       target;
        public CastType         cast;
        public int              scope;                      // 动作码
        public float            cd;
        public float            effectLife;

        public bool             bMultTarget     = false;
        public int              castId;                      // 消耗物品ID

        public string           name;
		public string           desc;
		public string           icon;
        public string           disable;
        public string           tips;
		public int              bufferID;                   // 技能携带bufferID
		public string           displayID;                  // 技能表现效果


        public bool Load( XElement element)
        {
            id              = Convert.ToInt32(element.Attribute("id").Value);
            type            = Convert.ToInt32(element.Attribute("type").Value);
            name            = element.Attribute("name").Value;
            icon            = element.Attribute("icon").Value;
            disable         = element.Attribute("disable").Value;
            desc            = element.Attribute("desc").Value;

            tips            = element.Attribute("tips").Value;
            bufferID        = Convert.ToInt32(element.Attribute("buffs").Value);
            target          = (TargetType)Convert.ToInt32(element.Attribute("target").Value);

            bMultTarget     = (bool)Convert.ToBoolean(element.Attribute("multtarget").Value);
            cast            = (CastType)Convert.ToInt32(element.Attribute("castType").Value);
            effectLife      = Convert.ToInt32(element.Attribute("effectLife").Value);
            cd              = Convert.ToSingle(element.Attribute("cd").Value);
            displayID       = element.Attribute("displayID").Value;
            castId          = Convert.ToInt32(element.Attribute("castId").Value);
            scope           = Convert.ToInt32(element.Attribute("scope").Value);
            return true;
        }
	}
	
	public class SkillConfigProvider : Singleton<SkillConfigProvider>, IDataProvider
	{

		private List<SkillConfig>            dataList = new List<SkillConfig>();
		public SkillConfigProvider()
		{

		}

		public string Path()
		{
			return "/data/SkillConfig.xml";
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
                        SkillConfig item = new SkillConfig();
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


		public SkillConfig GetData(int skillId)
		{
            SkillConfig ret = null;
			for (int i = 0; i < dataList.Count; ++i)
			{
				if (dataList [i].id == skillId)
				{
					ret = dataList [i];
					break;
				}
			}
			return ret;
		}
	}
}
