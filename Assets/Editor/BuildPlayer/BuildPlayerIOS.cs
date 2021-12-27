//---------------------------------------------------------------------------------------
// Copyright (c) MiGame 2019-2025
// Author: WYF
// Date: 2019-06-19
// Description: IOS打包
//---------------------------------------------------------------------------------------
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
#if !UNITY_EDITOR_WIN
using UnityEditor.iOS.Xcode;
#endif

public static class BuildPlayerIOS
{
    /// <summary>
    /// 证书类型
    /// </summary>
    public enum ECertificateType
    {
        Dev,
        Adhoc,
        Distribution,
        Num
    }

    /// <summary>
    /// 配置文件路径
    /// </summary>
    static public string ConfigPath;
    static public string PackageName;

	/// <summary>
	/// 导出工程
	/// </summary>
    [MenuItem("Mi/Build IOS/guonei/ipa")]
    public static void BuildIOS_guonei() { BuildIOS("guonei", true, true); }
    [MenuItem("Mi/Build IOS/haiwai/ipa")]
    public static void BuildIOS_haiwai() { BuildIOS("haiwai", true, true); }
    [MenuItem("Mi/Build IOS/guonei/prj")]
    public static void BuildIOSProject_guonei() { BuildIOS("guonei", false, true); }
    [MenuItem("Mi/Build IOS/haiwai/prj")]
    public static void BuildIOSProject_haiwai() { BuildIOS("haiwai", false, true); }
	[MenuItem("Mi/Build IOS/guonei/prj_nores")]
	public static void BuildIOSProject_guonei_nores() { BuildIOS("guonei", false, false); }
	[MenuItem("Mi/Build IOS/haiwai/prj_nores")]
	public static void BuildIOSProject_haiwai_nores() { BuildIOS("haiwai", false, false); }

	/// <summary>
	/// 获取文件夹名字
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public static string GetFolderName(string path)
    {
        var idx = path.LastIndexOf('/');
        if(idx < 0)
        {
            idx = path.LastIndexOf('\\');
        }
        if(idx < 0)
        {
            return path;
        }
        var folderName = path.Substring(idx + 1);
        if(folderName.Contains("."))
        {
            return GetFolderName(path.Remove(idx));
        }
        return folderName;
    }

