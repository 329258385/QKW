using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Solarmax
{
    public class FileReader
    {
        private static int _line_ptr = 0;
        private static List<string> _line_array = new List<string>();
        private static int _element_ptr = 0;
        private static string[] _element_array = null;
        private static List<string> lines_temp = new List<string>();

		private static string[] _element_title_array = null;
		private static Dictionary<string, string> _element_dict = new Dictionary<string, string> ();

        private static void _reset()
        {
            _line_ptr = 0;
            _line_array.Clear();
            lines_temp.Clear();
            _element_ptr = 0;
            _element_array = null;

			_element_title_array = null;
			_element_dict.Clear ();
        }

        private static string _next_element()
        {
            return _element_array[_element_ptr++];
        }

		/// <summary>
		/// 加载文件
		/// </summary>
		/// <returns><c>true</c>, if path was loaded, <c>false</c> otherwise.</returns>
		/// <param name="filePath">File path.</param>
        public static bool LoadPath(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                string text = string.Empty;
                try
                {
                    if (filePath.Contains("://"))
                    {
                        WWW www = new WWW(filePath);
                        while (!www.isDone) ;
                        text = www.text;
                    }
                    else
                    {
                        if (File.Exists(filePath))
                        {
                            StreamReader file = File.OpenText(filePath);
                            text = file.ReadToEnd();
                            file.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogErrorFormat("Error Loading File [{0}]", e.ToString() );
                    return false;
                }

                return LoadText(text);
            }

            return false;
        }

        public static bool LoadText(string text)
        {
            _reset();

            if (!string.IsNullOrEmpty(text))
            {
                string[] lines = text.Split('\n');
				if (lines.Length == 1) {
					lines = text.Split ('\r');
				}
                string temp;
                for (int i = 0; i < lines.Length; ++i)
                {
                    temp = lines[i];
                    if (temp.EndsWith("\r"))
                    {
                        temp = temp.Substring(0, temp.Length - 1);
                    }
                    lines_temp.Add(temp);
                }

                DeleteComments();

                return true;
            }

            return false;
        }

        public static void UnLoad()
        {
            _reset();
        }

        /// <summary>
        /// 删除#开始的注释行
        /// </summary>
        private static void DeleteComments()
        {
			bool first = true;
            foreach (var line in lines_temp)
            {
				if (!line.StartsWith ("#") && !string.IsNullOrEmpty (line.Trim())) {
					_line_array.Add (line);
				} else {
					if (first && line.StartsWith ("#")) {
						GenerateElementDict (line);
						first = false;
					}
				}
            }
        
            lines_temp.Clear();
        }

		/// <summary>
		/// 根据表头生成表格的对应项
		/// </summary>
		/// <param name="titleLine">Title line.</param>
		private static void GenerateElementDict(string titleLine)
		{
			if (titleLine.StartsWith ("#"))
				titleLine = titleLine.Substring (1);
			if (titleLine.EndsWith ("#"))
				titleLine = titleLine.Substring (0, titleLine.Length - 1);
			
			_element_title_array = titleLine.Split ('\t');
			for (int i = 0; i < _element_title_array.Length; ++i)
			{
				_element_dict.Add (_element_title_array [i], string.Empty);
			}
		}

        public static bool IsEnd()
        {
            if (_line_ptr >= _line_array.Count)
                return true;

            string str = _line_array[_line_ptr];
            if (str == null || string.IsNullOrEmpty(str) || str.StartsWith("\t"))
                return true;

            return false;
        }

        public static void ReadLine()
        {
            _element_ptr = 0;
            _element_array = _line_array[_line_ptr].Split('\t');
            _line_ptr++;

			// 将数据增加到dict中，方便读取
			for (int i = 0; i < _element_title_array.Length; ++i)
			{
				string key = _element_title_array [i];
				string value = i < _element_array.Length ? _element_array [i] : string.Empty;
				_element_dict [key] = value;
			}
        }

        public static int ReadInt()
        {
            string n = _next_element();

			if (string.IsNullOrEmpty (n))
				return 0;

            return int.Parse(n);
        }

		public static int ReadInt(string tag)
		{
			string n = _element_dict [tag];

			if (string.IsNullOrEmpty (n))
				return 0;

			return int.Parse(n);
		}

		public static float ReadFloat()
		{
			string n = _next_element();

			if (string.IsNullOrEmpty (n))
				return 0;
			
			return float.Parse(n);
		}

		public static float ReadFloat(string tag)
		{
			string n = _element_dict [tag];

			if (string.IsNullOrEmpty (n))
				return 0;

			return float.Parse(n);
		}

        public static string ReadString()
        {
            string n = _next_element();
			if (n.StartsWith ("\""))
				n = n.Substring (1);
			if (n.EndsWith ("\""))
				n = n.Substring (0, n.Length - 1);

			n = n.Replace ("\\n", "\n");

            return n;
        }

		public static string ReadString(string tag)
		{
			string n = _element_dict [tag];
			if (n.StartsWith ("\""))
				n = n.Substring (1);
			if (n.EndsWith ("\""))
				n = n.Substring (0, n.Length - 1);

			n = n.Replace ("\\n", "\n");

			return n;
		}

        public static bool ReadBoolean()
        {
            string n = _next_element().ToLowerInvariant();
            if (n == "1" || n == "true")
            {
                return true;
            }

            return false;
        }

		public static bool ReadBoolean(string tag)
		{
			string n = _element_dict [tag].ToLowerInvariant();
			if (n == "1" || n == "true")
			{
				return true;
			}

			return false;
		}
    }
}
