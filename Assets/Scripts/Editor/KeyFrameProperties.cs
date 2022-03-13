using System;
using System.Collections;
using System.Collections.Generic;
using TimeLines;
using UnityEditor;
using UnityEngine;






public class KeyFrameProperties : EditorWindow
{

    public TimeLine             Timeline;
    //public TimeLineKeyframe     Frame;

    public List<KeyFrameArgs>   Actions;
    //public void SetFrame(TimeLine line, TimeLineKeyframe frame)
    //{
    //    Actions                 = frame.Actions;
    //    Timeline                = line;
    //    Frame                   = frame;
    //}

    public virtual void AddAction()
    {

    }

    public void AddAction<T>(T args) where T: KeyFrameArgs
    {
        Actions.Add(args);
    }

    //public virtual void OnGUI()
    //{
    //    EditorGUILayout.Space();
    //    DragAndDrop.visualMode      = DragAndDropVisualMode.Generic;
    //    if (Frame != null)
    //    {
    //        EditorGUILayout.HelpBox(string.Format("ID = {0} , Time = {1} , Count = {2}", Frame.id, Frame.time, Actions.Count), MessageType.Info);
    //        for (int i = 0; i < Actions.Count; i++)
    //        {
    //            EditorGUILayout.Space();
    //            EditorGUILayout.BeginHorizontal();
    //            Actions[i].Foldout = EditorGUILayout.Foldout(Actions[i].Foldout, " No." + (i + 1) + " : ");
    //            if (GUILayout.Button("×", GUILayout.Width(50)))
    //            {
    //                Actions.RemoveAt(i);
    //                break;
    //            }
    //            EditorGUILayout.EndHorizontal();

    //            if (Actions[i].Foldout)
    //            {
    //                Actions[i].Operation        = (KeyFrameArgs.OperationType) EditorGUILayout.EnumPopup("操作: ", Actions[i].Operation);
    //                EditorGUILayout.HelpBox("创建或者删除",MessageType.Info);
    //                var fs                      =  Actions[i].GetType().GetFields();
    //                for (int j = 0; j < fs.Length; j++)
    //                {
    //                    EditorGUILayout.BeginHorizontal();
    //                    ArgsNote argsNote       = null;
    //                    string opName           = fs[j].Name;
    //                    var ans = fs[j].GetCustomAttributes(typeof (ArgsNote),true);
    //                    if (ans.Length > 0 && (argsNote = ans[0] as ArgsNote) != null)
    //                    {
    //                        opName              = argsNote.Title;
    //                    }
    //                    if (fs[j].FieldType == typeof(string))
    //                    {
    //                        var val             = EditorGUILayout.TextField(opName + " : ", (string)fs[j].GetValue(Actions[i]));
    //                        fs[j].SetValue(Actions[i], val);
    //                    }
    //                    if (fs[j].FieldType == typeof(int))
    //                    {
    //                        var val             = EditorGUILayout.IntField(opName + " : ", (int)fs[j].GetValue(Actions[i]));
    //                        fs[j].SetValue(Actions[i], val);
    //                    }
    //                    if (fs[j].FieldType == typeof(bool))
    //                    {
    //                        var val             = EditorGUILayout.Toggle(opName + " : ", (bool)fs[j].GetValue(Actions[i]));
    //                        fs[j].SetValue(Actions[i], val);
    //                    }
    //                    if (fs[j].FieldType == typeof(float))
    //                    {
    //                        var val             = EditorGUILayout.FloatField(opName + " : ", (float)fs[j].GetValue(Actions[i]));
    //                        fs[j].SetValue(Actions[i], val);
    //                    }
    //                    if (fs[j].FieldType == typeof(Vector3))
    //                    {
    //                        var val             = EditorGUILayout.Vector3Field(opName + " : ", (Vector3)fs[j].GetValue(Actions[i]));
    //                        fs[j].SetValue(Actions[i], val);
    //                    }
    //                    if (argsNote != null && !string.IsNullOrEmpty(argsNote.Note))
    //                    {
    //                        EditorGUILayout.PrefixLabel(argsNote.Note);
    //                    }
    //                    EditorGUILayout.EndHorizontal();
    //                }
    //            }
    //        }
    //        EditorGUILayout.Space();
    //        if (GUILayout.Button("AddAction"))
    //        {
    //            AddAction();
    //        }
    //    }
    //}
}
public class LaunchKeyFrame : KeyFrameProperties
{
    public override void AddAction()
    {
        base.AddAction(new LaunchKeyFrameExportArgs());
    }
}

public class EffectKeyFrame : KeyFrameProperties
{
    public override void AddAction()
    {
        base.AddAction(new EffectKeyFrameExportArgs());
    }
}

public class SoundKeyFrame : KeyFrameProperties
{
    public override void AddAction()
    {
        base.AddAction(new SoundKeyFrameExportArgs());
    }
}
public class ActionKeyFrame : KeyFrameProperties
{
    public override void AddAction()
    {
        base.AddAction(new ActionKeyFrameExportArgs());
    }
}
public class ShakeKeyFrame : KeyFrameProperties
{
    public override void AddAction()
    {
        base.AddAction(new ShakeKeyFrameExportArgs());
    }
}
public class LineKeyFrame : KeyFrameProperties
{
    public override void AddAction()
    {
        base.AddAction(new LineKeyFrameExportArgs());
    }
}
public class ComponentKeyFrame : KeyFrameProperties
{
    public override void AddAction()
    {
        base.AddAction(new ComponentKeyFrameExportArgs());
    }
}