using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;








public class GuideManager : MonoBehaviour
{

    public enum GuideFSM
    {
        Idle,
        delay,      // 延时阶段
        fadein,
        ani,        // 开始引导
        fadeout,
    }
    /// <summary>
    /// 当前的引导数据
    /// </summary>
    private CTagGuideConfig mCurGuideConfig = null;

    /// <summary>
    /// 当前引导事件ID
    /// </summary>
    private int             mCurGuideEventID = -1;

    /// <summary>
    /// 当前显示的引导按钮
    /// </summary>
    public GameObject       mBG           = null;
    public GameObject       mCurGuideBtn    = null;

    /// <summary>
    /// 
    /// </summary>
    private GameObject      mCtrlObject     = null;
    private GameObject      mOrgctrlObject  = null;
    // 引导状态机
    public GuideFSM         m_fsm = GuideFSM.Idle;

    private Vector3         mSrcPosition = Vector3.zero;
    private Vector3         mDstPosition = Vector3.zero;

    private static GuideManager Guide;
   
    /// <summary>
    /// 是否显示背景
    /// </summary>
    private bool            mIsShowGB = false;


    private void Start()
	{
        Guide = this;
        mBG.SetActive(false);
        mCurGuideBtn.SetActive(false);

        Invoke("RegistEvent", 0.5f);
	}

    void RegistEvent()
    {
        EventSystem.Instance.RegisterEvent(EventId.NetworkStatus, this, null, OnEventHandler);
        EventSystem.Instance.RegisterEvent(EventId.ReconnectResult, this, null, OnEventHandler);
    }


    private void OnEventHandler(int eventId, object data, params object[] args)
    {
        if (eventId == (int)EventId.NetworkStatus || eventId == (int)EventId.ReconnectResult )
        {
             ClearGuideData();
        }
    }
    /// <summary>
    /// 引导的逻辑
    /// </summary>
    private float fDelayTime = 0;
    private void Update( )
    {
        if( m_fsm == GuideFSM.delay )
        {
            fDelayTime -= Time.deltaTime;
            if (fDelayTime <= 0)
                m_fsm = GuideFSM.ani;
        }
        else if( m_fsm == GuideFSM.fadeout )
        {
            m_fsm = GuideFSM.Idle;
            FadeGuideButton(false);
        }

        else if( m_fsm == GuideFSM.ani )
        {
            AnimationGuideBtn();
        }
    }

    /// <summary>
    /// 开始某个引导
    /// </summary>
    public static void StartGuide(GuildCondition eC, string strC, GameObject objPanel = null )
    {
        Guide.GetGuideConfigByCondition( eC, strC, objPanel );
    }

    public static void StartGuide( int nGuideID )
    {
        Guide.StartNewGuide(nGuideID, true);
    }

    /// <summary>
    /// 触发引导引导结束
    /// </summary>
    public static void TriggerGuideEnd( GuildEndEvent eEvent )
    {
        if ( Guide.mCurGuideConfig != null && Guide.mCurGuideConfig.endCondition != eEvent)
            return;
        Guide._TriggerGuideEnd();
    }


    /// <summary>
    /// 触发引导引导结束
    /// </summary>
    public static void TriggerCurGuideEnd()
    {
        if( Guide.mCurGuideConfig != null )
            Guide._TriggerGuideEnd();
    }

    /// <summary>
    /// 触发引导引导结束
    /// </summary>
    public static void ClearGuideData()
    {
        Guide.Clear();
    }

    private void Clear()
    {
        m_fsm = GuideFSM.Idle;
        mCurGuideConfig = null;
        mCurGuideEventID = -1;
        mBG.SetActive(false);
        mCurGuideBtn.SetActive(false);
        if (mCtrlObject != null)
        {
            GameObject.Destroy(mCtrlObject);
            mCtrlObject = null;
        }
    }

    /// <summary>
    /// 触发引导引导结束
    /// </summary>
    private void _TriggerGuideEnd()
    {

        if (mCurGuideConfig == null)
            return;
        EndGuide();
        int nNextGuideID = mCurGuideConfig.nextID;
        if( nNextGuideID > 0 )
        {
          
            m_fsm = GuideFSM.Idle;
            mCurGuideConfig = null;
            StartNewGuide(nNextGuideID, true);
        }
        else
        {
            m_fsm = GuideFSM.Idle;
            mCurGuideConfig     = null;
            mCurGuideEventID    = 0;
            mBG.SetActive(false);
            mCurGuideBtn.SetActive(false);
        }
    }

