using System;
using Solarmax;

public class LocalSettingStorage : Singleton<LocalSettingStorage>, ILocalStorage
{
	public bool music = true;
	public bool sound = true;
	public int effectLevel = 0;//0高，1中，2低
	public int fightOption = 0; // 0侧边条，1长按
	public string ip = "";

	public string Name()
	{
		return "LocalSettingStorage";
	}

	public void Save(LocalStorageSystem manager)
	{
		manager.PutInt (music ? 1 : 0);
		manager.PutInt (sound ? 1 : 0);
		manager.PutInt (effectLevel);
		manager.PutInt (fightOption);
		manager.PutString (ip);
	}

	public void Load(LocalStorageSystem manager)
	{
		music = manager.GetInt () > 0;
		sound = manager.GetInt () > 0;
		effectLevel = manager.GetInt ();
		fightOption = manager.GetInt ();
		ip = manager.GetString ();
	}
}