	/// <summary>
	/// Build
	/// </summary>
	/// <param name="configName"></param>
	/// <param name="buildIPA"></param>
	/// <param name="buildRes"></param>
	public static void BuildIOS(string configName, bool buildIPA, bool buildRes)
    {
        ConfigPath = Path.Combine(Application.dataPath, string.Format("../../plugin/iOS/{0}/cfg_{1}.txt", configName, configName));
        PackageName = configName;
#if false
        var packageDirectory1 = Path.Combine(Application.dataPath, string.Format("../../../../Packages/iOS/{0}", PackageName));
        OnPostprocessBuild(BuildTarget.iOS, packageDirectory1);
        return;
#endif
        // 加载配置
        var cfg = BuildConfigIOS.ReadFromJson(ConfigPath);
        if (null == cfg)
        {
            return;
        }
        var configDir = ConfigPath.Replace(Path.GetFileName(ConfigPath), "");

        // Plugin文件
        var pluginPath = Path.Combine(Application.dataPath, "Plugins/iOS");
        if(Directory.Exists(pluginPath))
        {
            Directory.Delete(pluginPath, true);
        }
        Directory.CreateDirectory(pluginPath);
        for (int i = 0; i < cfg.PlatformPluginDirectories.Length; ++i)
        {
            var dir = Path.Combine(configDir, cfg.PlatformPluginDirectories[i]);
            var folderName = GetFolderName(cfg.PlatformPluginDirectories[i]);
            CopyDirectory(dir, pluginPath);
        }
        AssetDatabase.ImportAsset(pluginPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
		
        // SDK接口文件
        var sdkInterfacePath = Path.Combine(Application.dataPath, "ThirdParty/SdkInterface");
        var fileBackupPath = Path.Combine(Application.dataPath, "../../Backup");
        var sdkInterfaceBackupPath = Path.Combine(fileBackupPath, "SdkInterface");
        CopyDirectory(sdkInterfacePath, sdkInterfaceBackupPath);
        for (int i = 0; i < cfg.SdkInterfaceFiles.Length; ++i)
        {
            var filePath = Path.Combine(configDir, cfg.SdkInterfaceFiles[i]);
            var dstSrcPath = Path.Combine(sdkInterfacePath, Path.GetFileName(filePath));
            File.Copy(filePath, dstSrcPath, true);
            AssetDatabase.ImportAsset(dstSrcPath, ImportAssetOptions.ForceUpdate);
        }

        // 资源文件
        for(int i = 0; i < cfg.CopyFiles.Length; ++i)
        {
            var fc = cfg.CopyFiles[i];
            var srcPath = Path.Combine(configDir, fc.FilePath);
            var dstPath = Path.Combine(Application.dataPath, fc.DstAssetDir);
            dstPath = Path.Combine(dstPath, Path.GetFileName(srcPath));
            File.Copy(srcPath, dstPath, true);
			AssetDatabase.ImportAsset(dstPath, ImportAssetOptions.ForceUpdate);
		}

        // 宏
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, cfg.ScriptingDefineSymbols);
        AssetDatabase.Refresh();

        // SplashImage
        if (string.IsNullOrEmpty(cfg.Splash.ImgPath))
        {
            PlayerSettings.iOS.SetiPadLaunchScreenType(iOSLaunchScreenType.Default);
            PlayerSettings.iOS.SetiPhoneLaunchScreenType(iOSLaunchScreenType.Default);
        }
        else
        {
            var srcPath = Path.Combine(configDir, cfg.Splash.ImgPath);
            var fileName = Path.GetFileName(srcPath);
            var dstPath = Path.Combine(Application.dataPath, "Res/Resource/Icon");
            dstPath = Path.Combine(dstPath, fileName);
            File.Copy(srcPath, dstPath, true);
            AssetDatabase.ImportAsset(dstPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
            PlayerSettings.iOS.SetiPadLaunchScreenType(iOSLaunchScreenType.ImageAndBackgroundRelative);
            PlayerSettings.iOS.SetiPhoneLaunchScreenType(iOSLaunchScreenType.ImageAndBackgroundRelative);
            var assetPath = string.Format("Assets/Res/Resource/Icon/{0}", fileName);
            var tex = AssetDatabase.LoadMainAssetAtPath(assetPath) as Texture2D;
            PlayerSettings.iOS.SetLaunchScreenImage(tex, iOSLaunchScreenImageType.iPhoneLandscapeImage);
            PlayerSettings.iOS.SetLaunchScreenImage(tex, iOSLaunchScreenImageType.iPhonePortraitImage);
            PlayerSettings.iOS.SetLaunchScreenImage(tex, iOSLaunchScreenImageType.iPadImage);
        }

        // Build
        try
        {
            // AssetBundle
            if(buildRes)
			{
				ExportAssetBundleAll.BuildAutoNameAll(BuildTarget.iOS);
			}

            // 场景
            List<string> scenes = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
            {
                var scene = EditorBuildSettings.scenes[i];
                if (!scene.enabled)
                {
                    continue;
                }
                scenes.Add(scene.path);
            }

            // 先删除目录
            var packageDirectory = Path.Combine(Application.dataPath, string.Format("../../../../Packages/iOS/{0}", PackageName));
            if (Directory.Exists(packageDirectory))
            {
                Directory.Delete(packageDirectory, true);
            }

            // Build
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = packageDirectory,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };
            BuildPipeline.BuildPlayer(buildPlayerOptions);

            // Postprocess
            OnPostprocessBuild(BuildTarget.iOS, packageDirectory);

            // IPA
            if (buildIPA)
            {
                BuildIPA(configName, packageDirectory);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogErrorFormat("Build player failed! config: {0} \n {1}", ConfigPath, e.ToString());
        }
        finally
        {
            // 恢复文件
            if (Directory.Exists(fileBackupPath))
            {
                CopyDirectory(sdkInterfaceBackupPath, sdkInterfacePath);
                Directory.Delete(fileBackupPath, true);
            }
            if (Directory.Exists(pluginPath))
            {
                Directory.Delete(pluginPath, true);
            }

            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// 模式替换
    /// </summary>
    /// <param name="input"></param>
    /// <param name="pattern"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    static string ReplacePattern(string input, string pattern, string s)
    {
        var m = Regex.Match(input, pattern);
        if(!m.Success)
        {
            UnityEngine.Debug.LogErrorFormat("Pattern {0} mismatch!", pattern);
            return null;
        }

        var output = Regex.Replace(input, pattern, s);
        UnityEngine.Debug.LogFormat("Replaced \"{0}\" to \"{1}\"", m.Value, s);
        return output;
    }

    /// <summary>
    /// Build后处理
    /// </summary>
    /// <param name="target">Target.</param>
    /// <param name="pathToBuiltProject">Path to built project.</param>
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
#if !UNITY_EDITOR_WIN
        if (target != BuildTarget.iOS)
        {
            return;
        }

        var configName = Path.GetFileName(ConfigPath);

        // 加载配置
        var cfg = BuildConfigIOS.ReadFromJson(ConfigPath);
        if (null == cfg)
        {
            return;
        }

        // Get project
        var projectPath = Path.Combine(pathToBuiltProject, "Unity-iPhone.xcodeproj/project.pbxproj");
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projectPath));
        string targetGUID = proj.TargetGuidByName("Unity-iPhone");

        // Frameworks
        for (int i = 0; i < cfg.Frameworks.Length; ++i)
        {
            proj.AddFrameworkToProject(targetGUID, cfg.Frameworks[i], false);
        }

        // SetBuildProperties
        for (int i = 0; i < cfg.SetBuildProperties.Length; ++i)
        {
            var p = cfg.SetBuildProperties[i];
            proj.SetBuildProperty(targetGUID, p.Name, p.Value);
        }

        // AddBuildProperties
        for (int i = 0; i < cfg.AddBuildProperties.Length; ++i)
        {
            var p = cfg.AddBuildProperties[i];
            proj.AddBuildProperty(targetGUID, p.Name, p.Value);
        }

        // 新增的Plist
        var configDir = ConfigPath.Replace(Path.GetFileName(ConfigPath), "");
        for (int i = 0; i < cfg.AddPlists.Length; ++i)
        {
            var pp = cfg.AddPlists[i];
            var fileName = Path.GetFileName(pp);
            var srcPath = Path.Combine(configDir, pp);
            var dstPath = Path.Combine(pathToBuiltProject, fileName);
            File.Copy(srcPath, dstPath, true);
            var guid = proj.AddFile(dstPath, fileName, PBXSourceTree.Source);
            proj.AddFileToBuild(targetGUID, guid);
        }

		// Class
		var dstClassesPath = Path.Combine(pathToBuiltProject, "Classes");
		for (int i = 0; i < cfg.ClassesDirectories.Length; ++i)
		{
			var dir = Path.Combine(configDir, cfg.ClassesDirectories[i]);
			CopyDirectory(dir, dstClassesPath, filePath =>
			{
				var fileName = Path.GetFileName(filePath);
				var guid = proj.AddFile(filePath, Path.Combine("Classes", fileName), PBXSourceTree.Source);
				proj.AddFileToBuild(targetGUID, guid);
			});
		}

        // 证书
        proj.SetBuildProperty(targetGUID, "PROVISIONING_PROFILE_SPECIFIER", cfg.CertificateAdhoc.ProvisioningProfileSpecifier);
        proj.SetBuildProperty(targetGUID, "CODE_SIGN_IDENTITY", cfg.CertificateAdhoc.CodeSignIdentity);
        proj.SetTeamId(targetGUID, cfg.CertificateAdhoc.TeamId);

        // ProvisioningProfileSpecifier内容如果有"."会导致工程文件打不开
        var s = proj.WriteToString();
        if (cfg.CertificateAdhoc.ProvisioningProfileSpecifier.Contains("."))
        {
            var rs = string.Format("\"{0}\"", cfg.CertificateAdhoc.ProvisioningProfileSpecifier);
            s = s.Replace(cfg.CertificateAdhoc.ProvisioningProfileSpecifier, rs);
            UnityEngine.Debug.LogFormat("Replace {0} to {1}", cfg.CertificateAdhoc.ProvisioningProfileSpecifier, rs);
        }

        // Localization
        if (!string.IsNullOrEmpty(cfg.Localization.Directory) && cfg.Localization.Languages.Length > 0)
        {
            // 开发语言
            s = ReplacePattern(s, @"developmentRegion.+;", "developmentRegion = en;");
            if(string.IsNullOrEmpty(s))
            {
                return;
            }

            // 支持的语言
            var lans = string.Format("knownRegions = ({0});", string.Join(", ", cfg.Localization.Languages));
            s = ReplacePattern(s, @"knownRegions[^;]*;", lans);
            if (string.IsNullOrEmpty(s))
            {
                return;
            }

#if false
            proj = new PBXProject();
            proj.ReadFromString(s);

            // 拷贝strings
            var fileGuids = new string[cfg.Localization.Languages.Length];
            for (int i = 0; i < cfg.Localization.Languages.Length; ++i)
            {
                var lan = cfg.Localization.Languages[i];
                var dirName = string.Format("{0}/{1}.lproj", cfg.Localization.Directory, lan);
                var dir = Path.Combine(configDir, dirName);
                var dstDir = Path.Combine(pathToBuiltProject, dirName);
                if (!CopyDirectory(dir, dstDir))
                {
                    return;
                }
                fileGuids[i] = proj.AddFile(string.Format("{0}/InfoPlist.strings", dstDir), lan, PBXSourceTree.Group);
                proj.AddFileToBuild(targetGUID, fileGuids[i]);
            }
            //var folder = Path.Combine(pathToBuiltProject, cfg.Localization.Directory);
            //var guid = proj.AddFile(folder, "InfoPlist.strings", PBXSourceTree.Source);
            //proj.AddFileToBuild(targetGUID, guid);
            s = proj.WriteToString();
#endif
        }

        // Save Changes
        File.WriteAllText(projectPath, s);

        // Info.plist
        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        PlistDocument plist = new PlistDocument();
        string plistFileText = File.ReadAllText(plistPath);
        plist.ReadFromString(plistFileText);

        // Merge
        var plistPath1 = Path.Combine(configDir, cfg.InfoPlist);
        var plist1 = new PlistDocument();
        plist1.ReadFromFile(plistPath1);
        foreach (var pair in plist1.root.values)
        {
            plist.root[pair.Key] = pair.Value;
        }

        // Save
        plist.WriteToFile(plistPath);

        // bundleId
        var bundleId = plist.root["CFBundleIdentifier"].AsString();
        UnityEngine.Debug.LogFormat("Bundle identifier: {0}", bundleId);

        // ExportOptions.plist(命令行打包需要使用)
        GenExportOptions(ECertificateType.Dev, cfg.CertificateDev, bundleId, pathToBuiltProject);
        GenExportOptions(ECertificateType.Adhoc, cfg.CertificateAdhoc, bundleId, pathToBuiltProject);
        GenExportOptions(ECertificateType.Distribution, cfg.CertificateDistribution, bundleId, pathToBuiltProject);

        UnityEngine.Debug.LogFormat("Builc xcode project {0} success.", projectPath);
#endif
        }

