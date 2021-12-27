/*
 * Name: UpdateSystem
 * Function: 提供游戏数据更新功能
 * Author: LJP
 * Date: 2016-06-23
 * Framework: 
 *          主要提供游戏中数据更新，分两步：
 * 				1，整包更新优先，如果有整包更新，则直接弹出，并不更新之间的小包
 * 				2，StreamingAsstes中的配置文件，诸如txt,lua，这种数据流文件，自己解析的这些
 * 				3，Prefab文件，需要用AB更新。
 * 
 * 保存的文件：
 *			主要有filelist.txt、AssetBundles、assets/xxx.ab
 */

using GameCore.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;





namespace Solarmax
{
    [Serializable]
	public class DownTable
	{
		/// <summary>
		/// 资源表的路径
		/// </summary>
		public string   Url;

		/// <summary>
		/// 保存路径描述
		/// </summary>
		public string   path;
	}


    [Serializable]
	public class TableAsset
	{
		/// <summary>
		/// 资源路径名
		/// </summary>
		public string assetPath;
        /// <summary>
        /// md5码
        /// </summary>
        public string GUID;

        /// <summary>
        /// 文件
        /// </summary>
        public long Size;


        /// <summary>
        /// ab包路径
        /// </summary>
        public string assetBundlePath;
	}

    
	public class UpdateSystem : MonoSingleton<UpdateSystem>
	{
		private const string    AppVersionMark             = "_AppVersion_";
		private string          appVersion;
        static private bool     IsLoginUpdate = false;
        static private bool     IsNeedUpdateManifest = false;
        static public bool      IsPauseDownLoad = false;
        static public int       downLoadIndex = 0;

        public string                                   serverUrl;
        public string                                   serverCDN;
		public string                                   saveRoot;
        public string                                   saveVideo;
        public string                                   saveSkin;
		
        /// <summary>
        /// unity 主线程与协成共享资源是否需要 lock ,确认中 
        /// </summary>
        private long                                    nDownAllSize = 0;
		private List<DownTable>                         downList = new List<DownTable>();
        private Dictionary<string,TableAsset>           localFiles = new Dictionary<string, TableAsset>();
        private Dictionary<string, TableAsset>          serverFiles = new Dictionary<string, TableAsset>();


        public bool Init()
		{
			serverUrl = string.Empty;
			if (!ConfigSystem.Instance.TryGetConfig("UpdateServer", out serverUrl))
			{
				serverUrl = "https://ks3-cn-shanghai.ksyun.com/solarmax-cn/solarmax/";
			}

           
            //serverCDN = string.Empty;
            //ConfigSystem.Instance.TryGetConfig(cdnKey, out serverCDN);
            //if (string.IsNullOrEmpty(serverCDN))
            //{
            //    Debug.LogError("Server cdn is empty!");
            //    return false;
            //}

            if (Application.platform == RuntimePlatform.WindowsEditor) {
				saveRoot            = Application.dataPath + "/cache";
                saveVideo           = Application.dataPath + "/video2/";
                saveSkin            = Application.dataPath + "/skin/";
			} else if (Application.platform == RuntimePlatform.OSXEditor) {
				saveRoot            = Application.persistentDataPath;
                saveVideo           = Application.persistentDataPath + "/video2/";
                saveSkin            = Application.persistentDataPath + "/skin/";
			} else if (Application.platform == RuntimePlatform.Android) {
				saveRoot            = Application.persistentDataPath;
                saveVideo           = Application.persistentDataPath + "/video2/";
                saveSkin            = Application.persistentDataPath + "/skin/";
			} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
				saveRoot            = Application.persistentDataPath;
                saveVideo           = Application.persistentDataPath + "/video2/";
                saveSkin            = Application.persistentDataPath + "/skin/";
			}
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                saveRoot            = Application.dataPath + "/cache";
                saveVideo           = Application.dataPath + "/video2/";
                saveSkin            = Application.dataPath + "/skin/";
            }

			if (!saveRoot.EndsWith("/"))
				saveRoot += "/";

