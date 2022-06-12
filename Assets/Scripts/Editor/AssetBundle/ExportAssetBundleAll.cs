using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Solarmax;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using GameCore.Loader;

public class ExportAssetBundleAll {

	public class TableAsset
	{
		/// <summary>
		/// 资源路径名
		/// </summary>
		public string   assetPath;

        /// <summary>
        /// md5码
        /// </summary>
        public string   GUID;

        /// <summary>
        /// 文件
        /// </summary>
        public long     Size;

		/// <summary>
		/// ab包路径
		/// </summary>
		public string   assetBundlePath;
	}

	public class Ast
	{
		public string       pathName;
		public int          count;
	}

	private static string savePath      = "Assets/StreamingAssets/";
    private static string versionPath   = "";
    private static string ResPath1      = "/Res/";
    private static string maplist       = "assets/res/data/mapconfigs.ab";



    static bool IsPliigoPackage = false;
    static Dictionary<string, Ast> assets           = new Dictionary<string, Ast>();
    static Dictionary<string, Ast> paddingAssets    = new Dictionary<string, Ast>();
    static Dictionary<string, TableAsset> filelist  = new Dictionary<string, TableAsset>();
    

    public static void BeginBuild()
    {
        assets.Clear();
        filelist.Clear();
        paddingAssets.Clear();
    }


    /// ---------------------------------------------------------------------------
    /// <summary>
    /// 整理所有的资源
    /// </summary>
    /// ---------------------------------------------------------------------------
    public static void TranssendoDirectory( string strPath )
    {
        if (string.IsNullOrEmpty(strPath))
            return;

        DirectoryInfo directory = new DirectoryInfo(strPath);
        FileInfo[] files        = directory.GetFiles("*", SearchOption.AllDirectories);
        for( int i = 0; i < files.Length; i++ )
        {
            if(files[i].Name.StartsWith(".", StringComparison.Ordinal))
            {
                continue;
            }
            if (files[i].FullName.EndsWith(".meta"))
                continue;

            if (files[i].FullName.EndsWith(".cs"))
                continue;

            if (files[i].FullName.EndsWith(".mat"))
                continue;

            if (files[i].FullName.IndexOf("NGUI") > 0)
                continue;

            int nPos = files[i].FullName.IndexOf("Resource");
            if (nPos > 0)
                continue;

            int nFlag       = files[i].FullName.LastIndexOf("StreamingAssets");
#if UNITY_EDITOR_WIN
            int nSubLength  = 0;
            if ( nFlag <= 0 )
                nSubLength  = files[i].FullName.LastIndexOf("Assets\\");
            else
                nSubLength  = files[i].FullName.IndexOf("Assets\\");
#else
            int nSubLength = 0;
            if (nFlag <= 0)
                nSubLength = files[i].FullName.LastIndexOf("Assets/");
            else
                nSubLength = files[i].FullName.IndexOf("Assets/");
#endif
            Ast a           = new Ast();
            a.pathName      = files[i].FullName.Substring(nSubLength).ToLower().Replace("\\", "/");
            a.count         = 0;
            assets.Add(a.pathName, a);
        }
    }


    /// ---------------------------------------------------------------------------
    /// <summary>
    /// 整理所有资源的,减少资源依赖
    /// </summary>
    /// ---------------------------------------------------------------------------
    public static void ProcessDependenciesAsset()
    {
        List<Ast> list = new List<Ast>();
        list.AddRange(assets.Values);
        for( int i = 0; i < list.Count; ++i )
        {
            Ast a = list[i];
            // 获取所有依赖,如果是自身，则只引用增加
            string[] depends = AssetDatabase.GetDependencies(a.pathName, true);
            for( int j = 0; j < depends.Length; ++j )
            {
                string filePath = depends[j];
                if (filePath.EndsWith(".cs"))
                    continue;

                if (filePath.EndsWith(".js"))
                    continue;

                if (filePath.EndsWith(".mat"))
                    continue;

                if (filePath.IndexOf("NGUI") > 0)
                    continue;

                int nPos = filePath.IndexOf("background");
                if (nPos > 0)
                    continue;

                if (filePath == a.pathName)
                {
                    a.count++;
                }
                else
                {
                    int nSubLength  = Application.dataPath.LastIndexOf("Assets");
                    string path     = Application.dataPath.Substring(0, nSubLength) + filePath;
                    Ast d           = new Ast();
                    d.pathName      = filePath.ToLower().Replace("\\", "/");
                    d.count         = 0;
                    if (!paddingAssets.ContainsKey(d.pathName))
                    {
                        paddingAssets.Add(d.pathName, d);
                    }
                }
            }

            if (!paddingAssets.ContainsKey(a.pathName))
            {
                paddingAssets.Add(a.pathName, a);
            }
        }
    }