    /// <summary>
    /// 生成导出配置
    /// </summary>
    /// <param name="t"></param>
    /// <param name="cfg"></param>
    /// <param name="bundleId"></param>
    /// <param name="dir"></param>
    static void GenExportOptions(ECertificateType t, BuildConfigIOS.Certificate cfg, string bundleId, string dir)
    {
#if !UNITY_EDITOR_WIN
        var exportOptions = new PlistDocument();
        exportOptions.Create();
        string path;
        switch (t)
        {
            case ECertificateType.Dev:
                exportOptions.root.SetString("method", "development");
                path = Path.Combine(dir, "ExportOptions_Dev.plist");
                break;
            case ECertificateType.Adhoc:
                exportOptions.root.SetString("method", "ad-hoc");
                path = Path.Combine(dir, "ExportOptions_Adhoc.plist");
                break;
            case ECertificateType.Distribution:
                exportOptions.root.SetString("method", "app-store");
                path = Path.Combine(dir, "ExportOptions_AppStore.plist");
                break;
            default:
                UnityEngine.Debug.LogErrorFormat("Unknown CertificateType {0}!", t);
                return;
        }
        var dict = exportOptions.root.CreateDict("provisioningProfiles");
        dict.SetString(bundleId, cfg.ProvisioningProfileSpecifier);
        exportOptions.WriteToFile(path);
#endif
    }

