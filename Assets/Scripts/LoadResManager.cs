using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

/// <summary>
/// 资源加载管理器对象， 所有的资源将不能存在resources目录下，只有扩展目录和 StreamAssets 目录
/// </summary>
public class LoadResManager : Singleton<LoadResManager>
{

    /// <summary>
    /// 编辑器模式下资源加载目录
    /// </summary>
    static private string rootPathEditor        = "";

    /// <summary>
    /// 手机上资源可更新目录
    /// </summary>
    static public string rootPathPersistent    = "";

    /// <summary>
    /// 手机原始资源目录
    /// </summary>
    static public string rootPathStreamAssets = "";

    /// <summary>
    /// 特定文件增加后缀
    /// </summary>
    static public string FileExt   = ".ab";
    static public string TempRoot  = "assets/res/";


    static private AssetBundle          m_manifestAB = null;
    static private AssetBundleManifest  m_BundleManifest = null;
    static private Dictionary<string, ResourceObject> m_ResourceInfos;


    private bool bInitLoadResManager = false;

    public bool Init()
    {
        if (!bInitLoadResManager)
        {
            rootPathEditor = Application.dataPath;
#if UNITY_EDITOR
            rootPathStreamAssets = Application.streamingAssetsPath + "/";
            rootPathPersistent = Application.persistentDataPath + "/";
#else
#if UNITY_ANDROID
            rootPathStreamAssets    = Application.streamingAssetsPath + "/";
            rootPathPersistent      = Application.persistentDataPath + "/";
#endif
#if UNITY_IPHONE
            rootPathStreamAssets = Application.streamingAssetsPath + "/";
            rootPathPersistent   = Application.persistentDataPath + "/";
#endif
#endif
            m_ResourceInfos = new Dictionary<string, ResourceObject>();
            bInitLoadResManager = true;
        }
        return true;
    }


    static public void ReLoadManifest( )
    {
        if(m_manifestAB != null )
        {
            m_manifestAB.Unload(true);
            m_manifestAB = null;
            m_BundleManifest = null;
        }

        string filePath     = Application.persistentDataPath + "/StreamingAssets";
        FileInfo file       = new FileInfo(filePath);
        if (!file.Exists)
        {
            filePath        = Application.streamingAssetsPath + "/StreamingAssets";
        }
        m_manifestAB        = AssetBundle.LoadFromFile(filePath);
        m_BundleManifest    = m_manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

    }

    static private void LoadDependencyBymainfest(string path, out string[] dependencyAsset )
    {
        dependencyAsset = null;
        if ( !string.IsNullOrEmpty(path) )
        {

            if (m_BundleManifest == null)
            {
                string filePath        = Application.persistentDataPath + "/StreamingAssets";
                FileInfo file          = new FileInfo(filePath);
                if (!file.Exists)
                {
                    filePath           = Application.streamingAssetsPath + "/StreamingAssets";
                }
                m_manifestAB           = AssetBundle.LoadFromFile(filePath);
                m_BundleManifest       = m_manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }

            string bundleName          = string.Format("{0}{1}", path, FileExt);
            string[] dependencys       = m_BundleManifest.GetAllDependencies(bundleName);
            if (dependencys.Length <= 0)
                return;

            int nDependencyNum = dependencys.Length;
            string[] temp      = new string[nDependencyNum];
            dependencyAsset    = temp;
            for ( int i = 0; i < dependencys.Length; i++ )
            {
                dependencyAsset[i] = dependencys[i];
            }
        }
    }

