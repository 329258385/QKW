using System;
using Solarmax;

public class LocalLevelStorage : Singleton<LocalLevelStorage>, ILocalStorage
{

	public int Levelfails = 0; // 关卡失败次数
    public int LevelID = 0;
	public string Name()
	{
        return "LocalLevelfails";
	}

	public void Save(LocalStorageSystem manager)
	{
        manager.PutInt(Levelfails);
        manager.PutInt(LevelID);
	}

	public void Load(LocalStorageSystem manager)
	{
        Levelfails = manager.GetInt();
        LevelID    = manager.GetInt();
	}

    public void SetLevelInfo( string strLevel )
    {
        string sub = strLevel.Substring(1);
        LevelID = int.Parse(sub);
    }
}

