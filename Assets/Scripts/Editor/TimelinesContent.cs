using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CySkillEditor;
using TimeLines;


[Serializable]
public class TimelinesContent : ScriptableObject
{
    private List<TimelineMarkerCachedData>                          cachedMarkerData = new List<TimelineMarkerCachedData>();

    /// <summary>
    ///  key 时间线 value 渲染元素数组 包含所有可见clip
    /// </summary>
    private Dictionary<UnityEngine.Object, List<JClipRenderData>>   timelineClipRenderDataMap = new Dictionary<UnityEngine.Object, List<JClipRenderData>>();

    private Dictionary<JClipRenderData, float>                      sourcePositions = new Dictionary<JClipRenderData, float>();
    private Dictionary<JClipRenderData, float>                      SourcePositions
    {
        get { return sourcePositions; }
        set { sourcePositions = value; }
    }

    private float                       totalPixelWidthOfTimeline = 1.0f;
    private float                       FloatingWidth = 100.0f;
    private float                       lineHeight = 18.0f;
    private bool                        extendingFloatingWidth = false;
    private float                       additionalFloatingWidth = 0.0f;
    private float                       baseFloatingWidth = 200.0f;


    private TimeLineSequencer           currentSequence;
    public TimeLineSequencer            CurrentSequence
    {
        get { return currentSequence; }
        set
        {
            currentSequence = value;
            SequenceWindow.Repaint();
        }
    }

    private TimelinesWindow             sequenceWindow;
    public TimelinesWindow              SequenceWindow
    {
        get { return sequenceWindow; }
        set { sequenceWindow = value; }
    }

    private Rect                        FloatingArea
    {
        get;
        set;
    }

    private Rect                        ScrubArea
    {
        get;
        set;
    }

    private Rect                        HierarchyArea
    {
        get;
        set;

    }
    private Rect                        VisibleArea
    {
        get;
        set;
    }

    private Rect                        TotalArea
    {
        get;
        set;
    }

    private Rect                        HorizontalScrollArea
    {
        get;
        set;
    }

    private Rect                        VerticalScrollArea
    {
        get;
        set;
    }

    [SerializeField]
    private Rect                        SelectionArea
    {
        get;
        set;
    }

    [SerializeField]
    private Rect                        DisplayArea
    {
        get;
        set;
    }

    [SerializeField]
    private bool                        IsDragging
    {
        get;
        set;
    }

    [SerializeField]
    private bool                        HasStartedDrag
    {
        get;
        set;
    }

    [SerializeField]
    private bool                        HasProcessedInitialDrag
    {
        get;
        set;
    }

    [SerializeField]
    private bool                        IsBoxSelecting
    {
        get;
        set;
    }

    [SerializeField]
    private bool                        IsDuplicating
    {
        get;
        set;
    }

    [SerializeField]
    private bool                        HasDuplicated
    {
        get;
        set;
    }

    [SerializeField]
    private Vector2                     DragStartPosition
    {
        get;
        set;
    }

    [SerializeField]
    public bool                         ScrubHandleDrag
    {
        get;
        private set;
    }

    private float                       XScale
    {
        get
        {
            return (totalPixelWidthOfTimeline / ScrubArea.width);
        }
    }

    private float                       XScroll
    {
        get
        {
            return ScrollInfo.currentScroll.x;
        }
    }

    private float                       YScroll
    {
        get
        {
            return ScrollInfo.currentScroll.y;
        }
    }



    [SerializeField]
    private JZoomInfo                   zoomInfo;
    private JZoomInfo                   ZoomInfo
    {
        get { return zoomInfo; }
        set { zoomInfo = value; }
    }

    [SerializeField]
    private JScrollInfo                 scrollInfo;
    private JScrollInfo                 ScrollInfo
    {
        get { return scrollInfo; }
        set { scrollInfo = value; }
    }

    private List<TimeLineType>          showTypeList = new List<TimeLineType> { TimeLineType.Animation };
    private int                         CountClip = 0;
    private bool                        hasObjectsUnderMouse = false;


    //目录fold字典
    private Dictionary<int, bool>       foldStateDict;
    public Dictionary<int, bool>        FoldStateDict
    {
        get
        {
            if (foldStateDict == null || foldStateDict.Keys.Count != CurrentSequence.TimelineContainerCount + CurrentSequence.TimelineContainerCount)
            {

                foldStateDict = new Dictionary<int, bool>();
                for (int i = 0; i < CurrentSequence.TimelineContainerCount; i++)
                {
                    foldStateDict.Add(i + 1, true);
                }
            }

            return foldStateDict;
        }
        set { foldStateDict = value; }
    }

    protected float ConvertTimeToXPos(float time)
    {
        float xPos = DisplayArea.width * (time / CurrentSequence.Duration);
        return DisplayArea.x + ((xPos * XScale) - XScroll);
    }

    private void OnEnable()
    {
        hideFlags                   = HideFlags.HideAndDontSave;
        if (ScrollInfo == null)
        {
            ScrollInfo              = ScriptableObject.CreateInstance<JScrollInfo>();
            ScrollInfo.Reset();
        }

        if (ZoomInfo == null)
        {
            ZoomInfo                = ScriptableObject.CreateInstance<JZoomInfo>();
            ZoomInfo.Reset();
        }

        if (currentSequence)
            UpdateCachedMarkerInformation();
    }