    /// <summary>
    /// 加载依赖
    /// </summary>
    static private void LoadDependency( string path, string name, bool bFrist = false )
    {
        if (!string.IsNullOrEmpty(path))
        {
            string[] dependencyAsset = null;
            LoadDependencyBymainfest(path, out dependencyAsset );
            if (bFrist && dependencyAsset == null)
                return;

            if( dependencyAsset == null || dependencyAsset.Length <= 0 )
            {
                ResourceObject ResObj = null;
                m_ResourceInfos.TryGetValue(path, out ResObj);
                if (ResObj != null && ResObj.m_Asset != null)
                {
                    return;
                }

                ResObj      = new ResourceObject();
                m_ResourceInfos.Add(path, ResObj);
                ResObj.LoadRes(path, name, FileExt);
            }
            else
            {
                for( int i = 0; i < dependencyAsset.Length; i++ )
                {
                    string TempName = "";
                    string TempPath = dependencyAsset[i];
                    int pos1        = TempPath.LastIndexOf('.');
                    int pos2        = TempPath.LastIndexOf('/') + 1;
                    if (pos1 > 0)
                    {
                        TempName    = TempPath.Substring(pos2, pos1 - pos2);
                        TempPath    = TempPath.Substring(0, pos1);
                    }

                    LoadDependency(TempPath, TempName);
                }

                if(!bFrist)
                {
                    ResourceObject ResObj = null;
                    m_ResourceInfos.TryGetValue(path, out ResObj);
                    if (ResObj != null && ResObj.m_Asset != null)
                    {
                        return;
                    }

                    ResObj      = new ResourceObject();
                    m_ResourceInfos.Add(path, ResObj);
                    ResObj.LoadRes(path, name, FileExt);
                }
            }
        }
    }


    static public GameObject LoadRes( string path )
    {
        path                = path.ToLower();
        string name         = string.Empty;
        #if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
        path                = "assets/res/" + path;
        int pos1            = path.LastIndexOf('.');
        int pos2            = path.LastIndexOf('/') + 1;
        if (pos1 > 0)
        {
            name            = path.Substring(pos2, pos1 - pos2 );
        }
        path                = path.Substring(0, pos1);

        ResourceObject ResObj = null;
        m_ResourceInfos.TryGetValue(path, out ResObj);
        if (ResObj != null && ResObj.m_Asset != null)
        {
            GameObject ret = ResObj.m_Asset.LoadAsset<GameObject>(name);
            return ret;
        }

        // 优先加载依赖
        LoadDependency(path, name, true );
        
        // 加载自己
        ResObj      = new ResourceObject();
        m_ResourceInfos.Add(path, ResObj);
        return ResObj.LoadRes(path, name, FileExt);


        #else
        string filePath = "assets/res/" + path;
        return (GameObject)AssetDatabase.LoadAssetAtPath(filePath, typeof(GameObject) );
        #endif
    }


    static public void LoadScene( string path )
    {

        string name         = string.Empty;
        #if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
        path                = "assets/res/" + path;
        int pos1            = path.LastIndexOf('.');
        int pos2            = path.LastIndexOf('/') + 1;
        if (pos1 > 0)
        {
            name            = path.Substring(pos2, pos1 - pos2 );
        }
        path                = path.Substring(0, pos1);

        ResourceObject ResObj = null;
        m_ResourceInfos.TryGetValue(path, out ResObj);
        if (ResObj != null && ResObj.m_Asset != null)
        {
            return ;
        }

        // 优先加载依赖
        LoadDependency(path, name, true );
        
        // 加载自己
        ResObj      = new ResourceObject();
        m_ResourceInfos.Add(path, ResObj);
        ResObj.LoadScene(path, name, FileExt);
        #endif
    }