    /// <summary>
    /// 生成IPA
    /// </summary>
    /// <param name="configName"></param>
    /// <param name="projectPath"></param>
    /// <returns></returns>
    static public bool BuildIPA(string configName, string projectPath)
    {
        var cmdPath = Path.Combine(Application.dataPath, "../../Tools/build_ipa.sh");
        var appName = string.Format("{0}_{1}", PlayerSettings.productName, configName);
        var exportPath = Path.Combine(projectPath, "Export");

        // Build     
        var cmd = new StringBuilder();
        cmd.AppendFormat(" {0}", cmdPath);
        cmd.AppendFormat(" {0}", projectPath);
        cmd.AppendFormat(" {0}", exportPath);
        cmd.AppendFormat(" {0}", appName);
        if (!RunCmd("/bin/bash", cmd.ToString()))
        {
            return false;
        }

        // 判断文件是否存在
        var appFile = Path.GetFullPath(Path.Combine(exportPath, string.Format("{0}_adhoc.ipa", appName)));
        if (!File.Exists(appFile))
        {
            UnityEngine.Debug.LogErrorFormat("Build ipa {0} failed!", appFile);
			return false;
        }
        UnityEngine.Debug.LogFormat("Build ipa {0} success.", appFile);

        // 打开文件夹
        EditorUtility.RevealInFinder(appFile);
        return true;
    }

