using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace TimeLines
{
    
    public class TimeLine
    {
        public enum Type
        {
            Launch,
            Sound,
            Effect,
            Action,
            Shake
        }

        public ActionArgs       Args;
        public bool             End = false;
        public List<KeyFrame>   _keyFrames;
        public Type             LineType { private set; get; }


        public TimeLine( Type lineType, ActionArgs args )
        {
            LineType            = lineType;
            _keyFrames          = new List<KeyFrame>();
            Args                = args;
        }

        public void Tick( float duration )
        {
            if (End) return;
            for( int i = 0; i < _keyFrames.Count; i++ )
            {
                _keyFrames[i].Tick(duration);
            }

            if( _keyFrames.Count > 0 )
            {
                if (duration > _keyFrames[_keyFrames.Count - 1].Time)
                    End = true;
            }
            else
            {
                End = true;
            }
        }

        public void Stop()
        {
            for (int i = 0; i < _keyFrames.Count; i++)
            {
                _keyFrames[i].Over();
            }
        }

        public void AddKeyFrames(KeyFrame frame)
        {
            _keyFrames.Add(frame);
            _keyFrames = _keyFrames.OrderBy(o => o.Time).ToList();
        }

        public void BuildKeyFrame(XmlNode node, TimeLine line, TimeLine.Type tType)
        {
            var frames = node.SelectNodes("Frame");
            if (frames != null)
            {
                for (int i = 0; i < frames.Count; i++)
                {
                    var frame = frames[i];
                    if (frame.Attributes != null)
                    {
                        var point = float.Parse(frame.Attributes["Point"].Value);
                        KeyFrame keyFrame;
                        switch (tType)
                        {
                            case Type.Sound:
                                keyFrame = new SoundKeyFrame(point, Args);
                                break;
                            case Type.Effect:
                                keyFrame = new EffectKeyFrame(point, Args);
                                break;
                            case Type.Action:
                                keyFrame = new ActionKeyFrame(point, Args);
                                break;
                            case Type.Shake:
                                keyFrame = new ShakeKeyFrame(point, Args);
                                break;
                            default:
                                keyFrame = null;
                                break;
                        }
                        if (keyFrame != null)
                        {
                            keyFrame.BuildActions(tType, frame);
                            line.AddKeyFrames(keyFrame);
                        }
                    }
                }
            }

        }
    }
}
