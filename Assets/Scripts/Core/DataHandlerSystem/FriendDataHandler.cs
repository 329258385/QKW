using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;



/// <summary>
/// 社交管理器基本信息管理
/// </summary>
public class FriendDataHandler : Solarmax.Singleton<FriendDataHandler>, IDataHandler
{

    /// <summary>
    /// 我关注的好友
    /// </summary>
    public List<SimplePlayerData> myFollowList;

    /// <summary>
    /// 粉丝
    /// </summary>
    public List<SimplePlayerData> followerList;

    /// <summary>
    /// followerList 与本人的关注状态
    /// </summary>
    public List<bool>             followerStatus;

    /// <summary>
    /// followerList 与本人的关注状态
    /// </summary>
    public List<bool>             myFollowStatus;
    public FriendDataHandler()
	{
		
	}
		
	public bool Init()
	{
        myFollowList = new List<SimplePlayerData>();
        followerList = new List<SimplePlayerData>();
        followerStatus = new List<bool>();
        myFollowStatus = new List<bool>();
		Release ();
		return true;
	}

	public void Tick(float interval)
	{
		
	}

	public void Destroy()
	{
		Release ();
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	public void Release()
	{
        myFollowList.Clear();
        followerList.Clear();
        followerStatus.Clear();
        myFollowStatus.Clear();
	}


    /// <summary>
    /// 增加我关注的玩家
    /// </summary>
    public void AddMyFollow( SimplePlayerData sPlayer, bool bFollow )
    {
        if (!IsMyFollow(sPlayer, bFollow))
        {
            myFollowList.Add(sPlayer);
            myFollowStatus.Add(bFollow);
            SetMutualFollow(sPlayer.userId, true);
        }
    }

    public void DelMyFollow( SimplePlayerData sPlayer )
    {
        for (int i = 0; i < myFollowList.Count; i++)
        {
            if (myFollowList[i].userId == sPlayer.userId )
            {
                myFollowList.RemoveAt(i);
                myFollowStatus.RemoveAt(i);

                // 如果存在取消相互关注标示
                SetMutualFollow(sPlayer.userId, false);
                return;
            }
        }
    }

    /// <summary>
    /// 增加关注我的玩家
    /// </summary>
    public void AddFollower(SimplePlayerData sPlayer, bool bFollow )
    {
        if (!IsFollow(sPlayer, bFollow))
        {
            followerList.Add(sPlayer);
            followerStatus.Add(bFollow);
         
        }
    }


    /// <summary>
    /// 删除粉丝
    /// </summary>
    public void DelFollower(SimplePlayerData sPlayer)
    {
        for (int i = 0; i < followerList.Count; i++)
        {
            if (followerList[i].userId == sPlayer.userId)
            {
                followerList.RemoveAt(i);
                followerStatus.RemoveAt(i);
                return;
            }
        }
    }
    /// <summary>
    /// 判断这个人，我是否已关注
    /// </summary>
    public bool IsMyFollow(SimplePlayerData player, bool bFollow)
    {
        //int orderUserID = useID;
        //SimplePlayerData sPlayer = myFollowList.Find(delegate(SimplePlayerData role)
        //{
        //    return role.userId == orderUserID;
        //});

        //if (sPlayer != null)
        //    return true;
        for (int i = 0; i < myFollowList.Count; i++ )
        {
            if (myFollowList[i].userId == player.userId)
            {
                myFollowStatus[i] = bFollow;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 判断这个人，我是否已关注
    /// </summary>
    public bool IsMyFollowEX(int useID)
    {
        int orderUserID = useID;
        SimplePlayerData sPlayer = myFollowList.Find(delegate(SimplePlayerData role)
        {
            return role.userId == orderUserID;
        });

        if (sPlayer != null)
            return true;
        
        return false;
    }


    /// <summary>
    /// 这是个人是否是我的粉丝
    /// </summary>
    public bool IsFollow(SimplePlayerData player, bool bFollow)
    {
        //int orderUserID = useID;
        //SimplePlayerData sPlayer = followerList.Find(delegate(SimplePlayerData role)
        //{
        //    return role.userId == orderUserID;
        //});

        //if (sPlayer != null)
        //    return true;

        for (int i = 0; i < followerList.Count; i++)
        {
            if (followerList[i].userId == player.userId )
            {
                followerStatus[i] = bFollow;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 这是个人是否是我的粉丝
    /// </summary>
    public bool IsIsFollowEX(int useID)
    {
        int orderUserID = useID;
        SimplePlayerData sPlayer = followerList.Find(delegate(SimplePlayerData role)
        {
            return role.userId == orderUserID;
        });

        if (sPlayer != null)
            return true;

        return false;
    }


    public void SetMutualFollow( int useID, bool bValue )
    {
        for (int i = 0; i < followerList.Count; i++)
        {
            if (followerList[i].userId == useID)
            {
                followerStatus[i] = bValue;
                return;
            }
        }
    }

    public void SetMutualMyFollow(int useID, bool bValue)
    {
        for (int i = 0; i < myFollowList.Count; i++)
        {
            if (myFollowList[i].userId == useID)
            {
                myFollowStatus[i] = bValue;
                return;
            }
        }
    }
}
