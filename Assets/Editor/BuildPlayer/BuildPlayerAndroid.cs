//---------------------------------------------------------------------------------------
// Copyright (c) MiGame 2019-2025
// Author: WYF
// Date: 2019-07-06
// Description: Android打包
//---------------------------------------------------------------------------------------
using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Diagnostics;
using System.IO;

/// <summary>
/// 签名配置
/// </summary>
[Serializable]
public class KeystoreConfig
{
    public string FilePath;
    public string StorePassword;
    public string KeyAlias;
    public string KeyPassword;
}

public static class BuildPlayerAndroid
{
    /// <summary>
    /// 命令行打包
    /// Unity.exe -projectPath "" -logFile "" \
    /// -quit -batchmode -nographics \
    /// -executeMethod BuildPlayerAndroid.BuildCommand $channel,$outPath,$version,$bundleVersion
    /// </summary>
    public static void BuildCommand()
    {
        var cmdArgs = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(cmdArgs, "BuildPlayerAndroid.BuildCommand");
        if(index < 0)
        {
            UnityEngine.Debug.LogErrorFormat("BuildPlayerAndroid.BuildPlayerAndroid - Invalid args {0}", cmdArgs);
            return;
        }
        if(index + 1 >= cmdArgs.Length)
        {
            UnityEngine.Debug.LogErrorFormat("BuildPlayerAndroid.BuildPlayerAndroid - Invalid args {0}", cmdArgs);
            return;
        }
        var args = cmdArgs[index + 1].Split(',');
        if(args.Length < 3)
        {
            UnityEngine.Debug.LogErrorFormat("BuildPlayerAndroid.BuildPlayerAndroid - Invalid args {0}", args);
            return;
        }
        int vc;
        if(!int.TryParse(args[2], out vc))
        {
            vc = 0;
            //UnityEngine.Debug.LogErrorFormat("BuildPlayerAndroid.BuildPlayerAndroid - Invalid version code {0}", args[3]);
            //return;
        }
        BuildAndroid(args[0], vc, args[1], true, true);
    }

    [MenuItem("Mi/Build Android/LN")]
    public static void BuildAndroid_LN() { BuildAndroid("LN", 0, "", true); }
    [MenuItem("Mi/Build Android/LCN")]
    public static void BuildAndroid_LCN() { BuildAndroid("LCN", 0, "", true); }
    [MenuItem("Mi/Build Android/Google")]
    public static void BuildAndroid_Google() { BuildAndroid("google", 0, "", true); }
    [MenuItem("Mi/Build Android/Google_test")]
    public static void BuildAndroid_Google_test() { BuildAndroid("google_test", 0, "", true); }
    [MenuItem("Mi/Build Android/Xiaomi")]
    public static void BuildAndroid_Xiaomi() { BuildAndroid("xiaomi", 0, "", true); }
    [MenuItem("Mi/Build Android/LN_nores")]
    public static void BuildAndroid_LN_nores() { BuildAndroid("LN", 0, "", false); }
    [MenuItem("Mi/Build Android/LCN_nores")]
    public static void BuildAndroid_LCN_nores() { BuildAndroid("LCN", 0, "", false); }
    [MenuItem("Mi/Build Android/Google_nores")]
    public static void BuildAndroid_Google_nores() { BuildAndroid("google", 0, "", false); }
    [MenuItem("Mi/Build Android/Google_test_nores")]
    public static void BuildAndroid_Google_test_nores() { BuildAndroid("google_test", 0, "", false); }
    [MenuItem("Mi/Build Android/Xiaomi_nores")]
    public static void BuildAndroid_Xiaomi_nores() { BuildAndroid("xiaomi", 0, "", false); }

