using System;
using System.Collections.Generic;
using UnityEngine;






public class BuildTypeManager : MonoBehaviour
{
    public static BuildTypeManager Instance = null;
    public SpriteRenderer beginHalo;
    public SpriteRenderer endHalo;
    public LineRenderer line;
    public Transform SceneRoot;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
        SceneRoot = transform;
    }


    private List<BuildTypeBehaviour> mapList = new List<BuildTypeBehaviour>();
    public void InitSceneBuilds()
    {
        BuildTypeBehaviour[] builds = gameObject.GetComponentsInChildren<BuildTypeBehaviour>();
        if( builds == null )
        {
            Debug.LogError("场景配置错误了!!!");
            return;
        }

        foreach( var build in builds )
        {
            mapList.Add(build);
        }
    }


    public List<BuildTypeBehaviour> MapList
    {
        get
        {
            return mapList;
        }
    }
}
