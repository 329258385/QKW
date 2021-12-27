#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Diagnostics;

[CustomEditor(typeof(Package))]
public class Package : Editor
{
    public static void Build()
    {
        var args = System.Environment.GetCommandLineArgs();
        string configPath = string.Empty;
        string channel = string.Empty;
        string outPath = string.Empty;
        string version = "0.0.1";
        string bundleVersion = "1";
        if (args.Length > 9)
        {
            configPath = args[9];
            UnityEngine.Debug.Log("9 param:" + configPath);
        }

        if (args.Length > 10)
        {
            channel = args[10];
            UnityEngine.Debug.Log("10 param: " + channel);
        }

        if (args.Length > 11)
        {
            outPath = args[11];
            UnityEngine.Debug.Log("11 param: " + outPath);
        }

        if (args.Length > 12)
        {
            version = args[12];
            UnityEngine.Debug.Log("12 param: " + version);
        }

        if (args.Length > 13)
        {
            bundleVersion = args[13];
            UnityEngine.Debug.Log("13 param: " + bundleVersion);
        }

        if(!BuildApk(configPath, channel, outPath, version, bundleVersion))
        {
            return;
        }

        // 加密
        RunShellThreadStart();
    }

    /// <summary>
    /// Build apk
    /// </summary>
    /// <param name="configPath"></param>
    /// <param name="channel"></param>
    /// <param name="outPath"></param>
    /// <param name="version"></param>
    /// <param name="bundleVersion"></param>
    /// <returns></returns>
    public static bool BuildApk(string configPath, string channel, string outPath, string version, string bundleVersion)
    {
        if (configPath == string.Empty || channel == string.Empty)
        {
            UnityEngine.Debug.Log("config error: " + (configPath == string.Empty || channel == string.Empty));
            return false;
        }

        int configLocation = 0;
        string[] lines = System.IO.File.ReadAllLines(@configPath);
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
                            if (System.IO.Directory.Exists(config[1]))
                            {
                                if (System.IO.Directory.Exists(config[2]))
                                {
                                    System.IO.Directory.Delete(config[2], true);
                                }
                                CopyDirectory(config[1], config[2]);
                            }
                            else if (System.IO.File.Exists(config[1]))
                            {
                                if (System.IO.Directory.Exists(config[2]))
                                {
                                    System.IO.File.Delete(config[2]);
                                }
                                System.IO.File.Copy(config[1], config[2], true);
                            }
                            else
                            {
                                UnityEngine.Debug.LogError("复制文件错误，无法确定文件类型");
                            }
                        }
                        break;

                    case "delete":
                        if (config.Length > 2)
                        {
                            if (System.IO.Directory.Exists(config[1]))
                            {
                                System.IO.Directory.Delete(config[1], true);
                            }

                            if (System.IO.File.Exists(config[1]))
                            {
                                System.IO.File.Delete(config[1]);
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

        if (version != string.Empty)
        {
            Setting("Version", version);
        }

        if (bundleVersion != string.Empty)
        {
            Setting("BundleVersionCode", bundleVersion);
        }

        string[] scenes = new string[1];
        scenes[0] = Application.dataPath + "/Scenes/main.unity"; //每个工程都需要配置自己场景
        var result = BuildPipeline.BuildPlayer(scenes, outPath, BuildTarget.Android, BuildOptions.None); // BuildOptions.AcceptExternalModificationsToPlayer //导出android工程
        if (result != null )
        {
            UnityEngine.Debug.LogError("Apk打包成功");
            return true;
        }
        else
        {
            UnityEngine.Debug.LogError("Apk打包失败：" + result);
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
            case "BrowseKeyStore":
                PlayerSettings.Android.keystoreName = value;
                break;
            case "KeyStorePassword":
                PlayerSettings.Android.keystorePass = value;
                break;
            case "KeyAlias":
                PlayerSettings.Android.keyaliasName = value;
                break;
            case "KeyPassword":
                PlayerSettings.Android.keyaliasPass = value;
                break;
            case "BundleVersionCode":
                {
                    int code = 0;
                    int.TryParse(value, out code);
                    PlayerSettings.Android.bundleVersionCode = code;
                }
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
        System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(sourcePath);
        System.IO.Directory.CreateDirectory(destinationPath);
        foreach (System.IO.FileSystemInfo fsi in info.GetFileSystemInfos())
        {
            string destName = System.IO.Path.Combine(destinationPath, fsi.Name);

            if (fsi is System.IO.FileInfo)          //如果是文件，复制文件
                System.IO.File.Copy(fsi.FullName, destName, true);
            else                                    //如果是文件夹，新建文件夹，递归
            {
                System.IO.Directory.CreateDirectory(destName);
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
        foreach (string d in System.IO.Directory.GetFileSystemEntries(directoryPath))
        {
            if (System.IO.File.Exists(d))
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(d);
                if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                    fi.Attributes = System.IO.FileAttributes.Normal;
                System.IO.File.Delete(d);     //删除文件   
            }
            else
                DeleteFolder(d);    //删除文件夹
        }
        System.IO.Directory.Delete(directoryPath);    //删除空文件夹
    }
}

#endif