    static public  Texture2D LoadTex(string path )
    {
        path        = path.ToLower();
        string name = string.Empty;
        #if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
        path                = "assets/res/" + path;
        int pos1            = path.LastIndexOf('.');
        int pos2            = path.LastIndexOf('/') + 1;
        if (pos1 > 0)
        {
            name            =  path.Substring(pos2, pos1 - pos2 );
        }
        path                = path.Substring(0, pos1);

        ResourceObject ResObj = null;
        m_ResourceInfos.TryGetValue(path, out ResObj);
        if( ResObj != null && ResObj.m_Asset != null )
        {
            Texture2D ret = ResObj.m_Asset.LoadAsset<Texture2D>(name);
            return ret;
        }

        // 优先加载依赖
        LoadDependency(path, name, true);

        // 加载自己
        ResObj      = new ResourceObject();
        m_ResourceInfos.Add(path, ResObj);
        return ResObj.LoadTex(path, name, FileExt);
        #else
        string filePath = "assets/res/" + path;
        return (Texture2D)AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture2D) );
        #endif
    }

    static public string LoadConfig(string path, string namekey )
    {
        path = path.ToLower();
        string name = namekey;
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
        path        = "assets/res/" + path;
        int pos1    = path.LastIndexOf('.');
        path        = path.Substring(0, pos1);

        ResourceObject ResObj = null;
        m_ResourceInfos.TryGetValue(path, out ResObj);
        if (ResObj != null)
        {
            if (ResObj.m_Asset == null)
            {
                return "";
            }
            TextAsset ret = ResObj.m_Asset.LoadAsset<TextAsset>(name);
            if(null == ret)
            {
                Debug.LogErrorFormat("Load {0} from {1} failed!", name, path);
                return "";
            }
            return ret.text;
        }

        // 加载自己
        ResObj      = new ResourceObject();
        m_ResourceInfos.Add(path, ResObj);
        return ResObj.LoadTxt(path.ToLower(), name, FileExt);

#else
        /// 试着用www 读取 
        string filePath = "file://" + rootPathEditor + "/res/" + path;
        WWW www = new WWW(filePath);
        while (!www.isDone) ;

        if (www.error != null)
            return "";
        return www.text;
        #endif
    }


    static public AudioClip LoadSound(string path)
    {
        path = path.ToLower();
        string name = string.Empty;
        #if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
        path                = "assets/res/" + path;
        int pos1            = path.LastIndexOf('.');
        int pos2            = path.LastIndexOf('/') + 1;
        if (pos1 > 0)
        {
            name            = path.Substring(pos2, pos1 - pos2 );
        }
        path                = path.Substring(0, pos1);

        ResourceObject ResObj = null;
        m_ResourceInfos.TryGetValue(path, out ResObj);
        if (ResObj != null && ResObj.m_Asset != null)
        {
            AudioClip ret = ResObj.m_Asset.LoadAsset<AudioClip>(name);
            return ret;
        }

        // 加载自己
        ResObj      = new ResourceObject();
        m_ResourceInfos.Add(path, ResObj);
        return ResObj.LoadSound(path, name, FileExt);
        #else
        string filePath = "assets/res/" + path;
        return (AudioClip)AssetDatabase.LoadAssetAtPath(filePath, typeof(AudioClip) );
        #endif
    }

    /// <summary>
    /// txt 文件应该没有依赖关系
    /// </summary>
    static public string LoadTxt( string path )
    {
        path                = path.ToLower();
        #if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
        path        = "assets/res/" + path;
        string name = string.Empty;
        int pos1    = path.LastIndexOf('.');
        int pos2    = path.LastIndexOf('/') + 1;
        if (pos1 > 0)
        {
            name    = path.Substring(pos2, pos1 - pos2);
        }
        path        = path.Substring(0, pos1);

        ResourceObject ResObj = null;
        m_ResourceInfos.TryGetValue(path, out ResObj);
        if (ResObj != null && ResObj.m_Asset != null)
        {
            TextAsset ret = ResObj.m_Asset.LoadAsset<TextAsset>(name);
            return ret.text;
        }

        // 加载自己
        ResObj      = new ResourceObject();
        m_ResourceInfos.Add(path, ResObj);
        return ResObj.LoadTxt(path.ToLower(), name, FileExt);

        #else
        /// 试着用www 读取 
        string filePath = "file://" + rootPathEditor + "/res/" + path;
        WWW www = new WWW(filePath);
        while (!www.isDone) ;

        if (www.error != null)
            return "";
        return www.text;
        #endif
    }
}