    /// ------------------------------------------------------------------------------------
    /// <summary>
    /// 修改 AssetBundleName 标签
    /// </summary>
    /// ------------------------------------------------------------------------------------
    public static void ModifyBundleFlag()
    {
        List<Ast> list = new List<Ast>();
        list.AddRange( paddingAssets.Values );
        for( int i = 0; i < list.Count; i++ )
        {
            Ast ast             = list[i];
            string filePath     = ast.pathName;
            string bundleName   = string.Empty;
            int pos             = filePath.LastIndexOf('.');
            if (pos > 0)
            {
                filePath        = filePath.Substring(0, pos);
            }
            bundleName          = filePath + ".ab";
            AssetImporter ai    = AssetImporter.GetAtPath(ast.pathName);
            if(null == ai)
            {
                Debug.LogErrorFormat("Null AssetImporter of {0}", ast.pathName);
                continue;
            }
            if (string.IsNullOrEmpty(ai.assetBundleName))
            {
                Debug.LogFormat("new assets, set bundlename : {0} to asset : {1}", bundleName, ast.pathName);
            }
            else if (ai.assetBundleName.Equals(bundleName))
            {
                Debug.LogFormat("old assets, set bundlename : {0} to asset : {1}", bundleName, ast.pathName);
            }

            if (Path.GetDirectoryName(filePath).EndsWith("data/maplist", StringComparison.Ordinal))
            {
                ai.assetBundleName = maplist;
            }
            else
            {
                ai.assetBundleName = bundleName;
            }
        }
    }

    /// ------------------------------------------------------------------------------------
    /// <summary>
    /// 修改 AssetBundleName 标签
    /// </summary>
    /// ------------------------------------------------------------------------------------
    public static void ModifyBundleFlagNull()
    {
        List<Ast> list = new List<Ast>();
        list.AddRange(paddingAssets.Values);
        for (int i = 0; i < list.Count; i++)
        {
            Ast ast = list[i];
            string filePath = ast.pathName;
            string bundleName = string.Empty;
            int pos = filePath.LastIndexOf('.');
            if (pos > 0)
            {
                filePath = filePath.Substring(0, pos);
            }
            bundleName = filePath + ".ab";

            AssetImporter ai    = AssetImporter.GetAtPath(ast.pathName);
            ai.assetBundleName = "";
        }
    }

    /// <summary>
    /// ab
    /// </summary>
    /// <param name="targetPlatform"></param>
	public static void BuildAutoNameAll(BuildTarget targetPlatform)
	{
        BeginBuild();

        TranssendoDirectory(Application.dataPath + ResPath1);
        ProcessDependenciesAsset();
        ModifyBundleFlag();

        CheckSpritesTagsAndBundles();
        BuildPipeline.BuildAssetBundles(savePath, BuildAssetBundleOptions.None, targetPlatform);
        GeneriFileListsMD5();
        SaveFileList2Xml();
        DeleteAllManifests();
        Debug.Log ( "Build Finished" );
	}

    [MenuItem("Mi/生成游戏资源包 - Android")]
    public static void BuildAutoNameAllAndroid()
    {
        BuildAutoNameAll(BuildTarget.Android);
    }

   
    [MenuItem("Mi/生成游戏资源包 - IOS")]
    public static void BuildAutoNameAllIOS()
    {
        BuildAutoNameAll(BuildTarget.iOS);
    }

