using System;
using System.Collections.Generic;
using System.Linq;
using TimeLines;
using System.IO;
using System.Xml;
using UnityEngine;



/// <summary>
/// 每个技能的动作对应一个MemberAction
/// </summary>
public class MemberAction
{
    public enum ReleaseState
    {
        Ready,
        InRelease,
        End,
    }

    /// <summary>
    /// 动作持续了多长时间
    /// </summary>
    private float           _duration = 0.066f;
    public ActionArgs       Args;
    public ReleaseState     State;
    public string           SkillName;

    /// <summary>
    /// 
    /// </summary>
    private List<TimeLine>  _TimeLines;

    public MemberAction( string strAction )
    {
        SkillName        = strAction;
        State            = ReleaseState.Ready;
        SetTimeLines(strAction);
    }

    public MemberAction(string strAction, ActionArgs args )
    {
        SkillName       = strAction;
        this.Args       = args;
        this.State      = ReleaseState.Ready;
        SetTimeLines(strAction);
    }


    public void Tick( float delt )
    {
        if (State != ReleaseState.InRelease) return;
        bool r = false;
        if (_TimeLines == null)
        {
            return;
        }
        _duration += delt;
        for (int i = 0; i < _TimeLines.Count; i++)
        {
            if (_TimeLines[i].End == false)
            {
                _TimeLines[i].Tick(_duration);
                r = true;
            }
        }

        if (r == false)
        {
            Finish();
        }
    }

    /// <summary>
    /// 开始动作
    /// </summary>
    public void Start()
    {
        _duration       = 0f;
        State           = ReleaseState.InRelease;
    }

    public void Stop()
    {
        for( int i = 0; i < _TimeLines.Count; i++ )
        {
            _TimeLines[i].Stop();
        }
    }


    public void Finish()
    {
        State   = ReleaseState.End;
    }


    public void Reset( ActionArgs args )
    {
        this.Args       = args;
        this._duration  = 0f;
        for (int i = 0; i < _TimeLines.Count; i++)
        {
            _TimeLines[i].ReSet(Args);
        }
    }

    public void SetTimeLines( string strAction )
    {
        if( _TimeLines == null )
        {
            _TimeLines = new List<TimeLine>();
            if( !string.IsNullOrEmpty(strAction) )
            {
                if (ActionXMLManager.Get().XmlActions.ContainsKey(SkillName))
                {
                    var tls = ActionXMLManager.Get().XmlActions[SkillName];
                    for (int i = 0; i < tls.Count; i++)
                    {
                        _TimeLines.Add(tls[i].Clone(Args));
                    }
                }
                else
                {
                    Debug.LogError("技能报错 -> 找到Action : " + SkillName);
                }
            }
        }
    }
}


public class ActionXMLManager : Singleton<ActionXMLManager>
{
    public Dictionary<string, List<TimeLine>>   XmlActions = new Dictionary<string, List<TimeLine>>();
    public Dictionary<string, List<string>>     ActionEffects = new Dictionary<string, List<string>>();

    private List<TimeLine> CreateTimeLines(XmlDocument xml)
    {
        List<TimeLine> timeLines = new List<TimeLine>();
        var xSkill = xml.SelectSingleNode("Skill");
        if (xSkill != null)
        {
            if (xSkill.Attributes != null)
            {
                bool Loop = xSkill.Attributes["Loop"].Value == "1";
            }
            var xTimeLines = xSkill.SelectNodes("TimeLines");
            if (xTimeLines != null)
            {
                for (int i = 0; i < xTimeLines.Count; i++)
                {
                    var xTimeLine = xTimeLines[i];
                    if (xTimeLine.Attributes != null)
                    {
                        var tType = (TimeLine.Type)int.Parse(xTimeLine.Attributes["Type"].Value);
                        var timeLine = new TimeLine(tType, null);
                        timeLine.BuildKeyFrame(xTimeLine, timeLine, tType);
                        timeLines.Add(timeLine);
                    }
                }
            }
        }
        return timeLines;
    }

    private List<string> GetEffectFromTimeLine(List<TimeLine> timeLines)
    {
        List<string> ret = new List<string>();
        foreach (TimeLine timeLine in timeLines)
        {
            if (timeLine.LineType == TimeLine.Type.Effect)
            {
                foreach (var fm in timeLine._keyFrames)
                {
                    foreach (var v in fm.FramesActions)
                    {
                        if (v is EffectKeyFrameExportArgs)
                        {
                            ret.Add((v as EffectKeyFrameExportArgs).EffectPath);
                        }
                    }
                }
            }
        }
        return ret;
    }

    public void Init()
    {
        if (XmlActions.Count == 0)
        {
            //var nameXml         = GameConfigure.instance.Load("Skill.xml");
            //var eles            = nameXml.Elements();
            //var e               = eles.GetEnumerator();
            //while (e.MoveNext())
            //{
            //    XmlDocument xml = new XmlDocument();
            //    var path = Utility.GetStreamingAssetByPath("/Skill/" + e.Current.Name + ".xml");
            //    try
            //    {
            //        xml.LoadXml(Utility.ReadAStringFile(path));
            //        List<TimeLine> tileLines = CreateTimeLines(xml);
            //        XmlActions.Add(e.Current.Name.ToString(), tileLines);
            //        ActionEffects.Add(e.Current.Name.ToString(), GetEffectFromTimeLine(tileLines));
            //    }
            //    catch (Exception args)
            //    {
            //        DLog.LogError(args.Message);
            //        DLog.LogError(args.StackTrace);
            //    }
            //}
            //e.Dispose();

        }
    }
}