    private float TimeToContentX( float time )
    {
        return (time / ZoomInfo.meaningOfEveryMarker * ZoomInfo.currentXMarkerDist) - ScrollInfo.currentScroll.x;
    }

    private float ContentXToTime( float pos )
    {
        return (((pos + scrollInfo.currentScroll.x - ScrubArea.x) / ScrubArea.width) * CurrentSequence.Duration) / (totalPixelWidthOfTimeline / ScrubArea.width);
    }

    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 基本布局
    /// </summary>
    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    private void LayoutAreas()
    {
        GUILayout.BeginVertical();
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        //时间标尺背景
                        GUILayout.Box("Floating", USEditorUtility.ContentBackground, GUILayout.Width(200), GUILayout.Height(lineHeight));
                        if (UnityEngine.Event.current.type == EventType.Repaint)
                        {
                            if (FloatingArea != GUILayoutUtility.GetLastRect())
                            {
                                FloatingArea = GUILayoutUtility.GetLastRect();
                                SequenceWindow.Repaint();
                            }
                        }
                        //时间标尺
                        GUILayout.Box("Scrub", USEditorUtility.ContentBackground, GUILayout.ExpandWidth(true), GUILayout.Height(lineHeight));
                        if (UnityEngine.Event.current.type == EventType.Repaint)
                        {
                            if (ScrubArea != GUILayoutUtility.GetLastRect())
                            {
                                ScrubArea = GUILayoutUtility.GetLastRect();
                                SequenceWindow.Repaint();
                                UpdateCachedMarkerInformation();
                            }
                        }
                    }
                    GUILayout.EndHorizontal();

                    //整个内容显示区域
                    GUILayout.Box("Hierarchy", USEditorUtility.ContentBackground, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    if (UnityEngine.Event.current.type == EventType.Repaint)
                    {
                        if (HierarchyArea != GUILayoutUtility.GetLastRect())
                        {
                            HierarchyArea = GUILayoutUtility.GetLastRect();

                            SequenceWindow.Repaint();
                            UpdateCachedMarkerInformation();
                        }
                    }
                }
                //垂直滚动条
                GUILayout.EndVertical();

