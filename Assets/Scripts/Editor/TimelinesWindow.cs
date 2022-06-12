using System;
using System.Collections.Generic;
using TimeLines;
using UnityEditor;
using UnityEngine;





/// <summary>
/// 1 显示[title - timeline]
/// 2 定义接口，和 Editor以外接口通信;能运行时控制游戏逻辑
/// </summary>
public class TimelinesWindow : EditorWindow
{
    private static TimelinesWindow  thisWindow = null;

    [MenuItem("动作编辑器/事件编辑")]
    public static void ShowTimelineWindow()
    {
        var window                  = GetWindow<TimelinesWindow>();
        window.Show();
    }

    [SerializeField]
    private TimeLineSequencer       currentSequence;
    public TimeLineSequencer        CurrentSequence
    {
        get { return currentSequence; }
        private set { currentSequence = value;}
    }

    [SerializeField]
    private TimelinesContent        contentRenderer;
    private TimelinesContent        ContentRenderer
    {
        get { return contentRenderer; }
        set { contentRenderer = value; }
    }

    private Rect TopBar
    {
        get;
        set;
    }


    private Rect BottomBar
    {
        get;
        set;
    }

    [SerializeField]
    private float PreviousTime
    {
        get;
        set;
    }

    private bool                    _IsPlayingAnimation = false;

    private void OnEnable()
    {
        thisWindow                  = this;
        hideFlags                   = HideFlags.HideAndDontSave;  
        if(ContentRenderer == null )
        {
            ContentRenderer         = ScriptableObject.CreateInstance<TimelinesContent>();
            ContentRenderer.SequenceWindow = this;
        }

        if(Selection.activeTransform == null )
        {
            return;
        }
        EditorApplication.update    -= SequenceUpdate;
        EditorApplication.update    += SequenceUpdate;

        TimeLineSequencer sequence   = Selection.activeTransform.GetComponent<TimeLineSequencer>();
        if(sequence != null )
        {
            CurrentSequence          = sequence;
            if(ContentRenderer != null )
            {
                ContentRenderer.OnSequenceChange(CurrentSequence);
            }
        }
    }

    private void OnDestroy()
    {
        thisWindow = null;
        if( CurrentSequence != null )
        {
            CurrentSequence.Stop();
        }

        EditorApplication.update    -= SequenceUpdate;
    }


    private void PlayOrPause()
    {
        
    }

    private void Stop()
    {
        
    }
    
    public void SetRunningTime(float newRunningTime)
    {
        
    }


    private void OnGUI()
    {
        if( !CurrentSequence )
        {
            ShowNotification(new GUIContent("Select a Sequence Or Create New One!!!"));
        }

        if( CurrentSequence && CurrentSequence.TimelineContainerCount < 1 )
        {
            ShowNotification(new GUIContent("Drag A Object To Sequence Or add a New Container"));
        }


        GUILayout.BeginVertical();
        DisplayTopBar();
        DisplayBottomBar();
        DisplayEdittableArea();
        GUILayout.EndVertical();

        ProcessHotkeys();
        if( UnityEngine.Event.current.type == EventType.DragUpdated )
        {
            DragAndDrop.visualMode          = DragAndDropVisualMode.Link;
            UnityEngine.Event.current.Use();
        }

        if( UnityEngine.Event.current.type == EventType.DragPerform )
        {
            foreach( UnityEngine.Object dragObject in DragAndDrop.objectReferences )
            {
                GameObject go               = dragObject as GameObject;
                //if( go != CurrentSequence.gameObject )
                {
                    DragAndDrop.AcceptDrag();
                }
            }
        }
    }


    private void ProcessHotkeys()
    {
        if (UnityEngine.Event.current.rawType == EventType.KeyDown && (UnityEngine.Event.current.keyCode == KeyCode.Backspace || UnityEngine.Event.current.keyCode == KeyCode.Delete))
        {
            if( contentRenderer.SelectedObjects.Count > 0 )
            {
                contentRenderer.DeleteSelection();
            }
            UnityEngine.Event.current.Use();
        }
    }

