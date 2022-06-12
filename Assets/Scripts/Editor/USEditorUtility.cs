using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;




public static class USEditorUtility
{
    [NonSerialized]
    static private GUISkin uSeqSkin = null;
    static public GUISkin USeqSkin
    {
        get
        {
            if (!uSeqSkin)
            {
                string Skin     = "USequencerProSkin";
                if (!EditorGUIUtility.isProSkin)
                    Skin        = "USequencerFreeSkin";

                uSeqSkin        = Resources.Load(Skin, typeof(GUISkin)) as GUISkin;
            }

            if (!uSeqSkin)
                Debug.LogError("Couldn't find the uSequencer Skin, it is possible it has been moved from the resources folder");

            return uSeqSkin;
        }
    }

    private static Texture              playButton;
    public static Texture PlayButton
    {
        get
        {
            if (playButton == null)
                playButton      = LoadTexture("Play Button");
            return playButton;
        }

        set
        {
            ;
        }
    }

    private static Texture              pauseButton;
    public static Texture PauseButton
    {
        get
        {
            if( pauseButton == null )
                pauseButton     = LoadTexture("Pause Button");
            return pauseButton;
        }

        set
        {
            ;
        }
    }

    private static Texture stopButton;
    public  static Texture StopButton
    {
        get
        {
            if( stopButton == null )
                stopButton      = LoadTexture("Stop Button");
            return stopButton;
        }
        set
        {
            ;
        }
    }

    private static Texture editButton;
    public static Texture EditButton
    {
        get
        {
            if (editButton == null)
                editButton = LoadTexture("EditButton");
            return editButton;
        }
        set {; }
    }

    private static Texture deleteButton;
    public static Texture DeleteButton
    {
        get
        {
            if (deleteButton == null)
                deleteButton = LoadTexture("Delete Button") as Texture;
            return deleteButton;
        }
        set {; }
    }

    private static Texture timelineMarker;
    public static Texture TimelineMarker
    {
        get
        {
            if (timelineMarker == null)
                timelineMarker = Resources.Load("TimelineMarker") as Texture;
            return timelineMarker;
        }
        set {; }
    }


    private static Texture timelineScrubHead;
    public static Texture TimelineScrubHead
    {
        get
        {
            if (timelineScrubHead == null)
                timelineScrubHead = Resources.Load("TimelineScrubHead") as Texture;
            return timelineScrubHead;
        }
        set {; }
    }

    private static Texture timelineScrubTail;
    public static Texture TimelineScrubTail
    {
        get
        {
            if (timelineScrubTail == null)
                timelineScrubTail = Resources.Load("TimelineScrubTail") as Texture;
            return timelineScrubTail;
        }
        set {; }
    }

    private static Texture moreButton;
    public static Texture MoreButton
    {
        get
        {
            if (moreButton == null)
                moreButton = LoadTexture("More Button") as Texture;
            return moreButton;
        }
        set {; }
    }

    private static GUIStyle normalWhiteOutLineBG;
    public static GUIStyle NormalWhiteOutLineBG
    {
        get
        {
            if (normalWhiteOutLineBG == null)
                normalWhiteOutLineBG = USeqSkin.GetStyle("NormalWhiteOutLineBG");
            return normalWhiteOutLineBG;
        }
        set {; }
    }

    private static GUIStyle         toolbarButtonSmall;
    public static GUIStyle ToolbarButtonSmall
    {
        get
        {
            if (toolbarButtonSmall == null)
                toolbarButtonSmall  = USeqSkin.GetStyle("ToolbarButtonSmall");
            return toolbarButtonSmall;
        }
        set {; }
    }

    private static GUIStyle seperatorStyle;
    public static GUIStyle SeperatorStyle
    {
        get
        {
            if (seperatorStyle == null)
                seperatorStyle      = USEditorUtility.USeqSkin.GetStyle("Seperator");
            return seperatorStyle;
        }
        set {; }
    }

    private static GUIStyle contentBackground;
    public static GUIStyle ContentBackground
    {
        get
        {
            if (contentBackground == null)
                contentBackground = USEditorUtility.USeqSkin.GetStyle("USContentBackground");
            return contentBackground;
        }
        set {; }
    }

    private static GUIStyle scrubBarBackground;
    public static GUIStyle ScrubBarBackground
    {
        get
        {
            if (scrubBarBackground == null)
                scrubBarBackground = USeqSkin.GetStyle("USScrubBarBackground");
            return scrubBarBackground;
        }
        set {; }
    }

    static public Texture LoadTexture(string textureName)
    {
        string directoryName        = EditorGUIUtility.isProSkin ? "_ProElements" : "_FreeElements";
        string fullFilename         = String.Format("{0}/{1}", directoryName, textureName);
        return Resources.Load(fullFilename) as Texture;
    }

    public static bool DoRectsOverlap(Rect RectA, Rect RectB)
    {
        return RectA.xMin < RectB.xMax && RectA.xMax > RectB.xMin &&
            RectA.yMin < RectB.yMax && RectA.yMax > RectB.yMin;
    }
}