			return true;
		}

		public void Tick(float interval)
		{
		
		}

		public void Destroy()
		{
			
		}


        
		public void RequestResUpdate()
		{
            int nRet = GetNetworkRechability();
            if (nRet == 0)
            {
                /// 当前没有网络,请开启wifi网络
                if(!UISystem.Get().IsWindowVisible("CommonDialogWindow") )
                    UISystem.Get().ShowWindow("CommonDialogWindow");
                EventSystem.Instance.FireEvent(EventId.OnCommonDialog, 5, "",
                                                new EventDelegate(RequestResUpdate), new EventDelegate(QuitGame) );
                return;
            }

            if (UISystem.Get().IsWindowVisible("CommonDialogWindow"))
                UISystem.Get().HideWindow("CommonDialogWindow");

            TryGetDownloadFilesFromServer();
            if (nRet == 1)
            {
                /// 你当前正在4G网络下载更新资源将会消耗流量
                /// 你可切换至wifi免费下载资源
                if (IsLoginUpdate)
                {
                    UISystem.Get().ShowWindow("CommonWifiWindow");
                    EventSystem.Instance.FireEvent(EventId.OnCommonDialog, nDownAllSize, "",
                                                    new EventDelegate(QuitGame), new EventDelegate(TryDownloadFromServer));

                }
                else
                {
                    EventSystem.Instance.FireEvent(EventId.OnABDownloadingFinished);
                }
            }

            else
            {
                TryDownloadFromServer();
            }
        }

        public void QuitGame()
        {
            #if UNITY_ANDROID
            Application.Quit();
            #endif
        }


        /// <summary>
        /// 统计本次下载资源, 
        /// </summary>
        public void TryGetDownloadFilesFromServer()
        {
            // 上次版本，更新后的
            string lastVersion = PlayerPrefs.GetString(AppVersionMark);
            if (string.IsNullOrEmpty(lastVersion))
            {
                lastVersion = Application.version;
            }


            appVersion = lastVersion;
            string servVersion = String.Empty;
            string url = serverCDN + "Version.txt";
            using (WWW www = new WWW(url))
            {
                while (!www.isDone) ;
                if (www.error != null)
                {
                    appVersion = lastVersion;
                    EventSystem.Instance.FireEvent(EventId.OnABDownloadingFinished);
                    return;
                }
                servVersion = www.text;
            }

            // 如果 servVersion 新于 lastVersion
            if (CheckVersion(servVersion, lastVersion))
            {
                appVersion = servVersion;
                LoadFileListLocal();
                LoadFileListServer();
                GeneriUpdateFiles();
            }
        }


        public void TryDownloadFromServer()
        {
            if (IsLoginUpdate)
            {
                StartDownLoad();
            }
            else
            {
                EventSystem.Instance.FireEvent(EventId.OnABDownloadingFinished);
            }  
        }



        public void OnVritfyDownLoad()
        {
            StartDownLoad();
        }

        /// <summary>
        /// 检查是否需要升级, true需要升级
        /// </summary>
        /// <param name="ver1"></param>
        /// <param name="ver2"></param>
        /// <returns></returns>
        private bool CheckVersion(string ver1, string ver2)
		{
            var arr1 = ver1.Split('.');
            var arr2 = ver2.Split('.');
            for(int i = 0; i < arr1.Length && i < arr2.Length; ++i)
            {
                int n1;
                if(!int.TryParse(arr1[i], out n1))
                {
                    Debug.LogErrorFormat("Parse version error! ver1 = {0}, ver2 = {1}", ver1, ver2);
                    return false;
                }
                int n2;
                if (!int.TryParse(arr2[i], out n2))
                {
                    Debug.LogErrorFormat("Parse version error! ver1 = {0}, ver2 = {1}", ver1, ver2);
                    return false;
                }
                if(n1 == n2)
                {
                    continue;
                }
                return n1 > n2;
            }
            if(arr1.Length > arr2.Length)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 计算版本号(TODO)
        /// </summary>
        public int CalCode(string version)
        {
            int codec = 0;
            var arr = version.Split('.');
            for (int i = 0; i < arr.Length; ++i)
            {
                int n;
                if (!int.TryParse(arr[i], out n))
                {
                    Debug.LogErrorFormat("Parse version error! ver = {0}", version);

                    // TODO
                    return 1000;
                }

                int m = 1;
                for(int j = i; j < arr.Length - 1; ++j)
                {
                    m *= 1000;
                }
                codec += n * m;
            }
            return codec;
        }

        private void SaveAsset2Local(string path, WWW www )
		{
			if (File.Exists (path))
            {
                File.Delete(path);
            }

            int pos2        = path.LastIndexOf('/');
            string filePath = path.Substring(0, pos2);
            if( !Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            System.IO.FileStream fs = new System.IO.FileStream (path, System.IO.FileMode.Create);
			fs.Write (www.bytes, 0, www.bytes.Length);
			fs.Flush ();
			fs.Close ();
			fs.Dispose ();
		}


        /// <summary>
        /// 单独拿出来了，file 不打成 ab 文件了
        /// </summary>
        private void LoadFileListLocal()
        {
            string filePath = LoadResManager.rootPathPersistent + "filelist.xml";
            FileInfo file   = new FileInfo(filePath);
            if (!file.Exists)
            {
                filePath    = LoadResManager.rootPathStreamAssets + "filelist.xml";
            }
            else
            {
                #if UNITY_EDITOR 
                filePath    = "file:///" + filePath;
                #endif
            }

            localFiles.Clear();
            WWW www = new WWW(filePath);
            while (!www.isDone) ;
            if (!string.IsNullOrEmpty(www.text))
            {
                XDocument xmlDoc = XDocument.Parse(www.text);
                var xElement     = xmlDoc.Element("Assets");
                if (xElement == null)
                    return;

                var elements = xElement.Elements("Asset");
                foreach (var element in elements)
                {
                   
                    TableAsset item         = new TableAsset();
                    item.assetPath          = element.Attribute("id").Value;
                    item.GUID               = element.Attribute("md5").Value;
                    item.Size               = long.Parse(element.Attribute("Size").Value);
                    item.assetBundlePath    = element.Attribute("bundlePath").Value;
                    localFiles.Add(item.assetPath, item);
                }
            }
            www.Dispose();
        }


        private void LoadFileListServer()
        {
            serverFiles.Clear();
            string filePath = serverCDN + "filelist.xml";
            WWW www         = new WWW(filePath);
            while (!www.isDone) ;
            if (!string.IsNullOrEmpty(www.text))
            {
                XDocument xmlDoc = XDocument.Parse(www.text);
                var xElement = xmlDoc.Element("Assets");
                if (xElement == null)
                    return;

                var elements = xElement.Elements("Asset");
                foreach (var element in elements)
                {
                    TableAsset item         = new TableAsset();
                    item.assetPath          = element.Attribute("id" ).Value;
                    item.GUID               = element.Attribute("md5").Value;
                    item.Size               = long.Parse(element.Attribute("Size").Value);
                    item.assetBundlePath    = element.Attribute("bundlePath").Value;
                    serverFiles.Add(item.assetPath, item);
                }
            }
            www.Dispose();
        }

        private void SaveFileList2Xml()
        {
            // 文件列表文件
            string filePath = LoadResManager.rootPathPersistent + "filelist.xml";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
            XmlTextWriter writer    = new XmlTextWriter(fs, new System.Text.UTF8Encoding(false));
            writer.Formatting       = Formatting.Indented;

            writer.WriteStartDocument();
            writer.WriteStartElement("Assets");
            foreach (var v in serverFiles)
            {
                writer.WriteStartElement("Asset");
                writer.WriteAttributeString("id",           v.Value.assetPath);
                writer.WriteAttributeString("md5",          v.Value.GUID);
                writer.WriteAttributeString("Size",         v.Value.Size.ToString());
                writer.WriteAttributeString("bundlePath",   v.Value.assetBundlePath);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            fs.Close();
        }


        private void GeneriUpdateFiles()
        {
            foreach (var v in serverFiles)
            {
                string key       = v.Key;
                TableAsset asset = null;
                localFiles.TryGetValue(key, out asset);
                if( asset != null && !asset.GUID.Equals( v.Value.GUID ) )
                {
                    IsLoginUpdate = true;
                    string url  = serverCDN + asset.assetBundlePath;
                    string path = LoadResManager.rootPathPersistent + asset.assetBundlePath;
                    AddDownLoad( url, path, v.Value.Size);
                }

                /// 远程有，本地没有
                if( asset == null )
                {
                    IsLoginUpdate           = true; 
                    IsNeedUpdateManifest    = true;
                    string url              = serverCDN + v.Value.assetBundlePath;
                    string path             = LoadResManager.rootPathPersistent + v.Value.assetBundlePath;
                    AddDownLoad(url, path, v.Value.Size );
                }
            }

            if( IsNeedUpdateManifest  )
            {
                string url  = serverCDN + "StreamingAssets";
                string path = LoadResManager.rootPathPersistent     + "StreamingAssets";
                AddDownLoad(url, path);
            }
        }

        /// <summary>
        /// 重新加载client配置
        /// </summary>
        public void ReloadClient()
        {
            IsLoginUpdate = false;
            localFiles.Clear();
            serverFiles.Clear();

            DataProviderSystem.Instance.Destroy();
            DataProviderSystem.Instance.Init();
        }

        /// <summary>
        /// 获取客户端版本
        /// </summary>
        /// <returns>The app version.</returns>
        public string GetAppVersion()
		{
			return appVersion;
		}
		
		/// <summary>
		/// 下载版本中涉及到的资源表格
		/// </summary>
		private IEnumerator DownloadAssets(string url, string path )
		{
            yield return new WaitForSeconds(1.0f);

            using (WWW www = new WWW(url))
            {
                while (!www.isDone)
                {
                    yield return 1;
                }

                if( www.error != null )
                {
                    /// 异常处理 
                    Debug.LogError(url);
                    IsPauseDownLoad = true;
                }

                if (www.error == null)
                {
                    /// 保存在本地, 暂时只支持png格式的资源,将来可能在 framework 做
                    downLoadIndex++;
                    byte[] bytes = www.bytes;
                    SaveAsset2Local(path, www);
                    //EventSystem.Instance.FireEvent(EventId.OnUpdateDownLoad, (long)bytes.Length );
                    //EventSystem.Instance.FireEvent(EventId.OnUpdateDownLoad2, 1);
                }

                www.Dispose();
                
            }   
		}

        public void AddDownLoad( string url, string savePath, long nSize = 0)
        {
            // 判断是否存在
            if(IsExit(url))
            {
                return;
            }

            DownTable table = new DownTable();
            table.Url       = url;
            table.path      = savePath;
            nDownAllSize    += nSize;
            downList.Add(table);
        }


        public bool IsDownLoadIng()
        {
            if (downList.Count > 0)
                return true;

            return false;
        }


        private bool IsExit( string url )
        {
            for (int i = 0; i < downList.Count; ++i)
            {
                if (downList[i].Equals(url))
                    return true;
            }
            return false;
        }

        public void StartDownLoad( )
        {
            long nMaxProgress = nDownAllSize;
            long nCurProgress = 0;
            downLoadIndex     = 0;
            //EventSystem.Instance.FireEvent(EventId.OnStartDownLoad, nCurProgress, nMaxProgress);
            //EventSystem.Instance.FireEvent(EventId.OnStartDownLoad2, nCurProgress, downList.Count );
            StartCoroutine(StartUpdate());
        }

		/// <summary>
		/// 开始更新
		/// 小版本资源更新
		/// </summary>
		public IEnumerator StartUpdate()
		{
			yield return new WaitForSeconds (1.0f);

			for (; downLoadIndex < downList.Count; ) {
				var v           = downList[downLoadIndex];

                if( IsPauseDownLoad )
                {
                    int nCode   = GetNetworkRechability();
                    if (nCode > 0)
                        IsPauseDownLoad = false;
                    yield return new WaitForSeconds(2.0f);
                }

                //下载assetTable就好了
                yield return DownloadAssets (v.Url, v.path );
			}

            downList.Clear();
            if( IsLoginUpdate )
            {
                nDownAllSize = 0;
                SaveFileList2Xml();
                PlayerPrefs.SetString(AppVersionMark, appVersion );
                ReloadClient();
                
            }

            if( IsNeedUpdateManifest )
            {
                LoadResManager.ReLoadManifest();
                IsNeedUpdateManifest = false;
            }
			EventSystem.Instance.FireEvent (EventId.OnABDownloadingFinished);
		}

        public int GetNetworkRechability()
        {
            NetworkReachability reach = Application.internetReachability;
            int ret = 0;
            if (reach == NetworkReachability.NotReachable)
                ret = 0;
            else if (reach == NetworkReachability.ReachableViaCarrierDataNetwork)
                ret = 1;
            else if (reach == NetworkReachability.ReachableViaLocalAreaNetwork)
                ret = 2;

            return ret;
        }
    }
}

