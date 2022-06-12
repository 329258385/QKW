using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Solarmax;

public class ExportAssetBundle {

	public class TableAsset
	{
		/// <summary>
		/// 资源路径名
		/// </summary>
		public string assetPath;
		/// <summary>
		/// ab包路径
		/// </summary>
		public string assetBundlePath;
	}
	public class Ast
	{
		public string       pathName;
		public int          count;
	}

	private static string savePath = "Assets/AssetBundles/";
	[MenuItem("Mi/Build AssetBundle(Auto)")]
	public static void BuildAutoName()
	{

		Dictionary<System.Object, TableAsset> assets = new Dictionary<object, TableAsset> ();
		Object[] selects                        = Selection.objects;

		Dictionary<string, Ast> selectAssetMap  = new Dictionary<string, Ast> ();
		Dictionary<string, Ast> allAssetMap     = new Dictionary<string, Ast> ();
		for (int i = 0; i < selects.Length; ++i) {
			Ast a       = new Ast ();
			a.pathName  = AssetDatabase.GetAssetPath (selects [i]);
			a.count     = 0;
			selectAssetMap.Add (a.pathName, a);
			allAssetMap.Add (a.pathName, a);
		}

		// 生成所有的依赖项
		List<Ast> list = new List<Ast>();
		list.AddRange (selectAssetMap.Values);
		for (int i = 0; i < list.Count; ++i) {
			Ast a = list [i];
			// 获取所有依赖,如果是自身，则只引用增加；如果是其他，则新建或者获取，引用增加。
			string[] depends = AssetDatabase.GetDependencies (a.pathName, true);
			for (int j = 0; j < depends.Length; ++j) {
				string dstr = depends [j];
				if (dstr.EndsWith (".cs"))
					continue;
				
				if (dstr == a.pathName) {
					a.count++;
				} else {
					Ast d;
					if (allAssetMap.ContainsKey (dstr)) {
						d = allAssetMap [dstr];
					} else {
						d = new Ast ();
						d.pathName = dstr;
						d.count = 0;
						allAssetMap.Add (d.pathName, d);
					}

					d.count++;
					// 如果遍历的队列中没有这个依赖，则加在后面
					if (!list.Contains (d)) {
						list.Add (d);
					}
				}
			}
		
		}

		// 此时allAssetMap中包含所有这些资源和他们的依赖
		List<Ast> list2 = new List<Ast>();
		list2.AddRange (selectAssetMap.Values);
		for (int i = 0; i < list2.Count; ++i) {
			Ast now = list2 [i];
			// 获取专属依赖，打包在一起
			List<Ast> array = new List<Ast> ();
			array.Add (now);
			CalDepends (now, array, list2, allAssetMap);

			// array中包含所有专属资源，应该打包为同一个ab
			string filePath;
			string bundleName;
			if (now.pathName.StartsWith ("Assets/StreamingAssets"))
            {
				// streamingasset目录下的bundle名称为原来名称
				filePath = now.pathName;
				bundleName = filePath;
			}
            else
            {
				// 普通资源设置ab后缀
				int pos = now.pathName.LastIndexOf ('.');
				if (pos > 0) {
					filePath = now.pathName.Substring (0, pos);
				} else {
					filePath = now.pathName;
				}

				bundleName = filePath + ".ab";
			}

			bundleName = bundleName.ToLower ();

			// 生成表格
			TableAsset ta       = new TableAsset();
			ta.assetPath        = filePath;
			ta.assetBundlePath  = bundleName;
			assets.Add (filePath, ta);

			// 设置依赖项的bundle名
			for (int j = 0; j < array.Count; ++j) {
				AssetImporter ai = AssetImporter.GetAtPath (array [j].pathName);
				if (string.IsNullOrEmpty (ai.assetBundleName))
                {
					Debug.LogFormat ("new assets, set bundlename : {0} to asset : {1}", bundleName, array [j].pathName);
				}
                else if (ai.assetBundleName.Equals (bundleName))
                {
					Debug.LogFormat ("old assets, set bundlename : {0} to asset : {1}", bundleName, array [j].pathName);
				}
                else {
					Debug.LogFormat ("change ab name assets, old:{0}, set bundlename : {1} to asset : {2}", ai.assetBundleName, bundleName, array [j].pathName);
				}
				ai.assetBundleName = bundleName;
			}
		}

		if (!System.IO.Directory.Exists(savePath)) {
			System.IO.Directory.CreateDirectory (savePath);
		}

		BuildPipeline.BuildAssetBundles (savePath, BuildAssetBundleOptions.None, BuildTarget.Android);

		// 拷贝打包资源中的streamingassets文件，因为streamingasset不能打包，则直接拷贝
		for (int i = 0; i < list2.Count; ++i) {
			Ast now = list2 [i];
			if (now.pathName.StartsWith ("Assets/StreamingAssets")) {
				// 拷贝
				string dir = now.pathName.ToLower();
				int pos = dir.LastIndexOf ('/');
				dir = savePath + dir.Substring (0, pos + 1);
				if (!Directory.Exists (dir)) {
					Directory.CreateDirectory (dir);
				}
				AssetImporter ai = AssetImporter.GetAtPath (now.pathName);
				// 系统的拷贝方法
				File.Copy (now.pathName, savePath + now.pathName.ToLower(), true);
			}
		}

		Debug.Log ("Build Finished");
	}

