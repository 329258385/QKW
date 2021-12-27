using System;
using System.Collections.Generic;
using UnityEngine;


public class LoadingManager : MonoBehaviour
{
    public static LoadingManager        Instance;

    /// <summary>
    /// 加载方法被包成一个匿名方法保存在列表中
    /// </summary>
    public enum LoadingItem
    {
        SceneLoad,
        UI,
        None
    }

    public struct LoadingItemInfo
    {
        public int      nTotalNum;  //该节点一共多少个单元
        public float    fPercent;   // 该节点在整个LOADING过程中占多少比例

        public LoadingItemInfo(int total, float percent)
        {
            nTotalNum   = total;
            fPercent    = percent;
        }
    }

    public struct ActionInfo
    {
        public Action       action;
        public LoadingItem  loadingItemType;
    }

    Queue<ActionInfo>       funcQueue = new Queue<ActionInfo>();
    Dictionary<LoadingItem, LoadingItemInfo> LoadingItemInfoDic = new Dictionary<LoadingItem, LoadingItemInfo>();

    private float           _lastProgress;
    private float           _progress;
    private float           _targetProgress;
    private Coroutine       loadScence = null;
    private AsyncOperation  loadAsync = null;

    public  bool            _bSceneLoaded;
    public  bool            _bEnd;


    public float Progress
    {
        get { return _progress;  }
    }

    private void Awake()
    {
        Instance            = this;
        _progress           = 0.0f;
        _lastProgress       = 0.0f;
        _bSceneLoaded       = false;
        _bEnd               = false;
    }

    public void ResetShowForeground()
    {
        _progress           = 0.0f;
        _lastProgress       = 0.0f;
        _bSceneLoaded       = false;
        _bEnd               = false;
    }

    public void AddAction(ActionInfo actionInfo)
    {
        funcQueue.Enqueue(actionInfo);
        if (LoadingItemInfoDic.ContainsKey(actionInfo.loadingItemType))
        {
            LoadingItemInfo info;
            LoadingItemInfoDic.TryGetValue(actionInfo.loadingItemType, out info);
            info.nTotalNum++;
            LoadingItemInfoDic[actionInfo.loadingItemType] = info;
        }
        else
        {
            LoadingItemInfoDic.Add(actionInfo.loadingItemType, new LoadingItemInfo());
        }
    }

    void Update()
    {
        UpdateProcess();
        if (_bSceneLoaded)
        {
            if (funcQueue.Count > 0)
            {
                ActionInfo action = funcQueue.Dequeue();
                action.action.Invoke();
                LoadingItemInfo info;
                if (LoadingItemInfoDic.TryGetValue(action.loadingItemType, out info))
                {
                    _lastProgress += (1.0f / (float)info.nTotalNum) * info.fPercent;
                    _progress = _lastProgress;
                }
                else
                {
                    UnityEngine.Debug.LogError("Miss loadingItemType:" + action.loadingItemType);
                }
            }
            else
            {
                //判断模拟的进度值是否完成
                _progress = 1.0f;
            }
        }
    }

    void UpdateProcess()
    {
        if (loadAsync == null)
            return;

        _progress += 0.02f;
        if (!loadAsync.isDone)
        {
            return;
        }

        _lastProgress   += LoadingItemInfoDic[LoadingItem.SceneLoad].fPercent;
        _progress       = _targetProgress = _lastProgress;

        loadAsync       = null;
        _bSceneLoaded   = true;
        //GameApplication.instance.CurrentState.OnSceneLoadComplate();
        loadScence      = null;
    }
}