                GUILayout.Box("Scroll", USEditorUtility.ContentBackground, GUILayout.Width(lineHeight), GUILayout.ExpandHeight(true));
                if (UnityEngine.Event.current.type == EventType.Repaint)
                {
                    if (VerticalScrollArea != GUILayoutUtility.GetLastRect())
                    {
                        VerticalScrollArea      = GUILayoutUtility.GetLastRect();
                        SequenceWindow.Repaint();
                        UpdateCachedMarkerInformation();
                    }
                }
            }

            GUILayout.EndHorizontal();
            //水平滚动条
            GUILayout.BeginHorizontal();
            {
                GUILayout.Box("Scroll", USEditorUtility.ContentBackground, GUILayout.ExpandWidth(true), GUILayout.Height(lineHeight));
                if (UnityEngine.Event.current.type == EventType.Repaint)
                {
                    if (HorizontalScrollArea != GUILayoutUtility.GetLastRect())
                    {
                        HorizontalScrollArea = GUILayoutUtility.GetLastRect();
                        SequenceWindow.Repaint();
                        UpdateCachedMarkerInformation();
                    }
                }
                //右下角的横纵滚动条的边角
                GUILayout.Box("block bit", USEditorUtility.ContentBackground, GUILayout.Width(lineHeight), GUILayout.Height(lineHeight));
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 显示布局
    /// </summary>
    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    public void OnGUI()
    {
        LayoutAreas();
        if (CurrentSequence == null)
            return;


        ///标签区域、右边标尺区域
        GUI.Box(FloatingArea,   "", USEditorUtility.ContentBackground);
        GUI.Box(ScrubArea,      "", USEditorUtility.ScrubBarBackground);

        //绘制 时间标尺 当前时间刻度
        float widthOfContent    = ScrubArea.x + (CurrentSequence.Duration / ZoomInfo.meaningOfEveryMarker * ZoomInfo.currentXMarkerDist);
        GUILayout.BeginArea(ScrubArea);
        {
            foreach (var cachedMarker in cachedMarkerData)
            {
                GUI.DrawTexture(cachedMarker.MarkerRenderRect, USEditorUtility.TimelineMarker);
                if (cachedMarker.MarkerRenderLabel != string.Empty)
                    GUI.Label(cachedMarker.MarkerRenderLabelRect, cachedMarker.MarkerRenderLabel);
            }

            // Render our scrub Handle
            float currentScrubPosition  = TimeToContentX(CurrentSequence.RunningTime);
            float halfScrubHandleWidth  = 5.0f;
            Rect scrubHandleRect        = new Rect(currentScrubPosition - halfScrubHandleWidth, 0.0f, halfScrubHandleWidth * 2.0f, ScrubArea.height);
            GUI.color                   = new Color(1.0f, 0.1f, 0.1f, 0.65f);
            GUI.DrawTexture(scrubHandleRect, USEditorUtility.TimelineScrubHead);


            // Render the running time here
            GUI.color                   = Color.white;
            scrubHandleRect.x           += scrubHandleRect.width;
            scrubHandleRect.width       = 100.0f;
            GUI.Label(scrubHandleRect, CurrentSequence.RunningTime.ToString("#.####"));

            if (UnityEngine.Event.current.type == EventType.MouseDown)
                ScrubHandleDrag = true;
            if (UnityEngine.Event.current.rawType == EventType.MouseUp)
                ScrubHandleDrag = false;

            if (ScrubHandleDrag && UnityEngine.Event.current.isMouse)
            {
                float mousePosOnTimeline = ContentXToTime(FloatingWidth + UnityEngine.Event.current.mousePosition.x);
                sequenceWindow.SetRunningTime(mousePosOnTimeline);
                UnityEngine.Event.current.Use();
            }
        }
        GUILayout.EndArea();

        UpdateGrabHandle();

        ContentGUI();

        // Render our red line
        Rect scrubMarkerRect                = new Rect(ScrubArea.x + TimeToContentX(CurrentSequence.RunningTime), HierarchyArea.y, 1.0f, HierarchyArea.height);
        if (scrubMarkerRect.x < HierarchyArea.x)
            return;
        GUI.color                           = new Color(1.0f, 0.1f, 0.1f, 0.65f);
        GUI.DrawTexture(scrubMarkerRect, USEditorUtility.TimelineScrubTail);
        GUI.color                           = Color.white;
    }

    private void ContentGUI()
    {
        GUILayout.BeginArea(HierarchyArea);
        {
            GUILayout.BeginVertical();
            GUILayout.Box("", USEditorUtility.ContentBackground, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (UnityEngine.Event.current.type == EventType.Repaint)
            {
                if (VisibleArea != GUILayoutUtility.GetLastRect())
                {
                    VisibleArea = GUILayoutUtility.GetLastRect();
                    SequenceWindow.Repaint();
                }
            }

            GUILayout.BeginArea(VisibleArea);
            GUILayout.BeginScrollView(ScrollInfo.currentScroll, GUIStyle.none, GUIStyle.none);

            GUILayout.BeginVertical();
            DrawSideBarAndTimeLines();
            GUILayout.EndVertical();

            if (UnityEngine.Event.current.type == EventType.Repaint)
            {
                if (TotalArea != GUILayoutUtility.GetLastRect())
                {
                    TotalArea = GUILayoutUtility.GetLastRect();
                    SequenceWindow.Repaint();
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            //  SelectionArea = VisibleArea;
            if (VisibleArea.Contains(UnityEngine.Event.current.mousePosition) || UnityEngine.Event.current.rawType == EventType.MouseUp || UnityEngine.Event.current.rawType == EventType.MouseDrag)
                HandleEvent(UnityEngine.Event.current.rawType == EventType.MouseUp ? UnityEngine.Event.current.rawType : UnityEngine.Event.current.type, UnityEngine.Event.current.button, UnityEngine.Event.current.mousePosition);

            // Render our mouse drag box.
            if (IsBoxSelecting && HasStartedDrag)
            {
                Vector2 mousePosition       = UnityEngine.Event.current.mousePosition;
                Vector2 origin              = DragStartPosition;
                Vector2 destination         = mousePosition;
                if (mousePosition.x < DragStartPosition.x)
                {
                    origin.x                = mousePosition.x;
                    destination.x           = DragStartPosition.x;
                }

                if (mousePosition.y < DragStartPosition.y)
                {
                    origin.y                = mousePosition.y;
                    destination.y           = DragStartPosition.y;
                }

                Vector2 mouseDelta          = destination - origin;
                SelectionArea               = new Rect(origin.x, origin.y, mouseDelta.x, mouseDelta.y);
                if (!EditorGUIUtility.isProSkin)
                    GUI.Box(SelectionArea, "", USEditorUtility.USeqSkin.box);
                else
                    GUI.Box(SelectionArea, "");

                SequenceWindow.Repaint();
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    public void OnSequenceChange(TimeLineSequencer newSequence )
    {
        CurrentSequence             = newSequence;
        totalPixelWidthOfTimeline   = 1.0f;
        UpdateCachedMarkerInformation();
        SequenceWindow.Repaint();
    }

    /// --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 侧边栏渲染
    /// </summary>
    /// --------------------------------------------------------------------------------------------------------------------------
    private void DrawSideBarAndTimeLines()
    {
        CountClip = 0;
        foreach (KeyValuePair<UnityEngine.Object, List<JClipRenderData>> kvp in timelineClipRenderDataMap)
        {
            foreach (var clip in kvp.Value)
            {
                clip.index = CountClip++;
            }
        }

        TimelineContainer[] containers  = CurrentSequence.SortedTimelineContainers;
        for (int i = 0; i < containers.Length; i++)
        {
            GUILayout.BeginVertical();
            TimelineContainer container = containers[i];
            GUILayout.BeginHorizontal();
            GUILayout.Box(new GUIContent("", ""), USEditorUtility.USeqSkin.GetStyle("TimelinePaneBackground"), GUILayout.Height(lineHeight), GUILayout.MaxWidth(FloatingWidth));
            Rect FloatingRect = GUILayoutUtility.GetLastRect();
            GUILayout.Box(new GUIContent("", ""), USEditorUtility.USeqSkin.GetStyle("TimelinePaneBackground"), GUILayout.Height(lineHeight), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            FloatingRect.width -= lineHeight + 1;


            if (FoldStateDict.ContainsKey(i + 1))
            {
                Rect temp               = FloatingRect;
                temp.width              = 20;
                FoldStateDict[i + 1]    = EditorGUI.Foldout(temp, FoldStateDict[i + 1], "");
                Rect temp1              = FloatingRect;
                temp1.x                 += 20;
                temp1.width             -= 20;
                if (GUI.Button(temp1, new GUIContent(container.AffectedObject.name, "模型"), EditorStyles.toolbarButton))
                {
                    ResetSelection();
                    Selection.activeGameObject = container.gameObject;
                }

                Rect menuBtn            = FloatingRect;
                menuBtn.x               = menuBtn.x + menuBtn.width;
                menuBtn.width           = lineHeight;
                if (GUI.Button(menuBtn, new GUIContent("", USEditorUtility.DeleteButton, "Delete Timelines"), USEditorUtility.ToolbarButtonSmall))
                {
                    GameObject.DestroyImmediate(container.gameObject);
                }
            }


            {
                // TODO: 遍历操作
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Box(new GUIContent("", ""), USEditorUtility.USeqSkin.GetStyle("TimelinePaneBackground"), GUILayout.Height(lineHeight), GUILayout.MaxWidth(FloatingWidth));
                Rect FloatingRect1 = GUILayoutUtility.GetLastRect();
                GUILayout.Box(new GUIContent("", ""), USEditorUtility.USeqSkin.GetStyle("TimelinePaneBackground"), GUILayout.Height(lineHeight), GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                FloatingRect1.x         = +20;
                FloatingRect1.width     -= (20 + lineHeight + 1);

                Rect menuBtn            = FloatingRect1;
                menuBtn.x               = menuBtn.x + menuBtn.width + 1.0f;
                menuBtn.width           = lineHeight;
                if (GUI.Button(menuBtn, new GUIContent("", USEditorUtility.MoreButton, "Add Timeline"), USEditorUtility.ToolbarButtonSmall))
                {
                    TimelineBase line = container.AddNewTimeline(TimeLineType.Animation);
                    if (line != null)
                        AddNewAnimationTrack(line);
                }


                TimelineBase[] timelines = CurrentSequence.SortedTimelines;
                for (int k = 0; k < timelines.Length; k++)
                {
                    // TODO: 遍历操作
                    GUILayout.BeginVertical();
                    TimelineBase line = timelines[k];
                    if (line != null && line.TimelineContainer == container)
                    {
                        SideBarAndLineForAnimation(line);
                    }
                    GUILayout.EndVertical();
                }
            }
        }
        GUILayout.EndVertical();
    }


    private void UpdateGrabHandle()
    {
        FloatingWidth = additionalFloatingWidth + baseFloatingWidth;
        Rect resizeHandle = new Rect(FloatingWidth - 10.0f, ScrubArea.y, 10.0f, ScrubArea.height);
        GUILayout.BeginArea(resizeHandle, "", "box");

        if (UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.button == 0)
            extendingFloatingWidth = true;

        if (extendingFloatingWidth && UnityEngine.Event.current.type == EventType.MouseDrag)
        {
            additionalFloatingWidth += UnityEngine.Event.current.delta.x;
            FloatingWidth = additionalFloatingWidth + baseFloatingWidth;
            UpdateCachedMarkerInformation();
            SequenceWindow.Repaint();
        }
        GUILayout.EndArea();

        if (UnityEngine.Event.current.type == EventType.MouseUp)
            extendingFloatingWidth = false;
    }

    /// ------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 时间线的侧边栏绘制
    /// </summary>
    /// ------------------------------------------------------------------------------------------------------------------------------------------
    private void SideBarAndLineForAnimation(TimelineBase timeline)
    {
        if (timeline is TimelineAnimation)
        {
            GUILayout.BeginVertical();
            TimelineAnimation animationline = (TimelineAnimation)timeline;
            for (int j = 0; j < animationline.AnimationTracks.Count; j++)
            {
                GUILayout.BeginHorizontal();
                GUIStyle st                 = USEditorUtility.USeqSkin.GetStyle("TimelinePaneBackground");
                GUILayout.Box(new GUIContent("" + animationline.AffectedObject.name, ""), st, GUILayout.Height(lineHeight), GUILayout.MaxWidth(FloatingWidth));
                Rect FloatingRect           = GUILayoutUtility.GetLastRect();
                GUILayout.Box(new GUIContent("", "AnimationTimeline for" + animationline.AffectedObject.name + "Track" + j ), st, GUILayout.Height(lineHeight), GUILayout.ExpandWidth(true));
                Rect ContentRect            = GUILayoutUtility.GetLastRect();
                GUILayout.EndHorizontal();

                Rect addRect                = FloatingRect;
                Rect labelRect              = addRect;
                labelRect.x                 += 40;
                labelRect.width             -= (lineHeight + 41);
                GUI.Label(labelRect, "Track " + j);

                //轨道名字
                Rect nameRect               = addRect;
                nameRect.x                  += 40 + lineHeight + 40;
                nameRect.width              -= (lineHeight + 120);
                
                Rect enableRect             = addRect;
                enableRect.x                = addRect.x + addRect.width - 2 * lineHeight - 2.0f; ;
                enableRect.width            = lineHeight;
                animationline.AnimationTracks[j].Enable = GUI.Toggle(enableRect, animationline.AnimationTracks[j].Enable, new GUIContent("", USEditorUtility.EditButton, "Enable The Timeline"));

                addRect.x                   = addRect.x + addRect.width - lineHeight - 1.0f;
                addRect.width               = lineHeight;
                GenericMenu contextMenu     = new GenericMenu();
                if (GUI.Button(addRect, new GUIContent("", USEditorUtility.EditButton, "Options for this Timeline"), USEditorUtility.ToolbarButtonSmall))
                {
                    contextMenu             = MenuForAnimationTimeLine(animationline, animationline.AnimationTracks[j]);
                    contextMenu.ShowAsContext();
                }

                if (timelineClipRenderDataMap.ContainsKey(animationline.AnimationTracks[j]))
                {
                    ///时间线背景 区域 只接受右键
                    DisplayArea             = ContentRect;
                    GUI.BeginGroup(DisplayArea);
                    List<JClipRenderData> renderDataList = timelineClipRenderDataMap[animationline.AnimationTracks[j]];
                    AnimationGUI(animationline, animationline.AnimationTracks[j], renderDataList.ToArray());
                    GUI.EndGroup();

                }
            }
            GUILayout.EndVertical();
        }
    }


    /// ------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 时间线右键点击的菜单
    /// </summary>
    /// ------------------------------------------------------------------------------------------------------------------------------------------
    private GenericMenu MenuForAnimationTimeLine(TimelineAnimation Animationline, AnimationTrack linetrack)
    {
        GenericMenu contextMenu     = new GenericMenu();
        float newTime               = (((UnityEngine.Event.current.mousePosition.x + XScroll) / DisplayArea.width) * Animationline.Sequence.Duration) / XScale;

        foreach ( var effect in Enum.GetValues( typeof(EffectType)))
        {
            string name             = Enum.GetName(typeof(EffectType), (EffectType)effect);
            contextMenu.AddItem(new GUIContent("AddClip/" + name),
                                false, (obj) => AddNewTrackClip(((AnimationTrack)((object[])obj)[0]), ((EffectType)((object[])obj)[1]), ((float)((object[])obj)[2])),
                                new object[] { linetrack, (EffectType)effect, newTime });
        }

        //删除时间线
        contextMenu.AddItem(new GUIContent("DeleteLine"),
                            false, (obj) => RemoveTrackClip(((AnimationTrack)((object[])obj)[0])), new object[] { linetrack } );
        return contextMenu;
    }


    /// ------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 绘制动作片段UI
    /// </summary>
    /// ------------------------------------------------------------------------------------------------------------------------------------------
    private void AnimationGUI(TimelineAnimation Animationline, AnimationTrack linetrack, JClipRenderData[] renderDataList)
    {
        GenericMenu contextMenu     = new GenericMenu();
        ///event 右键点击
        bool isContext              = UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.button == 1;
        bool isChoose               = UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.button == 0 && UnityEngine.Event.current.clickCount == 1;
        bool hasBox                 = false;
        Rect DisplayAreaTemp        = DisplayArea;
        DisplayAreaTemp.x           = 0;
        DisplayAreaTemp.y           = 0;
        for (int j = 0; j < renderDataList.Length; j++)
        {
            JClipRenderData renderdata = renderDataList[j];
            KeyFrameClipData animationClipData = (KeyFrameClipData)renderdata.ClipData;
            AnimationTrack track = animationClipData.Track;
            if (linetrack != track)
            {
                continue;
            }
            var startX              = ConvertTimeToXPos(animationClipData.StartTime);
            var endX                = ConvertTimeToXPos(animationClipData.StartTime + animationClipData.PlaybackDuration);
            var transitionX         = ConvertTimeToXPos(animationClipData.StartTime + 0.2f);
            var handleWidth         = 2.0f;

            Rect renderRect         = new Rect(startX, DisplayArea.y, endX - startX, DisplayArea.height);
            Rect transitionRect     = new Rect(startX, DisplayArea.y, transitionX - startX, DisplayArea.height);
            Rect leftHandle         = new Rect(startX, DisplayArea.y, handleWidth * 2.0f, DisplayArea.height);
            Rect rightHandle        = new Rect(endX - (handleWidth * 2.0f), DisplayArea.y, handleWidth * 2.0f, DisplayArea.height);
            Rect labelRect          = new Rect();

            Rect renderRecttemp     = renderRect;
            renderRecttemp.x        -= DisplayArea.x;
            renderRecttemp.y        = 0;
            Rect transitionRecttemp = transitionRect;
            transitionRecttemp.y    = 0;
            transitionRecttemp.x    -= DisplayArea.x;
            Rect leftHandletemp     = leftHandle;
            leftHandletemp.y        = 0;
            leftHandletemp.x        -= DisplayArea.x;
            Rect rightHandletemp    = rightHandle;
            rightHandletemp.x       -= DisplayArea.x;
            rightHandletemp.y = 0;

            //GUI.color = new Color(70 / 255.0f, 147 / 255.0f, 236 / 255.0f, 1);
            GUI.color = ColorTools.GetGrandientColor((float)renderdata.index / (float)CountClip);
            if (SelectedObjects.Contains(renderdata))
            {
                GUI.color = ColorTools.SelectColor;// Color.yellow;
            }

            GUI.Box(renderRecttemp, "", USEditorUtility.NormalWhiteOutLineBG);
            GUI.Box(leftHandletemp, "");
            GUI.Box(rightHandletemp, "");

            labelRect                   = renderRecttemp;
            labelRect.width             = DisplayArea.width;

            renderdata.renderRect       = renderRect;
            renderdata.labelRect        = renderRect;
            renderdata.renderPosition   = new Vector2(startX, DisplayArea.y);
            renderdata.transitionRect   = transitionRect;
            renderdata.leftHandle       = leftHandle;
            renderdata.rightHandle      = rightHandle;
            renderdata.ClipData         = animationClipData;
            

            GUI.color                   = Color.black;
            labelRect.x                 += 4.0f;
            GUI.Label(labelRect, "" );

            GUI.color = Color.white;
            if( isContext && renderRecttemp.Contains( UnityEngine.Event.current.mousePosition ))
            {
                hasBox = true;
                contextMenu.AddItem(new GUIContent("DeleteClip"), false, (obj) => RemoveAnimationClip(((JClipRenderData)((object[])obj)[0])), new object[] { renderdata });
            }

            if (isContext && renderRecttemp.Contains(UnityEngine.Event.current.mousePosition))
            {
                UnityEngine.Event.current.Use();
                contextMenu.ShowAsContext();
            }
        }

        if (!hasBox && isChoose && DisplayAreaTemp.Contains(UnityEngine.Event.current.mousePosition) && (UnityEngine.Event.current.control || UnityEngine.Event.current.command))
        {
            Selection.activeGameObject = Animationline.gameObject;
            EditorGUIUtility.PingObject(Animationline.gameObject);

        }
        if (!hasBox && isContext && DisplayAreaTemp.Contains(UnityEngine.Event.current.mousePosition))
        {
            contextMenu = MenuForAnimationTimeLine(Animationline, linetrack);

        }
        if (!hasBox && isContext && DisplayAreaTemp.Contains(UnityEngine.Event.current.mousePosition))
        {
            UnityEngine.Event.current.Use();
            contextMenu.ShowAsContext();
        }
    }


    public void UpdateCachedMarkerInformation()
    {
        cachedMarkerData.Clear();

        // Update the area
        float currentZoomUpper          = Mathf.Ceil(ZoomInfo.currentZoom);
        float currentZoomlower          = Mathf.Floor(ZoomInfo.currentZoom);
        float zoomRatio                 = (ZoomInfo.currentZoom - currentZoomlower) / (currentZoomUpper / currentZoomUpper);

        float maxDuration               = Mathf.Min(CurrentSequence.Duration, 600);
        float minXMarkerDist            = ScrubArea.width / maxDuration;
        float maxXMarkerDist            = minXMarkerDist * 2.0f;
        ZoomInfo.currentXMarkerDist     = maxXMarkerDist * zoomRatio + minXMarkerDist * (1.0f - zoomRatio);

        float minXSmallMarkerHeight     = ScrubArea.height * 0.1f;
        float maxXSmallMarkerHeight     = ScrubArea.height * 0.8f;
        float currentXSmallMarkerHeight = maxXSmallMarkerHeight * zoomRatio + minXSmallMarkerHeight * (1.0f - zoomRatio);

        // Calculate our maximum zoom out.
        float maxNumberOfMarkersInPane = ScrubArea.width / minXMarkerDist;
        ZoomInfo.meaningOfEveryMarker = Mathf.Ceil(CurrentSequence.Duration / maxNumberOfMarkersInPane);

        int levelsDeep = Mathf.FloorToInt(ZoomInfo.currentZoom);
        while (levelsDeep > 1)
        {
            ZoomInfo.meaningOfEveryMarker *= 0.5f;
            levelsDeep -= 1;
        }

        totalPixelWidthOfTimeline = ZoomInfo.currentXMarkerDist * (CurrentSequence.Duration / ZoomInfo.meaningOfEveryMarker);

        // Calculate how much we can see, for our horizontal scroll bar, this is for clamping.
        ScrollInfo.visibleScroll.x = (ScrubArea.width / ZoomInfo.currentXMarkerDist) * ZoomInfo.currentXMarkerDist;
        if (ScrollInfo.visibleScroll.x > totalPixelWidthOfTimeline)
            ScrollInfo.visibleScroll.x = totalPixelWidthOfTimeline;

        // Create our markers
        //TimeToContentX(CurrentSequence.RunningTime)
        float markerValue = 0.0f;

        Rect markerRect = new Rect(-ScrollInfo.currentScroll.x, 0.0f, 1.0f, maxXSmallMarkerHeight);
        while (markerRect.x < ScrubArea.width)
        {
            if (markerValue > CurrentSequence.Duration)
                break;

            // Big marker
            cachedMarkerData.Add(new TimelineMarkerCachedData());

            cachedMarkerData[cachedMarkerData.Count - 1].MarkerRenderRect = markerRect;
            cachedMarkerData[cachedMarkerData.Count - 1].MarkerRenderLabelRect = new Rect(markerRect.x + 2.0f, markerRect.y, 40.0f, ScrubArea.height);
            cachedMarkerData[cachedMarkerData.Count - 1].MarkerRenderLabel = markerValue.ToString();

            // Small marker
            for (int n = 1; n <= 10; n++)
            {
                float xSmallPos = markerRect.x + ZoomInfo.currentXMarkerDist / 10.0f * n;

                Rect smallMarkerRect = markerRect;
                smallMarkerRect.x = xSmallPos;
                smallMarkerRect.height = minXSmallMarkerHeight;

                if (n == 5)
                    smallMarkerRect.height = currentXSmallMarkerHeight;

                cachedMarkerData.Add(new TimelineMarkerCachedData());
                cachedMarkerData[cachedMarkerData.Count - 1].MarkerRenderRect = smallMarkerRect;
            }

            markerRect.x += ZoomInfo.currentXMarkerDist;
            markerValue += ZoomInfo.meaningOfEveryMarker;
        }
    }

    /// ------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 事件处理接口
    /// </summary>
    /// ------------------------------------------------------------------------------------------------------------------------------------
    public void HandleEvent(EventType eventType, int button, Vector2 mousePosition)
    {
        hasObjectsUnderMouse = false;
        List<UnityEngine.Object> allObjectsUnderMouse = new List<UnityEngine.Object>();
        foreach (KeyValuePair<UnityEngine.Object, List<JClipRenderData>> kvp in timelineClipRenderDataMap)
        {
            foreach (JClipRenderData renderclip in kvp.Value)
            {
                if (IsBoxSelecting && HasStartedDrag)
                {
                    Rect temp = SelectionArea;
                    temp.y += ScrollInfo.currentScroll.y;
                    if (USEditorUtility.DoRectsOverlap(temp, renderclip.renderRect))
                    {
                        allObjectsUnderMouse.Add(renderclip);
                    }
                }
                else
                {
                    if (renderclip.renderRect.Contains(mousePosition))
                        allObjectsUnderMouse.Add(renderclip);
                }
            }
        }
        if (allObjectsUnderMouse.Count > 0)
        {
            hasObjectsUnderMouse = true;
        }

        switch (eventType)
        {
            case EventType.MouseDown:
                {
                    HasProcessedInitialDrag = false;
                    IsDragging = false;
                    IsBoxSelecting = false;
                    DragStartPosition = mousePosition;

                    if (!hasObjectsUnderMouse && UnityEngine.Event.current.button == 0)
                        IsBoxSelecting = true;
                    if (hasObjectsUnderMouse && UnityEngine.Event.current.button == 0)
                        IsDragging = true;
                    if (IsDragging && UnityEngine.Event.current.alt && UnityEngine.Event.current.control)
                        IsDuplicating = true;

                    // if we have no objects under our mouse, then we are likely trying to clear our selection
                    if (!hasObjectsUnderMouse && (!UnityEngine.Event.current.control && !UnityEngine.Event.current.command))
                    {
                        ResetSelection();
                    }

                    if (!UnityEngine.Event.current.control && !UnityEngine.Event.current.command)
                    {
                        Selection.activeGameObject = null;
                        Selection.activeObject = null;
                        Selection.activeTransform = null;
                        Selection.objects = new UnityEngine.Object[] { };
                    }

                    HasStartedDrag = false;
                    SequenceWindow.Repaint();
                }
                break;
            case EventType.MouseDrag:
                {
                    if (!HasStartedDrag)
                        HasStartedDrag = true;

                    SequenceWindow.Repaint();
                }
                break;
            case EventType.MouseUp:
                {
                    HasProcessedInitialDrag = false;
                    IsBoxSelecting = false;
                    IsDragging = false;
                    IsDuplicating = false;
                    HasDuplicated = false;
                    SequenceWindow.Repaint();
                }
                break;
        }

        //单选
        if ((!UnityEngine.Event.current.control && !UnityEngine.Event.current.command) && hasObjectsUnderMouse && !HasStartedDrag && ((eventType == EventType.MouseUp && button == 0) || (eventType == EventType.MouseDown && button == 1)))
        {

            EditorGUI.FocusTextInControl("");
           
        }
        else
        //多选 添加和删除
        if ((UnityEngine.Event.current.control || UnityEngine.Event.current.command) && hasObjectsUnderMouse && !HasStartedDrag && eventType == EventType.MouseUp)
        {
            foreach (var selectedObject in allObjectsUnderMouse)
            {
                if (!SelectedObjects.Contains(selectedObject))
                    OnSelectedObjects(new List<UnityEngine.Object> { selectedObject });
                else
                    OnDeSelectedObjects(new List<UnityEngine.Object> { selectedObject });
            }
        }
        else if (IsBoxSelecting && HasStartedDrag)
        {
            OnSelectedObjects(allObjectsUnderMouse);
        }
    }


    private List<UnityEngine.Object>        selectedObjects = new List<UnityEngine.Object>();
    public List<UnityEngine.Object>         SelectedObjects
    {
        get { return selectedObjects; }
        set { selectedObjects = value; }
    }

    public void ResetSelection()
    {
        if (SelectedObjects != null && SelectedObjects.Count > 0)
        {
            SelectedObjects.Clear();
            SourcePositions.Clear();
        }
    }

    public void OnSelectedObjects(List<UnityEngine.Object> selectedObjects)
    {
        foreach (var selectedObject in selectedObjects)
        {
            if (!SelectedObjects.Contains(selectedObject))
            {
                SelectedObjects.Add(selectedObject);
                var selection = Selection.objects != null ? Selection.objects.ToList() : new List<UnityEngine.Object>();
                selection.Add(selectedObject);
                Selection.objects = selection.ToArray();
            }
        }
    }

    public void OnDeSelectedObjects(List<UnityEngine.Object> selectedObjects)
    {
        foreach (var selectedObject in selectedObjects)
        {
            SelectedObjects.Remove(selectedObject);
            var selection = Selection.objects != null ? Selection.objects.ToList() : new List<UnityEngine.Object>();
            selection.Remove(selectedObject);
            Selection.objects = selection.ToArray();
        }
    }

    public void DeleteSelection()
    {
        SelectedObjects.Clear();
    }


    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 移除时间线
    /// </summary>
    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    private void AddNewAnimationTrack(TimelineBase line)
    {
        if (line is TimelineAnimation)
        {
            TimelineAnimation tline = (TimelineAnimation)line;
            var track               = ScriptableObject.CreateInstance<AnimationTrack>();
            tline.AddTrack(track);
            AddRenderDataForAnimation(tline);
        }
    }

    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 移除时间线
    /// </summary>
    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    private void RemoveAnimationTimeline(TimelineAnimation line, AnimationTrack track)
    {
        if (timelineClipRenderDataMap.ContainsKey(track))
        {
            timelineClipRenderDataMap.Remove(track);
        }

        line.RemoveTrack(track);
        TimelineContainer contain = line.TimelineContainer;
        if (line.AnimationTracks.Count == 0)
            DestroyImmediate(line.gameObject);
        //删除的是最后一个 删除掉container
        if (contain.transform.childCount == 0)
        {
            DestroyImmediate(contain.gameObject);
        }
    }


    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 增加节点数据
    /// </summary>
    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    private void AddNewTrackClip(AnimationTrack track, EffectType type, float time )
    {
        var clipData                = ScriptableObject.CreateInstance<KeyFrameClipData>();
        TimelineAnimation line      = (TimelineAnimation)track.TimeLine;
        clipData.TargetObject       = line.AffectedObject.gameObject;
        clipData.StartTime          = time;
        clipData.PlaybackDuration   = 1;
        clipData.Track              = track;
        track.AddClip(clipData);

        if ( timelineClipRenderDataMap.ContainsKey(track))
        {
            var cachedData          = ScriptableObject.CreateInstance<JClipRenderData>();
            cachedData.ClipData     = null;
            timelineClipRenderDataMap[track].Add(cachedData);
        }
        else
        {
            var cachedData          = ScriptableObject.CreateInstance<JClipRenderData>();
            cachedData.ClipData     = null;
            List<JClipRenderData> list = new List<JClipRenderData>();
            list.Add(cachedData);
            timelineClipRenderDataMap.Add(track, list);
        }
    }


    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 删除节点数据
    /// </summary>
    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    private void RemoveTrackClip( AnimationTrack track )
    {
        if( timelineClipRenderDataMap.ContainsKey( track ))
        {
            timelineClipRenderDataMap.Remove(track);
        }

        TimelineAnimation line = (TimelineAnimation)track.TimeLine;
        line.RemoveTrack(track);

        
        if( line.AnimationTracks.Count == 0 )
        {
            DestroyImmediate(line.gameObject);
        }

        TimelineContainer container = line.TimelineContainer;
        if( container.Timelines.Length == 0 )
        {
            DestroyImmediate(container.gameObject);
        }
    }

    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 为时间线添加绘制片段数据
    /// </summary>
    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    private void AddRenderDataForAnimation(TimelineBase timeline)
    {
        if ( timeline is TimelineAnimation)
        {
            TimelineAnimation animationline     = (TimelineAnimation)timeline;
            List<JClipRenderData> list          = new List<JClipRenderData>();
            for (int k = 0; k < animationline.AnimationTracks.Count; k++)
            {
                AnimationTrack track            = animationline.AnimationTracks[k];
                for (int l = 0; l < track.TrackClips.Count; l++)
                {
                    KeyFrameClipData animationClipData = track.TrackClips[l];
                    var cachedData              = ScriptableObject.CreateInstance<JClipRenderData>();
                    cachedData.ClipData         = animationClipData;
                    list.Add(cachedData);
                }
                if (!timelineClipRenderDataMap.ContainsKey(track))
                {
                    timelineClipRenderDataMap.Add(track, list);
                }
            }
        }
    }


    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 添加动作片段
    /// </summary>
    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    private void AddNewAnimationState(TimelineAnimation line, AnimationTrack track, float time, string stateName )
    {
        var clipData            = ScriptableObject.CreateInstance<KeyFrameClipData>();
        clipData.TargetObject   = line.AffectedObject.gameObject;
        clipData.StartTime      = time;
        clipData.PlaybackDuration = 0.2f;
        clipData.Track          = track;
        track.AddClip(clipData);
        if( timelineClipRenderDataMap.ContainsKey(track))
        {
            var cacheData       = ScriptableObject.CreateInstance<JClipRenderData>();
            cacheData.ClipData  = clipData;
            timelineClipRenderDataMap[track].Add(cacheData);
        }
        else
        {
            var cacheData       = ScriptableObject.CreateInstance<JClipRenderData>();
            cacheData.ClipData  = clipData;
            List<JClipRenderData> list = new List<JClipRenderData>();
            list.Add(cacheData);
            timelineClipRenderDataMap.Add(track, list);
        }
    }


    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 移除一个动作片段
    /// </summary>
    /// -------------------------------------------------------------------------------------------------------------------------------------------------
    private void RemoveAnimationClip( JClipRenderData clip )
    {
        if( clip.ClipData is KeyFrameClipData )
        {
            KeyFrameClipData anidata = (KeyFrameClipData)clip.ClipData;
            if (timelineClipRenderDataMap.ContainsKey(anidata.Track))
                if (timelineClipRenderDataMap[anidata.Track].Contains(clip))
                    timelineClipRenderDataMap[anidata.Track].Remove(clip);
            anidata.Track.RemoveClip(anidata);
        }
    }
        
}
