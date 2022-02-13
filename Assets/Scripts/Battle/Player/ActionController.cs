using System;
using System.Collections.Generic;
using System.Linq;
using TimeLines;
using UnityEngine;



/// <summary>
/// TimeLine 控制器
/// </summary>
public class ActionController
{
    public Dictionary<string, MemberAction> Actions = new Dictionary<string, MemberAction>();

    public void PreloadAction(string skillName)
    {
        if (!Actions.ContainsKey(skillName))
        {
            Actions.Add(skillName, new MemberAction(skillName));
        }
    }

    public MemberAction Release(string skillName, ActionArgs args)
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(skillName))
        {
            Debug.LogError("FK 传过来一个空技能 !");
            return null;
        }
#endif
        MemberAction skill;
        if (Actions.ContainsKey(skillName))
        {
            skill = Actions[skillName];
        }
        else
        {
            skill = new MemberAction(skillName, args);
            Actions.Add(skillName, skill);
        }

        skill.Reset(args);
        skill.Start();
        return skill;
    }

    public void Stop(string skillName)
    {
        if (Actions.ContainsKey(skillName))
        {
            Actions[skillName].Stop();
        }
    }

    public void StopAll()
    {
        var e = Actions.GetEnumerator();
        while (e.MoveNext())
        {
            e.Current.Value.Stop();
        }
        e.Dispose();
    }


    public void Tick(float deltaTime)
    {
        var e = Actions.GetEnumerator();
        while (e.MoveNext())
        {
            if (e.Current.Value.Args != null)
            {
                e.Current.Value.Tick(deltaTime * e.Current.Value.Args.TimeScale);
            }
            else
            {
                e.Current.Value.Tick(deltaTime);
            }
        }
        e.Dispose();
    }
}

