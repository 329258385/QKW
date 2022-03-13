using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;





[Serializable]
[CanEditMultipleObjects]
public class JClipRenderData : ScriptableObject
{
    [SerializeField]
    public Rect             renderRect;
    [SerializeField]
    public Rect             labelRect;
    [SerializeField]
    public Rect             transitionRect;
    [SerializeField]
    public Rect             leftHandle;
    [SerializeField]
    public Rect             rightHandle;
    [SerializeField]
    public Vector2          renderPosition;
    [SerializeField]
    public ScriptableObject ClipData;
    [SerializeField]
    public int              index;
}


[Serializable]
public class JZoomInfo : ScriptableObject
{
    // our default zoom level, 1, this is an arbitrary unit.
    [SerializeField]
    public float            currentZoom = 1.0f;
    [SerializeField]
    public float            meaningOfEveryMarker = 0.0f;
    [SerializeField]
    public float            currentXMarkerDist = 0.0f;

    private void OnEnable() { hideFlags = HideFlags.HideAndDontSave; }

    public void Reset()
    {
        currentZoom          = 1.0f;
        meaningOfEveryMarker = 0.0f;
        currentXMarkerDist   = 0.0f;
    }
}


[Serializable]
public class JScrollInfo : ScriptableObject
{
    [SerializeField]
    public Vector2 currentScroll = Vector2.zero;
    [SerializeField]
    public Vector2 visibleScroll = Vector2.one;

    private void OnEnable() { hideFlags = HideFlags.HideAndDontSave; }

    public void Reset()
    {
        currentScroll = Vector2.zero;
        visibleScroll = Vector2.one;
    }
}