	// 递归获取所有a资源的专属依赖，加入array，不是专属则加入global
	public static void CalDepends(Ast a, List<Ast> array, List<Ast> global, Dictionary<string, Ast> all)
	{
		string[] depends = AssetDatabase.GetDependencies (a.pathName, false);
		for (int i = 0; i < depends.Length; ++i) {
			string now_depends = depends [i];
			if (now_depends != a.pathName && !now_depends.EndsWith (".cs")) {
				Ast d = all [now_depends];
				if (d.count == a.count + 1) {
					array.Add (d);
					CalDepends (d, array, global, all);
				} else {
					if (!global.Contains (d)) {
						global.Add (d);
					}
				}
			}
		}
	}

	[MenuItem("Mi/CalculatePosition")]
	public static void CalculatePosition()
	{
		int count = 128;
		Vector3[] poses = new Vector3[count];
		poses [0] = new Vector3 (1, 0, 0);

		Vector3 centerPos = Vector3.zero;
		float angle = 360.0f / count;

		Vector3 axis = Vector3.Normalize(new Vector3(0f, 1f, -0.2f));

		Quaternion q = Quaternion.AngleAxis (angle, axis);
		Vector3 temp;
		for (int i = 1; i < count; ++i) {
			temp = q * poses [i - 1];
			poses [i] = temp;
		}

		System.Text.StringBuilder sb = new System.Text.StringBuilder ();
		sb.AppendLine ("using UnityEngine;");
		sb.AppendLine ("using System;");
		sb.AppendLine ("public static class MyCircle");
		sb.AppendLine ("{");
		sb.Append ("\tpublic static Vector3[] Positions = ");
		sb.AppendFormat ("new Vector3[{0}]", poses.Length);
		//sb.AppendFormat ("new Vector3[{0}]{", poses.Length);
		sb.AppendLine ("{\n");
		for (int i = 0; i < poses.Length; ++i) {
			temp = poses [i];
			sb.AppendFormat ("\tnew Vector3({0}f, {1}f, {2}f)", temp.x, temp.y, temp.z);
			if (i < poses.Length - 1)
				sb.Append (", ");

			if (i % 2 == 0)
				sb.Append ("\n\t");
		}
		sb.AppendLine ("\n\t\t};\n");
		sb.AppendLine ("}");

		string text = sb.ToString ();
		System.IO.FileStream fs = new System.IO.FileStream ("Assets/Scripts/Tools/MyCircle.cs", System.IO.FileMode.Create);
		byte[] bs = System.Text.Encoding.UTF8.GetBytes (text);
		fs.Write (bs, 0, bs.Length);
		fs.Flush ();
		fs.Close ();
	}

    [MenuItem("Mi/删除用户数据")]
    public static void DeletePlayerData() {
        Debug.Log("-------删除本地数据和录像，方便测试---------");
        PlayerPrefs.DeleteAll();
        string filePath = Application.dataPath + "/video2/";//UpdateSystem.Get().saveVideo;
        if (Directory.Exists(filePath)) {
            foreach (string d in Directory.GetFileSystemEntries(filePath)) {
                if (File.Exists(d)) {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);//直接删除其中的文件  
                }
            }
        }
    }

    [MenuItem("Mi/开启所有章节")]
    public static void UnlockAllChapters() {
        Debug.Log("-------解锁所有章节---------");
        PlayerPrefs.SetInt("UnlockAllChapters", 1);
        if (LevelDataHandler.Instance != null && LevelDataHandler.Instance.chapterList.Count > 0) {
            foreach (var chapter in LevelDataHandler.Instance.chapterList) {
                chapter.unLock = true;
            }
        }
    }

    [MenuItem("Mi/开启所有关卡（运行时生效）")]
    public static void UnlockAllLevels()
    {
        Debug.Log("-------解锁所有关卡---------");
        PlayerPrefs.SetInt("UnlockAllChapters", 1);
        if (LevelDataHandler.Instance != null && LevelDataHandler.Instance.chapterList.Count > 0)
        {
            foreach (var chapter in LevelDataHandler.Instance.chapterList)
            {
                chapter.unLock = true;
                foreach (var level in chapter.levelList)
                {
                    level.unLock = true;
                }
            }
        }
    }
}