	/// <summary>
	/// 拷贝目录（文件会覆盖）
	/// </summary>
	/// <param name="srcDirectory"></param>
	/// <param name="dstDirectory"></param>
	/// <param name="onCopyFile"></param>
	/// <returns></returns>
	static public bool CopyDirectory(string srcDirectory, string dstDirectory, Action<string> onCopyFile = null)
    {
        // 无效源目录
        if(!Directory.Exists(srcDirectory))
        {
            UnityEngine.Debug.LogErrorFormat("Directory {0} not exists!", srcDirectory);
            return false;
        }

        // 创建目录
        if (!Directory.Exists(dstDirectory))
        {
            Directory.CreateDirectory(dstDirectory);
        }

        // 获取目录下的文件和子目录
        DirectoryInfo dir = new DirectoryInfo(srcDirectory);
        FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
        foreach (FileSystemInfo i in fileinfo)
        {
            if(i.Name.StartsWith(".", StringComparison.Ordinal))
			{
				continue;
			}
			if (i.Extension == ".meta")
            {
                continue;
            }
            var dstPath = Path.Combine(dstDirectory, i.Name);
            if (i is DirectoryInfo)
            {
                // 递归调用复制子文件夹
                if(!CopyDirectory(i.FullName, dstPath))
                {
                    return false;
                }
                continue;
            }

            // 文件
            File.Copy(i.FullName, dstPath, true);
            if(null != onCopyFile)
			{
				onCopyFile(dstPath);
			}
            UnityEngine.Debug.LogFormat("Copy file {0} to {1}.", i.FullName, dstPath);
        }
        return true;
    }

    /// <summary>
    /// 运行cmd命令
    /// 会显示命令窗口
    /// </summary>
    /// <param name="cmdExe">指定应用程序的完整路径</param>
    /// <param name="cmdStr">执行命令行参数</param>
    static bool RunCmd(string cmdExe, string cmdStr)
    {
        bool result = false;
        try
        {
            using (Process myPro = new Process())
            {
                //指定启动进程是调用的应用程序和命令行参数
                ProcessStartInfo psi = new ProcessStartInfo(cmdExe, cmdStr);
                myPro.StartInfo = psi;
                myPro.Start();
                myPro.WaitForExit();
                result = true;
            }
        }
        catch(Exception e)
        {
            UnityEngine.Debug.LogErrorFormat("Run cmd {0} {1} failed! \n{2}", cmdExe, cmdStr, e.ToString());
            return false;
        }
        return result;
    }

    /// <summary>
    /// 运行cmd命令
    /// 不显示命令窗口
    /// </summary>
    /// <param name="cmdExe">指定应用程序的完整路径</param>
    /// <param name="cmdStr">执行命令行参数</param>
    static bool RunCmdWithOutWindow(string cmdExe, string cmdStr)
    {
        bool result = false;
        try
        {
            using (Process myPro = new Process())
            {
                myPro.StartInfo.FileName = "cmd.exe";
                myPro.StartInfo.UseShellExecute = false;
                myPro.StartInfo.RedirectStandardInput = true;
                myPro.StartInfo.RedirectStandardOutput = true;
                myPro.StartInfo.RedirectStandardError = true;
                myPro.StartInfo.CreateNoWindow = true;
                myPro.Start();
                //如果调用程序路径中有空格时，cmd命令执行失败，可以用双引号括起来 ，在这里两个引号表示一个引号（转义）
                string str = string.Format(@"""{0}"" {1} {2}", cmdExe, cmdStr, "&exit");

                myPro.StandardInput.WriteLine(str);
                myPro.StandardInput.AutoFlush = true;
                myPro.WaitForExit();

                result = true;
            }
        }
        catch(Exception e)
        {
            UnityEngine.Debug.LogErrorFormat("Run cmd {0} {1} failed! \n{2}", cmdExe, cmdStr, e.ToString());
            return false;
        }
        return result;
    }
}