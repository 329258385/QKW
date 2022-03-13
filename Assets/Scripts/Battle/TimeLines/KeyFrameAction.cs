using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimeLines
{
    public class ArgsNote : Attribute
    {
        public string   Title;
        public string   Note;

        public ArgsNote(string title, string note)
        {
            Title       = title;
            Note        = note;
        }
    }

    public abstract class KeyFrameArgs
    {
        public bool             Foldout { get; set; }
        public OperationType    Operation;
        public enum OperationType
        {
            TurnOn,
            TurnOff,
        }

        public abstract KeyFrameArgs Clone();
    }

    public class ActionKeyFrameExportArgs : KeyFrameArgs
    {
        [ArgsNote("动作名字", "")]
        public string ActionName = "";
        [ArgsNote("动画播放速度", "-1代表根据技能时间自动算,默认1")]
        public float Speed = 1f;
        [ArgsNote("动画时间长度", "自动算速度时要写,对应动画文件的Length")]
        public float ActionFileLegth = 1f;
        [ArgsNote("技能准备位置", "针对大体型怪物先后退再攻击")]
        public Vector3 BackPos = Vector3.zero;
        [ArgsNote("后退时间", "数值越小速度越快")]
        public float BackTime = 1f;
        [ArgsNote("后退后待多久", "数值越小速度越快")]
        public float StayTime = 1f;
        [ArgsNote("恢复时间", "数值越小速度越快")]
        public float RecoveryTime = 1f;
        [ArgsNote("取消头部IK限制", "取消IK限制,在动画播放完再次限制")]
        public bool DisableHeadIK;
        [ArgsNote("取消身体IK限制", "取消IK限制,在动画播放完再次限制")]
        public bool DisableBodyIK;
        public override KeyFrameArgs Clone()
        {
            return new ActionKeyFrameExportArgs()
            {
                Foldout         = Foldout,
                Operation       = Operation,
                ActionName      = ActionName,
                Speed           = Speed,
                ActionFileLegth = ActionFileLegth,
                BackPos         = BackPos,
                BackTime        = BackTime,
                StayTime        = StayTime,
                RecoveryTime    = RecoveryTime,
                DisableHeadIK   = DisableHeadIK,
                DisableBodyIK   = DisableBodyIK,
            };
        }
    }

    public class EffectKeyFrameExportArgs : KeyFrameArgs
    {
        [ArgsNote("特效路径", "以Resources为根")]
        public string EffectPath = "";
        [ArgsNote("骨骼点的名字", "骨骼点上要放BonePoint脚本并设置")]
        public string BonePoint = "";
        [ArgsNote("特效命名", "强制取消会用到")]
        public string EffectName = "";
        [ArgsNote("生命周期", "-1代表特效自带或逻辑设置")]
        public float LifeTime = -1;
        [ArgsNote("自动删除", "是否自动删除")]
        public bool AutoDestroy = true;
        [ArgsNote("绑定骨骼", "跟随移动")]
        public bool BuildBone = true;
        [ArgsNote("缩放", "是否跟随释放者")]
        public bool AutoScale = true;
        [ArgsNote("出生坐标", "勾上出生在(0,0)坐标")]
        public bool ZeroPos = false;

        public override KeyFrameArgs Clone()
        {
            return new EffectKeyFrameExportArgs()
            {
                Foldout         = Foldout,
                Operation       = Operation,
                BonePoint       = BonePoint,
                EffectName      = EffectName,
                LifeTime        = LifeTime,
                AutoDestroy     = AutoDestroy,
                BuildBone       = BuildBone,
                AutoScale       = AutoScale,
                ZeroPos         = ZeroPos
            };
        }
    }

    public class SoundKeyFrameExportArgs : KeyFrameArgs
    {
        [ArgsNote("音效路径", "以Resources为根")]
        public string SoundPath = "";
        [ArgsNote("音效名字", "命名")]
        public string SoundName = "";
        [ArgsNote("Loop", "是否循环")]
        public bool Loop = false;
        [ArgsNote("音效音量", "0 - 100")]
        public int Volume = 100;

        public override KeyFrameArgs Clone()
        {
            return new SoundKeyFrameExportArgs()
            {
                Foldout     = Foldout,
                Operation   = Operation,
                SoundPath   = SoundPath,
                SoundName   = SoundName,
                Loop        = Loop,
                Volume      = Volume,
            };
        }
    }

    public class ShakeKeyFrameExportArgs : KeyFrameArgs
    {
        [ArgsNote("震动时间", "")]
        public float Duration = 0.5f;
        [ArgsNote("X轴力度", "")]
        public float XForce = 0.5f;
        [ArgsNote("Y轴力度", "")]
        public float YForce = 0.5f;
        [ArgsNote("Z轴力度", "")]
        public float ZForce = 0.5f;

        public override KeyFrameArgs Clone()
        {
            return new ShakeKeyFrameExportArgs()
            {
                Foldout     = Foldout,
                Operation   = Operation,
                Duration    = Duration,
                XForce      = XForce,
                YForce      = YForce,
                ZForce      = ZForce,
            };
        }
    }

    public class LaunchKeyFrameExportArgs : KeyFrameArgs
    {
        [ArgsNote("发射物路径", "以Resources为根")]
        public string   LaunchPath = "";
        [ArgsNote("骨骼点的名字", "骨骼点上要放BonePoint脚本并设置")]
        public string   BonePoint = "";
        [ArgsNote("对应的Hit", "")]
        public int      HitCond = -1;

        [ArgsNote("作为结束标志", "勾选代表该发射物落地/命中后即通知技能结束")]
        public bool     AsFinish = false;
        [ArgsNote("是否更改发射物速度", "默认表示不更改")]
        public int      Speed = 30;
        [ArgsNote("发射物方向", "-1:默认,1:天空")]
        public int      LaunchDirection = -1;
        [ArgsNote("打击特效", "打击到目标点时播放")]
        public string   HitEffectName = "";
        [ArgsNote("发射物技能", "技能ID")]
        public int      SkillId = -1;


        public override KeyFrameArgs Clone()
        {
            return new LaunchKeyFrameExportArgs()
            {
                Foldout         = Foldout,
                Operation       = Operation,
                LaunchPath      = LaunchPath,
                BonePoint       = BonePoint,
                AsFinish        = AsFinish,
                HitCond         = HitCond,
                Speed           = Speed,
                LaunchDirection = LaunchDirection,
                HitEffectName   = HitEffectName,
                SkillId = SkillId
            };
        }
    }

    public class LineKeyFrameExportArgs : KeyFrameArgs
    {
        [ArgsNote("特效路径", "")]
        public string       Path = "";
        [ArgsNote("特效命名", "")]
        public string       Name = "";
        [ArgsNote("持续时间", "")]
        public float        Duration = -1;
        [ArgsNote("起点", "1:释放者 2:技能目标")]
        public int          Start = -1;
        [ArgsNote("起点绑点", "")]
        public string       StartBind = "";
        [ArgsNote("终点", "1:释放者 2:技能目标")]
        public int          End = -1;
        [ArgsNote("终点绑点", "")]
        public string       EndBind = "";

        public override KeyFrameArgs Clone()
        {
            return new LineKeyFrameExportArgs()
            {
                Foldout     = Foldout,
                Operation   = Operation,
                Path        = Path,
                Name        = Name,
                Duration    = Duration,
                Start       = Start,
                StartBind   = StartBind,
                End         = End,
                EndBind     = EndBind,
            };
        }
    }

    public class ComponentKeyFrameExportArgs : KeyFrameArgs
    {
        [ArgsNote("脚本名字", "预制体上的脚本")]
        public string           ComponentName = "";
        [ArgsNote("方法名", "...")]
        public string           FunName = "";

        public override KeyFrameArgs Clone()
        {
            return new ComponentKeyFrameExportArgs()
            {
                ComponentName   = ComponentName,
                FunName         = FunName
            };
        }
    }
}
