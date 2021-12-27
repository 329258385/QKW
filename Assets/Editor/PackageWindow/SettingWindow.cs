using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Threading;
using System.IO;


public class PackageSetting : EditorWindow {
    /// <summary>
    /// 宏定义：宏定义
    /// </summary>
    const string MICRO = "ERSION;UINTY_EDITORMAP_SERVER;NO_RACE_VERSION;UINTY_EDITORMAP;";

    /// <summary>
    /// 宏定义：开发服务器
    /// </summary>
    const string LN_SERVER = "LN_SERVER;";
    
    /// <summary>
    /// 宏定义：海外服务器
    /// </summary>
    const string EN_SERVER = "EN_SERVER;";

    /// <summary>
    /// 宏定义：国内服务器
    /// </summary>
    const string CN_SERVER = "CN_SERVER;";

    /// <summary>
    /// 宏定义：测试包
    /// </summary>
    const string TEST_PACKAGE = "TEST_PACKAGE;"; 

    string version = "0.1.3"; // 版本号
    string microDefine = MICRO; 
    int money = 1000;           //  初始金币


    bool ln_Server  = false;      // 复选框状态
    bool en_Server  = false;
    bool cn_Server  = false;
    bool testStatus = false;

    [MenuItem("Mi/打包")]
    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(PackageSetting));
    }

    void OnGUI() {
        GUILayout.Space(5);
        GUI.skin.label.fontSize = 15;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("打包设置");
        GUILayout.Space(5);
        // money = EditorGUILayout.IntField("初始金币", money);
        // EditorGUILayout.BeginHorizontal();
        testStatus  = EditorGUILayout.Toggle("测试包(包含GM命令)", testStatus);
        ln_Server   = EditorGUILayout.Toggle("开发", ln_Server);
        en_Server   = EditorGUILayout.Toggle("海外", en_Server);
        cn_Server   = EditorGUILayout.Toggle("国内", cn_Server);
        microDefine = "ERSION;UINTY_EDITORMAP_SERVER;NO_RACE_VERSION;UINTY_EDITORMAP;";
        version = EditorGUILayout.TextField("版本号", version);
        microDefine = EditorGUILayout.TextField("宏定义", microDefine);
        // EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        GUI.skin.button.fontSize = 15;
        if (GUILayout.Button("开始打包")) {
            if (Application.platform == RuntimePlatform.WindowsEditor) { // Android 打包
                if (ln_Server) {
                    microDefine += LN_SERVER;
                }
                if(cn_Server)
                {
                    microDefine += CN_SERVER;
                }

                if (en_Server)
                {
                    microDefine += EN_SERVER;
                }

                if (testStatus)
                {
                    microDefine += TEST_PACKAGE;
                }

                PlayerSettings.bundleVersion = version;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, microDefine);
#if UNITY_EDITOR_WIN
                var defaultPath = string.Format("{0}/../../../EncryptDll/package/", Application.dataPath);
                string outputPath = EditorUtility.SaveFilePanel("请选择APK目录", defaultPath, version, "apk");
                if (string.IsNullOrEmpty(outputPath)) {
                    UnityEngine.Debug.LogError("未选择打包路径，选择完成点击保存");
                    return;
                }

                string[] scenes = new string[1];
                scenes[0] = Application.dataPath + "/Scenes/Game.unity";
                var result = BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.Android, BuildOptions.None);
                if (result != null ) {
                    RunShellThreadStart();
                } else {
                    UnityEngine.Debug.LogError("Apk打包失败：" + result);
                }
#endif
            } else if (Application.platform == RuntimePlatform.OSXEditor) { // IOS 打包
                    // TODO::IOS打包，暂未完成.....
                    UnityEngine.Debug.LogError("IOS打包，暂未完成.....");
            } else {                                                        // 其他平台打包
                UnityEngine.Debug.LogError("其他平台，暂未完成.....");
            }

                return;

            }
        }

    private static void RunShellThreadStart() {
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
}
