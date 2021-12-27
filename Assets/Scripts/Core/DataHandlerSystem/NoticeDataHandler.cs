using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Plugin;
using Solarmax;



public class tagRed
{
    public tagRed()
    {
        UnlockSkillID = new bool[6];
        for( int i = 0; i < UnlockSkillID.Length; i++ )
        {
            UnlockSkillID[i] = false;
        }
    }
    public bool[]  UnlockSkillID = null;
};


public enum NOTICEIMAGEITEM
{
    NOTICE_GETRACE,             // 获得种族
    NOTICE_UPGRADE_SKILL,       // 升级界面
    NOTICE_NUM,
};


public class NoticeDataHandler : Solarmax.Singleton<NoticeDataHandler>, Solarmax.IDataHandler
{
	
   
    /// <summary>
    /// 红点类型
    /// </summary>
    private int[]     notices = null;


    private tagRed[] unskill = new tagRed[6];
    private List<NetMessage.RaceData> raceList = new List<NetMessage.RaceData>();

    /// <summary>
    /// 
    /// </summary>
    public NoticeDataHandler( )
    {
        int nMax = (int)NOTICEIMAGEITEM.NOTICE_NUM;
        notices  = new int[nMax];
        for (int i = 0; i < (int)NOTICEIMAGEITEM.NOTICE_NUM; i++)
            notices[i] = 0;

        for( int i = 0; i < 6; i++ )
        {
            unskill[i] = new tagRed();
        }
    }


	public bool Init()
    {
        EventSystem.Instance.RegisterEvent(EventId.OnStorageLoaded, this, null, OnEventHandler);
        EventSystem.Instance.RegisterEvent(EventId.OnGetRaceData, this, null, OnEventHandler);
        //string str = "100000000000100000000000010000";
        //ResolveString2flag(str);
        return true;
    }

	public void Destroy()
    {

    }

	public void Tick(float interval)
    {
       
    }

    /// <summary>
    /// 从字符串中解析种族技能的表示
    /// </summary>
    /// <param name="str"></param>
    public void ResolveString2flag( string str )
    {
        if ( !string.IsNullOrEmpty(str))
        {
            bool bNoticy = false;
            tagRed race = null;
            char[] pbuf = str.ToCharArray();
            for( int i = 0; i < pbuf.Length; i++ )
            {
                int mod = i / 6 + 1;
                int idx = i % 6;
                if (mod >= 6)
                    return;

                race = unskill[mod];
                if( pbuf[i] == '1' )
                {
                    bNoticy = true;
                    race.UnlockSkillID[idx] = true;
                }
                else if( pbuf[i] == '0')
                {
                    race.UnlockSkillID[idx] = false;
                }
            }

            // 这样写是因为同时有两天网络消息更新跟标示
            if (bNoticy)
                SetNoticeValue(NOTICEIMAGEITEM.NOTICE_GETRACE, 1);
            EventSystem.Instance.FireEvent(EventId.OnUpdateNoticy, true);
        }
    }
  
    /// <summary>
    /// 判断有没有新解锁的技能
    /// </summary>
    /// <param name="nRace"></param>
    public bool IsUnlockBySkill( int nRaceID )
    {
        tagRed race = unskill[nRaceID];
       if (race == null)
           return false;

       for (int i = 0; i < race.UnlockSkillID.Length; i++)
       {
           if (race.UnlockSkillID[i])
               return true;
       }

       return false;
    }


    public bool IsUnlockBySkill( int nRaceID, int idx)
    {
        tagRed race = unskill[nRaceID];
        if (race==null)
            return true;

        if (idx >= 0 && idx < race.UnlockSkillID.Length)
            return race.UnlockSkillID[idx];
        
        return false;
    }


    public void ResetUnlockSkill(int nRaceID, int idx)
    {
        tagRed race = unskill[nRaceID];
        if (race == null)
            return;

        if (idx >= 0 && idx < race.UnlockSkillID.Length)
            race.UnlockSkillID[idx] = false;
    }


    public int GetNoticeValue( NOTICEIMAGEITEM notice )
    {
        int idx = (int)notice;
        if( idx >= 0 && idx < (int)NOTICEIMAGEITEM.NOTICE_NUM)
        {
            return notices[idx];
        }
        return 0;
    }

    public void SetNoticeValue(NOTICEIMAGEITEM notice,int nValue )
    {
        int idx = (int)notice;
        if (idx >= 0 && idx < (int)NOTICEIMAGEITEM.NOTICE_NUM)
        {
            notices[idx] = nValue;
        }
    }

    public bool IsNotice( NOTICEIMAGEITEM notice )
    {
        int idx = (int)notice;
        return notices[idx] > 0;
    }

    private void OnEventHandler(int eventId, object data, params object[] args)
    {
        if (eventId == (int)EventId.OnStorageLoaded)
        {
            string redstr = (string)args[0];
            if( !string.IsNullOrEmpty(redstr) )
            {
                NoticeDataHandler.Instance.ResolveString2flag(redstr);
            }
        }
        else if (eventId == (int)EventId.OnGetRaceData)
        {
            IList<NetMessage.RaceData> list = (IList<NetMessage.RaceData>)args[0];
            raceList.Clear();
            raceList.AddRange(list);
            raceList.Sort((args0, arg1) =>
            {
                return args0.race.CompareTo(arg1.race);
            });

            if( IsHaveUpgradeSkill() )
            {
                SetNoticeValue(NOTICEIMAGEITEM.NOTICE_GETRACE, 1);
                EventSystem.Instance.FireEvent(EventId.OnUpdateNoticy, true);
            }
        }
    }


    public void SyncSkillFlag2Server( )
    {
        string strFlag = "000000000000000000000000000000";
        char[] buf = strFlag.ToCharArray();

        int idx = 0;
        bool bNoticy = false;
        for( int i = 1; i < unskill.Length; i++ )
        {
            tagRed race = unskill[i];
            for( int j = 0; j < race.UnlockSkillID.Length; j++ )
            {
                if (idx < strFlag.Length)
                {
                    if (race.UnlockSkillID[j])
                    {
                        buf[idx] = '1';
                        bNoticy = true;
                    }
                    else
                        buf[idx] = '0';
                    idx++;
                }
            }
        }

        SetNoticeValue(NOTICEIMAGEITEM.NOTICE_GETRACE, bNoticy ? 1 : 0);

        strFlag = new string(buf);
		NetSystem.Instance.helper.SetClientStorage((int)NetMessage.ClientStorageConst.ClientStorageRedPoints, strFlag);
    }


    private bool IsHaveUpgradeSkill()
    {
        // 只要有一个就返回true
        for (int i = 0; i < raceList.Count; i++ )
        {
            NetMessage.RaceData data = raceList[i];
            if( data != null )
            {
                for( int j = 0; j < data.skills.Count; j++ )
                {
                    
                }
            }
        }
        return false;
    }
}	