    /// <summary>
    /// BuildAndroid
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="bundleVersion"></param>
    /// <param name="outPath"></param>
    /// <param name="buildRes"></param>
    /// <param name="isBatchMode"></param>
    /// <returns></returns>
    public static bool BuildAndroid(string channel, int bundleVersion, string outPath, bool buildRes, bool isBatchMode = false)
    {
        // 目录
        if(string.IsNullOrEmpty(outPath))
        {
            var packageDirectory = Path.Combine(Application.dataPath, string.Format("../../../../Packages/Android/{0}", channel));
            outPath = Path.Combine(packageDirectory, string.Format("solarmax_{0}.apk", channel));
        }

        // 创建目录
        var dir = outPath.Replace(Path.GetFileName(outPath), "");
        if(!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // 删除旧文件
        if(File.Exists(outPath))
        {
            File.Delete(outPath);
        }

        // 版本号
        if(bundleVersion <= 0)
        {
            bundleVersion = PlayerSettings.Android.bundleVersionCode;
        }

        var packagePath = string.Format("{0}/../../Package", Application.dataPath);
        var configPath = string.Format("{0}/Config/Solarmax3.txt", packagePath);

        // output
        var sb = new StringBuilder("BuildAndroid:\n");
        sb.AppendFormat("channel: {0}\n", channel);
        sb.AppendFormat("outPath: {0}\n", outPath);
        sb.AppendFormat("version code: {0}\n", bundleVersion);
        sb.AppendFormat("package path: {0}\n", packagePath);
        sb.AppendFormat("config path: {0}\n", configPath);
        UnityEngine.Debug.Log(sb.ToString());

        // 签名
        KeystoreConfig cfg = LoadKeystoreConfig();
        if (null == cfg)
        {
            return false;
        }
        PlayerSettings.Android.keystoreName = cfg.FilePath;
        PlayerSettings.Android.keystorePass = cfg.StorePassword;
        PlayerSettings.Android.keyaliasName = cfg.KeyAlias;
        PlayerSettings.Android.keyaliasPass = cfg.KeyPassword;

        // Plugin文件
        var pluginPath = Path.Combine(Application.dataPath, "Plugins/Android");
        if (Directory.Exists(pluginPath))
        {
            Directory.Delete(pluginPath, true);
        }

        // build
        if (!PackageAndroid.BuildApk(configPath, channel, outPath, bundleVersion, buildRes))
        {
            return false;
        }

        // Plugin文件
        if (Directory.Exists(pluginPath))
        {
            Directory.Delete(pluginPath, true);
        }

        // 后处理
        if (!PostProcessApk(outPath))
        {
            return false;
        }

        // 判断文件是否存在
        if (!File.Exists(outPath))
        {
            UnityEngine.Debug.LogErrorFormat("Build apk {0} failed!", outPath);
            return false;
        }
        UnityEngine.Debug.LogFormat("Build apk {0} success.", outPath);

        // 打开文件夹
        if(!isBatchMode)
        {
            EditorUtility.RevealInFinder(outPath);
        }

        return true;
    }

    /// <summary>
    /// 后处理apk
    /// </summary>
    /// <param name="apkPath"></param>
    /// <returns></returns>
    public static bool PostProcessApk(string apkPath)
    {
#if false
        if (string.IsNullOrEmpty(apkPath))
        {
            apkPath = "e:/apk/e.apk";
        }
#endif
        if (!File.Exists(apkPath))
        {
            UnityEngine.Debug.LogErrorFormat("File {0} is not exists!", apkPath);
            return false;
        }

        // 文件夹
        var fileName = Path.GetFileName(apkPath);
        var dir = apkPath.Replace(fileName, "");
        var unpackDir = string.Format("{0}{1}", dir, Path.GetFileNameWithoutExtension(fileName));
        if (Directory.Exists(unpackDir))
        {
            Directory.Delete(unpackDir, true);
        }
        Directory.CreateDirectory(unpackDir);

        // 解压
        if(!UnpackApk(apkPath, unpackDir))
        {
            return false;
        }

        // 加密DLL
        if(!EncryptDll(unpackDir))
        {
            return false;
        }

        // 替换mono.so
        if(!ReplaceLibMono(unpackDir))
        {
            return false;
        }

        // 打包
        File.Delete(apkPath);
        if(!PackApk(unpackDir, apkPath))
        {
            Directory.Delete(unpackDir, true);
            return false;
        }

        // 临时目录
        Directory.Delete(unpackDir, true);
        return true;
    }

    /// <summary>
    /// 加密DLL
    /// </summary>
    /// <param name="apkPath"></param>
    /// <returns></returns>
    public static bool EncryptDll(string apkPath)
    {
        var dllPath = string.Format("{0}/assets/bin/Data/Managed/Assembly-CSharp.dll", apkPath);
        if(!File.Exists(dllPath))
        {
            UnityEngine.Debug.LogErrorFormat("File {0} not exit!", dllPath);
            return false;
        }

        var sb = new StringBuilder();
        var bytes = File.ReadAllBytes(dllPath);
        byte c = (byte)'x';
        sb.AppendFormat("EncryptDll code: {0}\n", c);
        sb.AppendFormat("Before encode bytes[0]:{0}\n", bytes[0]);
        bytes[0] = (byte)(bytes[0] ^ c);
        sb.AppendFormat("After encode bytes[0]:{0}\n", bytes[0]);
        sb.AppendFormat("After decode bytes[0]:{0}\n", bytes[0] ^ c);
        File.WriteAllBytes(dllPath, bytes);

        UnityEngine.Debug.Log(sb.ToString());
        return true;
    }

    /// <summary>
    /// 替换so
    /// </summary>
    /// <param name="apkPath"></param>
    /// <returns></returns>
    public static bool ReplaceLibMono(string apkPath)
    {
        var libs = new string[] { "lib/armeabi-v7a/libmono.so", "lib/x86/libmono.so" };
        for(int i = 0; i < libs.Length; ++i)
        {
            var lib = libs[i];
            var srcPath = string.Format("{0}/../../Package/{1}", Application.dataPath, lib);
            var dstPath = string.Format("{0}/{1}", apkPath, lib);
            if(!File.Exists(dstPath))
            {
                UnityEngine.Debug.LogWarningFormat("{0} not exists, ignored.", dstPath);
                continue;
            }
            if(!File.Exists(srcPath))
            {
                UnityEngine.Debug.LogErrorFormat("File {0} not exists!", srcPath);
                return false;
            }
            File.Copy(srcPath, dstPath, true);
            UnityEngine.Debug.LogFormat("Replaced {0} from {1}.", dstPath, srcPath);
        }
        return true;
    }

    /// <summary>
    /// 解压apk
    /// </summary>
    /// <param name="apkPath"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static bool UnpackApk(string apkPath, string dir)
    {
#if UNITY_EDITOR_WIN
        var cmd = string.Format("{0}/../../Tools/unpackapk.bat", Application.dataPath);
#else
        var cmd = string.Format("{0}/../../Tools/unpackapk.sh", Application.dataPath);
#endif
        var args = string.Format("{0} {1}", apkPath, dir);
        if (!RunCmd(cmd, args))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 加载签名配置
    /// </summary>
    /// <returns></returns>
    static KeystoreConfig LoadKeystoreConfig()
    {
        var cfgPath = string.Format("{0}/../../Package/Config/keystore.json", Application.dataPath);
        if (!File.Exists(cfgPath))
        {
            UnityEngine.Debug.LogErrorFormat("File {0} is not exists!", cfgPath);
            return null;
        }

        // 解析
        KeystoreConfig cfg = null;
        try
        {
            var txt = File.ReadAllText(cfgPath);
            cfg = JsonUtility.FromJson<KeystoreConfig>(txt);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogErrorFormat("Parse KeystoreConfig from {0} failed!\n{1}", cfgPath, e.ToString());
            return null;
        }

        // 检查签名文件
        var cfgDir = cfgPath.Replace(Path.GetFileName(cfgPath), "");
        var keystoreFilePath = string.Format("{0}{1}", cfgDir, cfg.FilePath);
        if (!File.Exists(keystoreFilePath))
        {
            UnityEngine.Debug.LogErrorFormat("File {0} is not exists!", keystoreFilePath);
            return null;
        }
        cfg.FilePath = keystoreFilePath;
        return cfg;
    }

    /// <summary>
    /// 打包apk
    /// </summary>
    /// <param name="path"></param>
    /// <param name="apkPath"></param>
    /// <returns></returns>
    public static bool PackApk(string path, string apkPath)
    {
        if (!Directory.Exists(path))
        {
            UnityEngine.Debug.LogErrorFormat("Directory {0} is not exists!", path);
            return false;
        }
        if(File.Exists(apkPath))
        {
            File.Delete(apkPath);
        }

        // 签名配置
        var cfgPath = string.Format("{0}/../../Package/Config/keystore.json", Application.dataPath);
        if (!File.Exists(cfgPath))
        {
            UnityEngine.Debug.LogErrorFormat("File {0} is not exists!", cfgPath);
            return false;
        }

        // 解析
        KeystoreConfig cfg = LoadKeystoreConfig();
        if(null == cfg)
        {
            return false;
        }

        // 打包、签名、对齐
#if UNITY_EDITOR_WIN
        var cmd = string.Format("{0}/../../Tools/packapk.bat", Application.dataPath);
#else
        var cmd = string.Format("{0}/../../Tools/packapk.sh", Application.dataPath);
#endif
        var apkDir = apkPath.Replace(Path.GetFileName(apkPath), "");
        var apkFileName = Path.GetFileNameWithoutExtension(apkPath);
        var sb = new StringBuilder();
        sb.AppendFormat("{0} {1} {2}", path, apkDir, apkFileName);
        sb.AppendFormat(" {0} {1} {2} {3}", cfg.FilePath, cfg.StorePassword, cfg.KeyPassword, cfg.KeyAlias);
        if (!RunCmd(cmd, sb.ToString()))
        {
            return false;
        }

        // 检查文件是否存在
        if (!File.Exists(string.Format("{0}{1}.apk", apkDir, apkFileName)))
        {
            return false;
        }

        // 删除临时文件
        File.Delete(string.Format("{0}{1}_unsigned.apk", apkDir, apkFileName));
        File.Delete(string.Format("{0}{1}_signed.apk", apkDir, apkFileName));

        return true;
    }

    /// <summary>
    /// 运行cmd命令
    /// </summary>
    /// <param name="cmdStr">执行命令行参数</param>
    static bool RunCmd(string cmdStr, string args = null)
    {
        try
        {
            var sb = new StringBuilder(cmdStr);
            if (!string.IsNullOrEmpty(args))
            {
                sb.AppendFormat(" {0}", args);
            }
            UnityEngine.Debug.LogFormat("RunCmd {0} start...", sb.ToString());
            using (Process pro = new Process())
            {
#if UNITY_EDITOR_WIN
                ProcessStartInfo psi = new ProcessStartInfo(cmdStr);
                if (!string.IsNullOrEmpty(args))
                {
                    psi.Arguments = args;
                }
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
#else
                ProcessStartInfo psi = new ProcessStartInfo("/bin/bash", sb.ToString());
#endif
                pro.StartInfo = psi;
                pro.Start();
                pro.WaitForExit();
                if(pro.ExitCode != 0)
                {
                    UnityEngine.Debug.LogErrorFormat("Run cmd {0} failed! exit code: {1}", cmdStr, pro.ExitCode);
                    return false;
                }
                UnityEngine.Debug.LogFormat("RunCmd {0} success.", sb.ToString());
                return true;
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogErrorFormat("Run cmd {0} failed! \n{1}", cmdStr, e.ToString());
            return false;
        }
    }
}