    private void DisplayTopBar()
    {
        float space     = 16.0f;
        GUILayout.Box("", EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(18.0f));
        if( UnityEngine.Event.current.type == EventType.Repaint )
        {
            TopBar      = GUILayoutUtility.GetLastRect();
        }


        GUILayout.BeginArea(TopBar);
        {
            GUILayout.BeginHorizontal();
            {
                if( GUILayout.Button("Create Sequence", EditorStyles.toolbarButton ))
                {
                    GameObject newSequence          = new GameObject("Sequence");
                    TimeLineSequencer sequencer     = newSequence.AddComponent<TimeLineSequencer>();
                    if( CurrentSequence == null )
                    {
                        Selection.activeGameObject  = newSequence;
                        Selection.activeTransform   = newSequence.transform;
                        CurrentSequence             = sequencer;
                        ContentRenderer.OnSequenceChange(CurrentSequence);
                        CurrentSequence.CreateNewTimelineContainer(newSequence.transform);
                    }

                    Repaint();
                }

                string label                        = "Select a Sequence";
                if( GUILayout.Button( label, EditorStyles.toolbarButton, GUILayout.Width( 150.0f )))
                {
                    GenericMenu menu                = new GenericMenu();
                    menu.ShowAsContext();
                }

                GUILayout.Space(space);
                GUILayout.Box("", USEditorUtility.SeperatorStyle, GUILayout.Height(18.0f));
                GUILayout.Space(space);

                if (CurrentSequence != null)
                {
                    if (GUILayout.Button(new GUIContent(!_IsPlayingAnimation ? USEditorUtility.PlayButton : USEditorUtility.PauseButton, "Toggle Play Mode (P)"), USEditorUtility.ToolbarButtonSmall))
                        PlayOrPause();
                    if (GUILayout.Button(USEditorUtility.StopButton, USEditorUtility.ToolbarButtonSmall))
                        Stop();

                    GUILayout.Space(space);
                    GUILayout.Box("", USEditorUtility.SeperatorStyle, GUILayout.Height(18.0f));
                    GUILayout.Space(space);
                }

                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    //EditorDataContainer.SaveSkillAssetData();
                }
                if (GUILayout.Button("Load", EditorStyles.toolbarButton))
                {
                    //EditorDataContainer.LoadSkillAssetData();
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }

    private void DisplayBottomBar()
    {
        float space         = 16.0f;
        GUILayout.Box("", EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(20.0f));
        if (UnityEngine.Event.current.type == EventType.Repaint)
            BottomBar       = GUILayoutUtility.GetLastRect();

        if (CurrentSequence == null)
            return;

        GUILayout.BeginArea(BottomBar);
        {
            GUILayout.BeginHorizontal();
            {
                string[] showAllOptions         = { "Show All", "Show Only Animated" };
                int selectedShowAll             = 0;
                selectedShowAll                 = EditorGUILayout.Popup( selectedShowAll, showAllOptions, EditorStyles.toolbarPopup, GUILayout.MaxWidth( 120.0f ));

                GUILayout.Space(space);
                GUILayout.Box("", USEditorUtility.SeperatorStyle, GUILayout.Height(18.0f));
                GUILayout.Space(space);

                EditorGUILayout.LabelField("", "Running Time", GUILayout.MaxWidth(100.0f));

                //开始检查是否有任何界面元素变化
                EditorGUI.BeginChangeCheck();
                float runningTime = EditorGUILayout.FloatField("", CurrentSequence.RunningTime, GUILayout.MaxWidth(50.0f));
                if (EditorGUI.EndChangeCheck())
                {
                    SetRunningTime(runningTime);
                }
                GUILayout.Space(space);
                GUILayout.Box("", USEditorUtility.SeperatorStyle, GUILayout.Height(18.0f));
                GUILayout.Space(space);

                EditorGUILayout.LabelField("", "Duration", GUILayout.MaxWidth(50.0f));
                EditorGUI.BeginChangeCheck();
                CurrentSequence.Duration = EditorGUILayout.FloatField("", CurrentSequence.Duration, GUILayout.MaxWidth(50.0f));
                if (EditorGUI.EndChangeCheck())
                {
                    ContentRenderer.UpdateCachedMarkerInformation();
                }

                GUILayout.Space(space);
                GUILayout.Box("", USEditorUtility.SeperatorStyle, GUILayout.Height(18.0f));
                GUILayout.Space(space);

                EditorGUILayout.LabelField("", "PlaybackRate", GUILayout.MaxWidth(80.0f));
                EditorGUI.BeginChangeCheck();
                float playbackRate = EditorGUILayout.FloatField("", CurrentSequence.PlaybackRate, GUILayout.MaxWidth(50.0f));
                if (EditorGUI.EndChangeCheck())
                {
                    CurrentSequence.PlaybackRate        = playbackRate;
                    ContentRenderer.UpdateCachedMarkerInformation();
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }

    private void DisplayEdittableArea()
    {
        if( CurrentSequence != null && contentRenderer != null )
        {
            contentRenderer.OnGUI();
        }
    }


    private void SequenceUpdate()
    {
        if( CurrentSequence != null )
        {
            float currentTime           = Time.realtimeSinceStartup;
            float deltaTime             = currentTime - PreviousTime;
            if (Mathf.Abs(deltaTime) > TimeLineSequencer.SequenceUpdateRate)
            {
                if (CurrentSequence.IsPlaying && !Application.isPlaying)
                {
                    Repaint();
                }
                PreviousTime = currentTime;
            }
        }

        TimeLineSequencer nextSequence = null;
        if (Selection.activeGameObject != null && (CurrentSequence == null || Selection.activeGameObject != CurrentSequence.gameObject))
        {
            nextSequence = Selection.activeGameObject.GetComponent<TimeLineSequencer>();
            if (nextSequence != null)
            {
                bool isPrefab = PrefabUtility.GetPrefabParent(nextSequence.gameObject) == null && PrefabUtility.GetPrefabObject(nextSequence.gameObject) != null;
                if (isPrefab)
                    nextSequence = null;
            }
        }
        else
        {
            return;
        }

        if (nextSequence == null) return;

        CurrentSequence         = nextSequence;
        ContentRenderer.OnSequenceChange(CurrentSequence);
    }
}
