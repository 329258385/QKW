/*
 * Name: LocalPlayerDataManager
 * Function: 提供游戏本地化存储游戏数据功能
 * Author: FangJun
 * Date: 2014-8-19
 * Framework: 
 *          version+data
 */

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Solarmax
{
    public sealed class LocalStorageSystem : Singleton<LocalStorageSystem>, Lifecycle
    {
		/// <summary>
		/// 存盘标识
		/// </summary>
		private bool needSaveDisk;
		/// <summary>
		/// app version
		/// </summary>
		private string appVersion;
		/// <summary>
		/// 本地存储数据的版本，此数据用于更新版本时本地存储数据格式可能发生变化所用
		/// </summary>
		private string storageVersion;
		/// <summary>
		/// 本地存储的所有对象
		/// </summary>
		public List<ILocalStorage> storageList;

		private int saveTempIndex;   // 存储临时数据
		private string saveTempName; // 存储临时数据

		private const string StorageVersionMark = "_LocalStorageVersion_";

		public LocalStorageSystem()
		{
			appVersion = string.Empty;
			storageList = new List<ILocalStorage> ();
			saveTempIndex = 0;
			saveTempName = string.Empty;
		}

		/// <summary>
		/// 初始化
		/// </summary>
		public bool Init()
		{
			LoggerSystem.Instance.Debug("LocalStorageSystem    init   begin");

			// 注册管理器
			RegisterLocalStorage (LocalAccountStorage.Get ());
			RegisterLocalStorage (LocalSettingStorage.Get ());
            RegisterLocalStorage (LocalLevelStorage.Get() );

			// 获取客户端版本
			appVersion          = UpdateSystem.Instance.GetAppVersion();

			// 加载存储信息
			LoadStorage();


			LoggerSystem.Instance.Debug("LocalStorageSystem    init   end");
			return true;
		}

		public void Tick(float interval)
		{
			if (!needSaveDisk)
				return;

			SaveStorage();

			needSaveDisk = false;
		}

		public void Destroy()
		{
			LoggerSystem.Instance.Debug("LocalPlayerDataManager    destroy   begin");

			SaveStorage();
			storageList.Clear();

			LoggerSystem.Instance.Debug("LocalPlayerDataManager    destroy   end");
		}

		/// <summary>
		/// 注册本地存储对象
		/// </summary>
		/// <param name="storage">Storage.</param>
		private void RegisterLocalStorage(ILocalStorage storage)
		{
			if (!storageList.Contains (storage))
				storageList.Add (storage);
		}

		/// <summary>
		/// 加载本地存储对象，并校验数据
		/// </summary>
		/// <returns><c>true</c>, if storage was loaded, <c>false</c> otherwise.</returns>
		private bool LoadStorage()
		{
			storageVersion = PlayerPrefs.GetString(StorageVersionMark);
			//if (!VarifyVersion())
			//{
			//	Debug.LogError("客户端版本与本地存储版本不匹配，将删除本地存储数据！");
			//	//DeleteStorage();
			//	return true;
			//}

			ILocalStorage storage;
			for (int i = 0; i < storageList.Count; ++i)
			{
				storage = storageList[i];
				saveTempName = storage.Name();
				saveTempIndex = 0;
				storage.Load(this);
			}

			return true;
		}

		/// <summary>
		/// 存储本地存储数据
		/// </summary>
		private void SaveStorage()
		{
			if (!needSaveDisk)
				return;

			// 数据版本信息
			PlayerPrefs.SetString(StorageVersionMark, appVersion);

			// 用户信息
			ILocalStorage storage;
			for (int i = 0; i < storageList.Count; ++i)
			{
				storage = storageList[i];
				saveTempName = storage.Name();
				saveTempIndex = 0;
				storage.Save(this);
			}

			PlayerPrefs.Save();

			needSaveDisk = false;
		}

		/// <summary>
		/// 删除所有本地存储
		/// </summary>
		private void DeleteStorage()
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
		}

		/// <summary>
		/// 存盘
		/// </summary>
		public void NeedSaveToDisk()
		{
			needSaveDisk = true;

			//SaveStorage ();
		}

		public void SetAppVersion(string version)
		{
			appVersion = version;
		}

		/// <summary>
		/// 校验数据版本和客户端版本是否一致
		/// </summary>
		/// <returns><c>true</c>, if version was varifyed, <c>false</c> otherwise.</returns>
		public bool VarifyVersion()
		{
			// 只有同版本的数据可以被校验通过，不同版本数据需要被清空
			// 版本长度不同肯定被清空。如果是4位版本号，则忽略最后一位版本的区别。
			Debug.LogFormat("游戏版本：[{0}]  本地数据版本：[{1}]", appVersion, storageVersion);

			bool ret = true;

			string[] storageArray = storageVersion.Split ('.');
			string[] appArray = appVersion.Split ('.');
			if (storageArray.Length != appArray.Length)
				ret = false;

			for (int i = 0; i < appArray.Length - 1; ++i) {
				if (!storageArray [i].Equals (appArray [i])) {
					ret = false;
					break;
				}
			}

			return ret;
		}


		public char GetChar()
		{
			return (char)PlayerPrefs.GetInt(saveTempName + ++saveTempIndex);
		}

		public short GetShort()
		{
			return (short)PlayerPrefs.GetInt(saveTempName + ++saveTempIndex);
		}

		public int GetInt()
		{
			return PlayerPrefs.GetInt(saveTempName + ++saveTempIndex);
		}

		public float GetFloat()
		{
			return PlayerPrefs.GetFloat(saveTempName + ++saveTempIndex);
		}

		public long GetLong()
		{
			// note: use for 8byte long size
			if (sizeof(long) == 2 * sizeof(int))
			{
				long temp1 = PlayerPrefs.GetInt(saveTempName + ++saveTempIndex);
				long temp2 = PlayerPrefs.GetInt(saveTempName + ++saveTempIndex);
				return ((temp1 << (8 * sizeof(int))) | temp2);
			}
			//		else
			//		{
			//			return PlayerPrefs.GetInt(saveTempName + ++saveTempIndex);
			//		}
		}

		public string GetString()
		{
			return PlayerPrefs.GetString(saveTempName + ++saveTempIndex);
		}

		public void PutChar(char data)
		{
			PlayerPrefs.SetInt(saveTempName + ++saveTempIndex, data);
		}

		public void PutShort(short data)
		{
			PlayerPrefs.SetInt(saveTempName + ++saveTempIndex, data);
		}

		public void PutInt(int data)
		{
			PlayerPrefs.SetInt(saveTempName + ++saveTempIndex, data);
		}

		public void PutFloat(float data)
		{
			PlayerPrefs.SetFloat(saveTempName + ++saveTempIndex, data);
		}

		public void PutLong(long data)
		{
			if (sizeof(long) == 2 * sizeof(int))
			{
				PlayerPrefs.SetInt(saveTempName + ++saveTempIndex, (int)(data >> (8 * sizeof(int))));
				PlayerPrefs.SetInt(saveTempName + ++saveTempIndex, (int)(data));
			}
			//		else
			//		{
			//			PlayerPrefs.SetInt(saveTempName + ++saveTempIndex, (int)(data));
			//		}
		}

		public void PutString(string data)
		{
			PlayerPrefs.SetString(saveTempName + ++saveTempIndex, data);
		}


    }
}
