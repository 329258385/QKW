using System;
using Solarmax;

public class LocalAccountStorage : Singleton<LocalAccountStorage>, ILocalStorage
{
	public string account = string.Empty;
	public string singleCurrentLevel = string.Empty;
    public string guideFightLevel = string.Empty;
	public string Name()
	{
		return "LocalAccountStorage";
	}

	public void Save(LocalStorageSystem manager)
	{
		manager.PutString(account);
		manager.PutString (singleCurrentLevel);
        manager.PutString(guideFightLevel);
	}

	public void Load(LocalStorageSystem manager)
	{
		account = manager.GetString();
		singleCurrentLevel = manager.GetString ();
        guideFightLevel = manager.GetString();
	}
}

