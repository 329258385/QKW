//---------------------------------------------------------------------------------------
// Copyright (c) MiGame 2019-2025
// Author: WYF
// Date: 2019-06-19
// Description: IOS打包配置
//---------------------------------------------------------------------------------------
using System;
using System.IO;
using UnityEngine;

[Serializable]
public class BuildConfigIOS
{
    /// <summary>
    /// BuildProperty
    /// </summary>
    [Serializable]
    public class BuildProperty
    {
        public string Name;
        public string Value;
    }

    /// <summary>
    /// 证书配置
    /// </summary>
    [Serializable]
    public class Certificate
    {
        public string ProvisioningProfileSpecifier;
        public string CodeSignIdentity;
        public string TeamId;
    }

    /// <summary>
    /// 拷贝文件
    /// </summary>
    [Serializable]
    public class CopyFile
    {
        public string FilePath;
        public string DstAssetDir;
    }

    /// <summary>
    /// SplashImage
    /// </summary>
    [Serializable]
    public class SplashImage
    {
        public string ImgPath;
    }

    /// <summary>
    /// 本地化配置
    /// </summary>
    [Serializable]
    public class LocalizationConfig
    {
        public string Directory;
        public string[] Languages;
    }

    /// <summary>
    /// 宏
    /// </summary>
    public string ScriptingDefineSymbols;

    /// <summary>
    /// Framework
    /// </summary>
    public string[] Frameworks;

    /// <summary>
    /// 设置属性
    /// </summary>
    public BuildProperty[] SetBuildProperties;

    /// <summary>
    /// 添加属性
    /// </summary>
    public BuildProperty[] AddBuildProperties;

    /// <summary>
    /// Info.plist
    /// </summary>
    public string InfoPlist;

    /// <summary>
    /// Plist
    /// </summary>
    public string[] AddPlists;

    /// <summary>
    /// Classes
    /// </summary>
    public string[] ClassesDirectories;

    /// <summary>
    /// 插件文件目录
    /// </summary>
    public string[] PlatformPluginDirectories;

    /// <summary>
    /// 拷贝文件
    /// </summary>
    public CopyFile[] CopyFiles;

    /// <summary>
    /// SDK接口文件
    /// </summary>
    public string[] SdkInterfaceFiles;

    /// <summary>
    /// Splash
    /// </summary>
    public SplashImage Splash;

    /// <summary>
    /// 本地化
    /// </summary>
    public LocalizationConfig Localization;

    /// <summary>
    /// 证书
    /// </summary>
    public Certificate CertificateDev;
    public Certificate CertificateAdhoc;
    public Certificate CertificateDistribution;

    /// <summary>
    /// 从json读取配置
    /// </summary>
    /// <returns>The from json.</returns>
    /// <param name="path">Path.</param>
    static public BuildConfigIOS ReadFromJson(string path)
    {
        try
        {
            var txt = File.ReadAllText(path);
            if (string.IsNullOrEmpty(txt))
            {
                Debug.LogErrorFormat("Read file {0} failed!", path);
                return null;
            }
            var cfg = JsonUtility.FromJson(txt, typeof(BuildConfigIOS)) as BuildConfigIOS;
            if (null == cfg)
            {
                Debug.LogErrorFormat("Parse BuildConfigIOS {0} failed!", path);
                return null;
            }

            // ProvisioningProfileSpecifier内容如果有"."会导致工程文件打不开
            return cfg;
        }
        catch(Exception e)
        {
            Debug.LogErrorFormat("BuildConfigIOS ReadFromJson {0} failed! \n{1}", path, e.ToString());
            return null;
        }
    }
}