    /// <summary>
    /// 开始新的引导
    /// </summary>
    /// <param name="nGuideID"></param>
    private void StartNewGuide( int nGuideID, bool bReSetBG = false )
    {
        if (m_fsm != GuideFSM.Idle)
            return;

        if (nGuideID == mCurGuideEventID)
            return;
        mIsShowGB = false;
        mCurGuideEventID    = nGuideID;
        mCurGuideConfig     = GuideDataProvider.Get().GetValue(mCurGuideEventID);
        if (mCurGuideConfig != null)
        {
            if (bReSetBG && !string.IsNullOrEmpty(mCurGuideConfig.ctrlname) )
            {
                // 有一个潜规则，uiwindow 的窗口名跟prefab 名字相同，至少_前相同，例如 StartWindow	UI/StartWindow_h4
                string[] str = mCurGuideConfig.window.Split('_');
                BaseWindow uiPanel = UISystem.Get().GetWindow( str[0] );
                if(uiPanel!= null )
                {
                    InstanceCtrlObject(uiPanel.gameObject, mCurGuideConfig.ctrlname );
                }
            }
            //Debug.Log("StartNewGuide = " + mCurGuideEventID.ToString());

            mSrcPosition.x = mCurGuideConfig.srcX;
            mSrcPosition.y = mCurGuideConfig.srcY;
            mSrcPosition.z = 0;
            mDstPosition.x = mCurGuideConfig.dstX;
            mDstPosition.y = mCurGuideConfig.dstY;
            mDstPosition.z = 0;
            if( mCurGuideConfig.coordsinates == Coordinates.CD_3D )
            {
                mSrcPosition = ChangeWorldPos2UILocal(mSrcPosition);
                mDstPosition = ChangeWorldPos2UILocal(mDstPosition); 
            }

            TweenPosition tp = mCurGuideBtn.GetComponent<TweenPosition>();
            if( tp == null )
            {
                mCurGuideBtn.AddComponent<TweenPosition>();
            }

            m_fsm = GuideFSM.delay;
            fDelayTime = 0.5f;
            mCurGuideBtn.transform.localPosition = mSrcPosition;
            mCurGuideBtn.transform.rotation      = Quaternion.Euler(0, 0, mCurGuideConfig.angle);
        }
    }


    /// <summary>
    /// 结束当前引导
    /// </summary>
    public void EndGuide( )
    {
        if (mCtrlObject != null)
        {
            GameObject.Destroy(mCtrlObject);
            mCtrlObject = null;
        }
        if (mCurGuideBtn != null)
            mCurGuideBtn.SetActive(false);

        mBG.SetActive(false);
    }

    /// <summary>
    /// 动画当前引导按钮
    /// </summary>
    private void AnimationGuideBtn( )
    {

        if (mCurGuideConfig == null || mCurGuideConfig.moveType == BtnMoveType.BMT_Null)
            return;

        if (mIsShowGB)
            mBG.SetActive(true);
        if (mCtrlObject != null)
            mCtrlObject.SetActive(true);

        mCurGuideBtn.SetActive(true);
        TweenPosition tp = mCurGuideBtn.GetComponent<TweenPosition>();
        if( tp != null )
        {
            tp.ResetToBeginning();
            if (mCurGuideConfig.moveType == BtnMoveType.BMT_ResetMove)
                tp.style = UITweener.Style.Once;
            else
                tp.style = UITweener.Style.PingPong;

            tp.from = mSrcPosition;
            tp.to   = mDstPosition;
            tp.duration = mCurGuideConfig.duration;
            tp.SetOnFinished(() =>
            {
                Guide.m_fsm = GuideFSM.fadeout;
            });
            tp.Play(true);
        }

        FadeGuideButton(true);
        m_fsm = GuideFSM.Idle;
    }


    private void FadeGuideButton( bool bFadeIn )
    {
       
        TweenAlpha ta = mCurGuideBtn.GetComponent<TweenAlpha>();
        if (ta == null)
        {
            ta = mCurGuideBtn.AddComponent<TweenAlpha>();
        }

        ta.ResetToBeginning();
        ta.from     = bFadeIn ? 0 : 1;
        ta.to       = bFadeIn ? 1 : 0;
        ta.duration = 0.5f;
        ta.SetOnFinished(() =>
        {
            if (!bFadeIn)
                Guide.m_fsm = GuideFSM.ani;

        });
        ta.Play(true);
    }

    private Vector3 ChangeWorldPos2UILocal( Vector3 pos )
    {
        Vector3 local   = Camera.main.WorldToScreenPoint(pos );
        local.z         = 0;
        Vector3 Temp    = UICamera.currentCamera.ScreenToWorldPoint(local);
        return gameObject.transform.worldToLocalMatrix.MultiplyPoint(Temp);
    }


    private void GetGuideConfigByCondition(GuildCondition eC, string strC, GameObject objPanel )
    {
        int nCompltedID = -1;
        CTagGuideConfig temp = GuideDataProvider.Get().GetGuideConfigByCondition(eC, strC, nCompltedID);
        if( temp != null && mCurGuideConfig != null )
        {
            if (temp.ID <= mCurGuideConfig.ID)
                return;

            StartNewGuide(temp.ID);
        }

        else if( temp != null && mCurGuideConfig == null )
        {
            StartNewGuide( temp.ID );
        }

        if (temp != null && !string.IsNullOrEmpty(temp.ctrlname))
        {
            InstanceCtrlObject(objPanel, temp.ctrlname);
        }
    }


    private void InstanceCtrlObject( GameObject objPanel, string strCtrl )
    {
        if (objPanel == null)
            return;


        Transform form = objPanel.transform.Find(strCtrl);
        if(form)
        {
            GameObject pTemp    = form.gameObject;
            mCtrlObject         = GameObject.Instantiate(pTemp);

            mIsShowGB = true;
            string[] sub = mCtrlObject.name.Split( '(' );
            mCtrlObject.name = sub[0];
            mCtrlObject.transform.localPosition = form.localPosition;
            mCtrlObject.transform.position = form.position;
            
            mCtrlObject.transform.parent = mCurGuideBtn.transform.parent;
            mCtrlObject.transform.localScale = form.localScale;
            mCtrlObject.GetComponent<UISprite>().depth = 123;

            // 遍历所有子对象
            UIWidget[] trans;
            trans = mCtrlObject.GetComponentsInChildren<UIWidget>();
            for( int i = 0; i < trans.Length; i++ )
            {
                UIWidget sprite = trans[i];
                if( sprite != null )
                    sprite.depth += 123;
            }
            mCtrlObject.SetActive(false);

        }
    }
}