    [MenuItem("Mi/清楚 bundleName ")]
    public static void ClearBundleNameNULL()
    {
        BeginBuild();

        TranssendoDirectory(Application.dataPath + ResPath1);
        ProcessDependenciesAsset();
        ModifyBundleFlagNull();
    }

    static private void CheckSpritesTagsAndBundles()
    {
        string[] guids                  = AssetDatabase.FindAssets("t:sprite");
        Dictionary<string, string> dict = new Dictionary<string, string>();
        foreach (string guid in guids)
        {
            string pathSrc              = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter ti          = TextureImporter.GetAtPath(pathSrc) as TextureImporter;
            if( !string.IsNullOrEmpty(ti.spritePackingTag) )
            {
                string path    = pathSrc.ToLower().Replace("\\", "/");
                string bundle  = "";
                int pos        = path.LastIndexOf('/');
                if (pos > 0)
                {
                    bundle     = path.Substring(0, pos + 1);
                }
                bundle         = bundle + ti.spritePackingTag.ToLower() +  ".ab";
                AssetImporter ai = AssetImporter.GetAtPath(pathSrc);
                if (null != ai)
                {
                    ai.assetBundleName = bundle;
                }

            }
        }
    }

    /// ----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 计算文件的MD5
    /// </summary>
    /// ----------------------------------------------------------------------------------------------------------
    static public string GeneriFileMD5( string szFile )
    {
        try
        {
            FileStream file = new FileStream(szFile, FileMode.Open);
            MD5 md5         = new MD5CryptoServiceProvider();
            byte[] ret      = md5.ComputeHash(file);
            file.Close();

            StringBuilder sc= new StringBuilder();
            for (int i = 0; i < ret.Length; i++)
                sc.Append(ret[i].ToString("x2"));

            return sc.ToString();
        }
        catch( Exception e )
        {

        }
        return "";
    }


    static public void SaveFileList2Xml()
    {
        
        int nSubLength          = Application.dataPath.LastIndexOf("Assets");
        string path             = Application.dataPath.Substring(0, nSubLength);
        string url              = path + "/" + savePath + "filelist.xml";

        XmlTextWriter writer    = new XmlTextWriter(url, new System.Text.UTF8Encoding(false));
        writer.Formatting       = Formatting.Indented;

        writer.WriteStartDocument();
        writer.WriteStartElement("Assets");
        foreach( var v in filelist )
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
    }


    static public void GeneriFileListsMD5()
    {
        string path             = Application.dataPath;
        int nLength             = path.Length;
        #if UNITY_ANDROID
        int nSubLength  = 0;
        nSubLength      = path.IndexOf("Assets");
        path            = path.Substring(0, nSubLength );
        #elif UNITY_IPHONE
        int nSubLength  = 0;
        nSubLength      = path.IndexOf("Assets");
        path            = path.Substring(0, nSubLength );
        #endif
        
        string url      = path + savePath;

        DirectoryInfo directory = new DirectoryInfo(url);
        FileInfo[] files        = directory.GetFiles("*.ab", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            string fengefile        = "StreamingAssets";
            int nFlag               = files[i].FullName.LastIndexOf(fengefile) + fengefile.Length + 1;
            string filePath         = files[i].FullName.Substring(nFlag).ToLower().Replace("\\", "/");
            if (!filelist.ContainsKey(filePath))
            {
                TableAsset ta       = new TableAsset();
                ta.GUID             = GeneriFileMD5(files[i].FullName);
                ta.assetPath        = filePath.Replace(".ab", "");
                ta.Size             = files[i].Length;
                ta.assetBundlePath  = filePath;
                filelist.Add(filePath, ta);
            }
        }
    }


    static private void DeleteAllManifests()
    {
        DirectoryInfo directory = new DirectoryInfo(savePath);
        FileInfo[] files = directory.GetFiles("*.manifest", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            File.Delete(files[i].FullName);
            File.Delete(files[i].FullName + ".meta" );
        }

    }
}
