#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System;
using System.IO;

public static class PackageAndroid
{
    /// <summary>
    /// Build apk
    /// </summary>
    /// <param name="configPath"></param>
    /// <param name="channel"></param>
    /// <param name="outPath"></param>
    /// <param name="bundleVersion"></param>
    /// <param name="buildRes"></param>
    /// <returns></returns>
    public static bool BuildApk(string configPath, string channel, string outPath, int bundleVersion, bool buildRes)
    {
        if (configPath == string.Empty || channel == string.Empty)
        {
            UnityEngine.Debug.Log("config error: " + (configPath == string.Empty || channel == string.Empty));
            return false;
        }

        var configDir = configPath.Replace(Path.GetFileName(configPath), "");
        UnityEngine.Debug.LogFormat("Config dir {0}", configDir);

        int configLocation = 0;
        string[] lines = File.ReadAllLines(@configPath);
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("#"))
            {
                continue;
            }

            if (lines[i] == channel)
            {
                configLocation = i + 1;
                break;
            }
        }

        for (int j = configLocation; j < lines.Length; ++j)
        {
            if (lines[j].StartsWith("#"))
            {
                continue;
            }

            if (lines[j] == "end")
            {
                break;
            }

            var config = lines[j].Split(' ');
            if (config.Length > 0)
            {
                switch (config[0])
                {
                    case "setting":
                        if (config.Length > 2)
                        {
                            Setting(config[1], config[2]);
                        }
                        break;

                    case "copy":
                        if (config.Length > 2)
                        {
                            var srcPath = Path.Combine(configDir, config[1]);
                            var dstPath = Path.Combine(configDir, config[2]);
                            UnityEngine.Debug.LogFormat("Copy from {0} to {1}", srcPath, dstPath);
                            if (Directory.Exists(srcPath))
                            {
                                if (Directory.Exists(dstPath))
                                {
                                    Directory.Delete(dstPath, true);
                                }
                                CopyDirectory(srcPath, dstPath);
                            }
                            else if (File.Exists(srcPath))
                            {
                                if (File.Exists(dstPath))
                                {
                                    File.Delete(dstPath);
                                }
                                File.Copy(srcPath, dstPath, true);
                            }
                            else
                            {
                                UnityEngine.Debug.LogErrorFormat("File or Dir {0} is not exists!", srcPath);
                                return false;
                            }
                            AssetDatabase.ImportAsset(dstPath, ImportAssetOptions.ForceUpdate);
                        }
                        break;

                    case "delete":
                        if (config.Length > 1)
                        {
                            var path = Path.Combine(configDir, config[1]);
                            if (Directory.Exists(path))
                            {
                                UnityEngine.Debug.LogFormat("Delete directory {0}", path);
                                Directory.Delete(path, true);
                            }
                            else if (File.Exists(path))
                            {
                                UnityEngine.Debug.LogFormat("Delete file {0}", path);
                                File.Delete(path);
                            }
                        }
                        break;
                }
            }
        }

        if (outPath == string.Empty)
        {
            UnityEngine.Debug.LogError("15 error: out path is null");
            return false;
        }

        if (bundleVersion > 0)
        {
            PlayerSettings.Android.bundleVersionCode = bundleVersion;
        }
        AssetDatabase.ImportAsset("Assets/Plugins/Android", ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        // Asset bundle
        if (buildRes)
        {
            ExportAssetBundleAll.BuildAutoNameAll(BuildTarget.Android);
        }

        try
        {
            string[] scenes = new string[1];
            scenes[0] = Application.dataPath + "/Scenes/main.unity"; //每个工程都需要配置自己场景
            var result = BuildPipeline.BuildPlayer(scenes, outPath, BuildTarget.Android, BuildOptions.None); // BuildOptions.AcceptExternalModificationsToPlayer //导出android工程
            if (result != null )
            {
                UnityEngine.Debug.Log("Apk打包成功");
                return true;
            }
            else
            {
                UnityEngine.Debug.LogError("Apk打包失败：" + result);
                return false;
            }
        }
        catch(Exception e)
        {
            UnityEngine.Debug.LogErrorFormat("BuildPlayer failed!\n{0}", e.ToString());
            return false;
        }
    }

    public static void Setting(string name, string value)
    {
        switch (name)
        {
            case "CompanyName":
                PlayerSettings.companyName = value;
                break;
            case "ProductName":
                PlayerSettings.productName = value;
                break;
            case "PackageName":
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, value); //applicationIdentifier = value;
                break;
            case "Version":
                PlayerSettings.bundleVersion = value;
                break;
            case "ScriptingDefineSymbols":
                var micro = value;
                //PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
                //                 if (micro.EndsWith(";"))
                //                 {
                //                     micro += value;
                //                 }
                //                 else
                //                 {
                //                     micro += ";" + value;
                //                 }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, micro);
                break;
            case "SplashScreen":
                PlayerSettings.SplashScreen.show = value.ToLower() == "true";
                PlayerSettings.SplashScreen.showUnityLogo = false;// value.ToLower() == "true";
                if (PlayerSettings.SplashScreen.show == true)
                {
                    var logos = new PlayerSettings.SplashScreenLogo[1];
                    Sprite companyLogo = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Res/Resource/Icon/health_logo.png", typeof(Sprite));
                    logos[0] = PlayerSettings.SplashScreenLogo.Create(5, companyLogo);
                    PlayerSettings.Android.splashScreenScale = AndroidSplashScreenScale.Center;
                    PlayerSettings.SplashScreen.logos = logos;
                }
                else
                {
                    var logos = new PlayerSettings.SplashScreenLogo[1];
                    PlayerSettings.SplashScreen.logos = logos;
                }
                break;
        }
    }

    private static void RunShellThreadStart()
    {
        Process process = new Process();
        process.StartInfo.FileName = "EncryptApk.exe";
        //  process.StartInfo.Arguments = @"";
        //  process.StartInfo.WorkingDirectory = string.Format("{ 0}/../../../ EncryptDll/ ", Application.dataPath);
        process.StartInfo.CreateNoWindow = false;
        process.StartInfo.ErrorDialog = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        process.WaitForExit();
        int ExitCode = process.ExitCode;
    }

    public static void CopyDirectory(string sourcePath, string destinationPath)
    {
        DirectoryInfo info = new DirectoryInfo(sourcePath);
        Directory.CreateDirectory(destinationPath);
        foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
        {
            string destName = Path.Combine(destinationPath, fsi.Name);

            if (fsi is FileInfo)          //如果是文件，复制文件
                File.Copy(fsi.FullName, destName, true);
            else                                    //如果是文件夹，新建文件夹，递归
            {
                Directory.CreateDirectory(destName);
                CopyDirectory(fsi.FullName, destName);
            }
        }
    }

    /// <summary>
    /// 删除文件夹（及文件夹下所有子文件夹和文件）
    /// </summary>
    /// <param name="directoryPath"></param>
    public static void DeleteFolder(string directoryPath)
    {
        foreach (string d in Directory.GetFileSystemEntries(directoryPath))
        {
            if (File.Exists(d))
            {
                FileInfo fi = new FileInfo(d);
                if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                    fi.Attributes = FileAttributes.Normal;
                File.Delete(d);     //删除文件   
            }
            else
                DeleteFolder(d);    //删除文件夹
        }
        Directory.Delete(directoryPath);    //删除空文件夹
    }
}

#endif