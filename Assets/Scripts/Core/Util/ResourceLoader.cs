using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;





public class ResourceObject
{
    public AssetBundle              m_Asset;

    private DateTime                m_LastUseTime;

    private readonly List<object>   m_DependencyList;



    public ResourceObject( )
    { 
        m_LastUseTime       = DateTime.Now;
        m_DependencyList    = new List<object>();
    }


    public void AddDependency(object dependency)
    {
        if (m_DependencyList.Contains(dependency))
        {
            return;
        }

        m_DependencyList.Add(dependency);
    }


    /// <summary>
    /// 释放资源
    /// </summary>
    public void Release( bool isShutdown )
    {

    }


    /// <summary>
    /// 获取对象时的事件。
    /// </summary>
    public void OnSpawn()
    {

    }

    /// <summary>
    /// 回收对象时的事件。
    /// </summary>
    public void OnUnspawn()
    {

    }


    public void LoadPacker(string SpritePacker, string fileExt )
    {
        string filePath = LoadResManager.rootPathPersistent + SpritePacker + fileExt;
        FileInfo file = new FileInfo(filePath);
        if (file.Exists)
        {
            m_Asset  = AssetBundle.LoadFromFile(filePath);
        }
        else
        {
            filePath = LoadResManager.rootPathStreamAssets + SpritePacker + fileExt;
            m_Asset  = AssetBundle.LoadFromFile(filePath);
        }
    }

    public GameObject LoadRes( string Path, string name, string fileExt )
    {
        GameObject ret  = null;
        string filePath = LoadResManager.rootPathPersistent + Path + fileExt;
        FileInfo file   = new FileInfo(filePath);
        if (file.Exists)
        {
            m_Asset     = AssetBundle.LoadFromFile(filePath);
            ret         = m_Asset.LoadAsset<GameObject>(name);
        }
        else
        {
            filePath   = LoadResManager.rootPathStreamAssets + Path + fileExt;
            m_Asset    = AssetBundle.LoadFromFile(filePath);
            if (m_Asset == null)
                return null;
            ret        = m_Asset.LoadAsset<GameObject>(name);
        }
        return ret;
    }
    public void LoadScene(string Path, string name, string fileExt)
    {

        string filePath = LoadResManager.rootPathPersistent + Path + fileExt;
        FileInfo file   = new FileInfo(filePath);
        if (!file.Exists)
        {
            filePath = LoadResManager.rootPathStreamAssets + Path + fileExt;
        }

        m_Asset = AssetBundle.LoadFromFile(filePath);
        if (m_Asset == null)
        {
            Debug.LogErrorFormat("Load scene bundle {0} failed!", filePath);
        }
    }



    public Material LoadMat(string Path, string name, string fileExt )
    {
        Material ret  = null;
        string filePath = LoadResManager.rootPathPersistent + Path + fileExt;
        FileInfo file = new FileInfo(filePath);
        if (file.Exists)
        {
            m_Asset    = AssetBundle.LoadFromFile(filePath);
            ret        = m_Asset.LoadAsset<Material>(name);
        }
        else
        {
            filePath   = LoadResManager.rootPathStreamAssets + Path + fileExt;
            m_Asset    = AssetBundle.LoadFromFile(filePath);
            if (m_Asset == null)
                return null;
            ret        = m_Asset.LoadAsset<Material>(name);
        }
        return ret;
    }

    public AudioClip LoadSound(string Path, string name, string fileExt)
    {
        AudioClip ret   = null;
        string filePath = LoadResManager.rootPathPersistent + Path + fileExt;
        FileInfo file   = new FileInfo(filePath);
        if (file.Exists)
        {
            m_Asset     = AssetBundle.LoadFromFile(filePath);
            ret         = m_Asset.LoadAsset<AudioClip>(name);
        }
        else
        {
            filePath    = LoadResManager.rootPathStreamAssets + Path + fileExt;
            m_Asset     = AssetBundle.LoadFromFile(filePath);
            if (m_Asset == null)
                return null;
            ret         = m_Asset.LoadAsset<AudioClip>(name);
        }
        return ret;
    }

    public Texture2D LoadTex(string Path, string name, string fileExt)
    {
        Texture2D ret   = null;
        string filePath = LoadResManager.rootPathPersistent + Path + fileExt;
        FileInfo file   = new FileInfo(filePath);
       
        if ( file.Exists)
        {
            m_Asset = AssetBundle.LoadFromFile(filePath);
            ret = m_Asset.LoadAsset<Texture2D>(name);
        }
       
        else
        {
            filePath   = LoadResManager.rootPathStreamAssets + Path + fileExt;
            m_Asset    = AssetBundle.LoadFromFile(filePath);
            if (m_Asset == null)
                return null;
            ret        = m_Asset.LoadAsset<Texture2D>(name);
        }
        return ret;
    }

    public string LoadTxt( string Path, string name, string fileExt)
    {

        TextAsset ret   = null;
        string filePath = LoadResManager.rootPathPersistent + Path + fileExt;
        FileInfo file = new FileInfo(filePath);
        if (file.Exists)
        {
            m_Asset     = AssetBundle.LoadFromFile(filePath);
            ret         = m_Asset.LoadAsset<TextAsset>(name);
        }
        else
        {
            filePath    = LoadResManager.rootPathStreamAssets + Path + fileExt;
            m_Asset     = AssetBundle.LoadFromFile(filePath);
            if (m_Asset == null)
            {
                Debug.LogErrorFormat("Load {0} failed!", filePath);
                return null;
            }
            ret         = m_Asset.LoadAsset<TextAsset>(name);
        }

        if (ret == null)
        {
            Debug.LogErrorFormat("Load {0} from {1} failed!", name, filePath);
            return "";
        }
        return ret.text;
    }
}


