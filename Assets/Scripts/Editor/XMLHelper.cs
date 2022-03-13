using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;





public class XMLHelper : Editor
{

    public struct XMLFiles
    {
        public string       Path;
        public string       FileName;
    }

    public static void LoadXML(string path)
    {
        Files               = new List<XMLFiles>();
        var files           = Directory.GetFiles(path, "*.xml");
        foreach (string t in files)
        {
            Files.Add(new XMLFiles{FileName = new FileInfo(t).Name, Path = t});
        }
    }


    public static XmlDocument       Active;
    public static List<XMLFiles>    Files;
    public static string[]          GetNames()
    {
        string[] names = new string[Files.Count];
        for (int i = 0; i < Files.Count; i++)
        {
            var fullName = Files[i].FileName;
            var ns = fullName.Split('_');
            for (int j = 0; j < ns.Length - 1; j++)
            {
                names[i] += ns[j] + "/";
            }
            names[i] += fullName;
        }
        return names;
    }
    public static XmlDocument GetActive(int index)
    {
        Active = new XmlDocument();
        Active.Load(Files[index].Path);
        return Active;
    }

    //public static bool SaveXML(List<TimeLine> lines,int index)
    //{
    //    var xml         = new XmlDocument();
    //    var skill       = xml.CreateElement("Skill");
    //    skill.SetAttribute("Loop", TimelinesWindow.Loop ? "1" : "0");
    //    xml.AppendChild(skill);
    //    for (int i = 0; i < lines.Count; i++)
    //    {
    //        var frames = lines[i].keys;
    //        var e = xml.CreateElement("TimeLines");
    //        e.SetAttribute("Type", ((int)lines[i].type).ToString());
    //        e.SetAttribute("TypeName", lines[i].type.ToString());
    //        skill.AppendChild(e);

    //        for (int j = 0; j < frames.Count; j++)
    //        {
    //            var frame = xml.CreateElement("Frame");
    //            frame.SetAttribute("Point", frames[j].time.ToString(CultureInfo.InvariantCulture));
    //            e.AppendChild(frame);

    //            for (int k = 0; k < frames[j].Actions.Count; k++)
    //            {
    //                var act = frames[j].Actions[k];
    //                var action = xml.CreateElement("Action");
    //                action.SetAttribute("Operation", act.Operation.ToString());
    //                var fs = act.GetType().GetFields();
    //                for (int l = 0; l < fs.Length; l++)
    //                {
    //                    if (fs[l].FieldType == typeof(string))
    //                    {
    //                        action.SetAttribute(fs[l].Name, fs[l].GetValue(act).ToString());
    //                    }
    //                    if (fs[l].FieldType == typeof(int))
    //                    {
    //                        action.SetAttribute(fs[l].Name, fs[l].GetValue(act).ToString());
    //                    }
    //                    if (fs[l].FieldType == typeof(bool))
    //                    {
    //                        action.SetAttribute(fs[l].Name, fs[l].GetValue(act).ToString());
    //                    }
    //                    if (fs[l].FieldType == typeof(float))
    //                    {
    //                        action.SetAttribute(fs[l].Name, fs[l].GetValue(act).ToString());
    //                    }
    //                    if (fs[l].FieldType == typeof(Vector3))
    //                    {
    //                        var v = (Vector3)fs[l].GetValue(act);
    //                        action.SetAttribute(fs[l].Name, v.x + "," + v.y + "," + v.z);
    //                    }
    //                }
    //                frame.AppendChild(action);
    //            }
    //        }
    //    }
    //    xml.Save(Files[index].Path);
    //    return true;
    //}
}
