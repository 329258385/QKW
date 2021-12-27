using System;
using System.Collections.Generic;
using System.Text;

namespace Solarmax
{
    public class DataProviderSystem : Singleton<DataProviderSystem>, Lifecycle
    {
        private List<IDataProvider> mDataProvider = new List<IDataProvider>();

        public bool Init()
        {
			LoggerSystem.Instance.Debug("DataProviderSystem   init   begin");

            // 注册
			RegisterDataProvider (ChestConfigProvider.Instance);
			RegisterDataProvider (DictionaryDataProvider.Instance);
			RegisterDataProvider (HeroConfigProvider.Instance);
			RegisterDataProvider (LadderConfigProvider.Instance);
			RegisterDataProvider (NameConfigProvider.Instance);
			RegisterDataProvider (ArticleConfigProvider.Instance);
			RegisterDataProvider (UIWindowConfigProvider.Instance);
			RegisterDataProvider (MapNodeConfigProvider.Instance);
			RegisterDataProvider (MapBuildingConfigProvider.Instance);
			RegisterDataProvider (MapPlayerConfigProvider.Instance);
			RegisterDataProvider (MapConfigProvider.Instance);
			RegisterDataProvider (GameVariableConfigProvider.Instance);
			RegisterDataProvider (SkillConfigProvider.Instance);
			RegisterDataProvider (SkillBufferConfigProvider.Instance);
			RegisterDataProvider (GuideDataProvider.Instance);
			RegisterDataProvider (LevelConfigConfigProvider.Instance);
			RegisterDataProvider (ChapterConfigProvider.Instance);
            RegisterDataProvider (BuildUpgradeConfigProvider.Instance);

            // 加载
            if (!Load()) return false;

			LoggerSystem.Instance.Debug("DataProviderSystem   init   end");
            return true;
        }

		public void Tick(float interval)
        {

        }

        public void Destroy()
        {
            mDataProvider.Clear();
        }

        private bool Load()
        {
            IDataProvider provider = null;
            for (int i = 0; i < mDataProvider.Count; ++i)
            {
                provider = mDataProvider[i];
                if (null != provider)
                {
                    if( !provider.IsXML() )
					    FileReader.LoadPath(AssetManager.Get().LoadStramingAsset(provider.Path()));

                    provider.Load();
                    if( !provider.IsXML() )
                        FileReader.UnLoad();
                }
            }

            return true;
        }

        private void RegisterDataProvider(IDataProvider dataProvider)
        {
            mDataProvider.Add(dataProvider);
        }

		public static string FormatDataProviderPath(string datapath)
        {
            return System.IO.Path.Combine(Framework.Instance.GetStreamAssetsRootDir(), datapath);
		}
		public void LoadExtraData (string path)
		{
			IDataProvider provider = null;
			for (int i = 0; i < mDataProvider.Count; ++i)
			{
				provider = mDataProvider[i];
				if (null != provider)
				{
					FileReader.LoadPath(System.IO.Path.Combine(path, provider.Path()));
					FileReader.UnLoad();
				}
			}
		}
    }
}
