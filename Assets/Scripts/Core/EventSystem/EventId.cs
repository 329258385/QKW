using System;

public enum EventId : int
{
	Undefine = 0,
	JoinRoom,						// 加入房间功能
	UpdateRoomInfo,					// 更新房间玩家信息
	NoticeSelfTeam,					// 战斗界面显示己方星球的提示
	RequestUserResult,				// 获取玩家信息结果
	CreateUserResult,				// 创建玩家账户结果
	SyncUserData,					// 同步玩家信息
	LoadRankList,					// 加载了排行榜数据

	OnMatch,						// match
	OnReady,						// ready
	OnFinished,						// finish
	OnFinishedColor,				// finish color

	OnOccupiedNode, 			    // 佔領星球
	OnFriendFollowResult,			// 关注好友回复
	OnFriendLoadResult,				// 获取粉丝和关注列表回复
	OnFriendNotifyResult,			// 被关注、取消关注通知
	OnFriendRecommendResult,		// 获取推荐朋友回复
	OnFriendSearchResult,			// 搜索朋友结果
	OnFriendSearchResultShow,		// 收缩朋友结果展示

	OnTeamCreateResult,				// 创建队伍
	OnTeamInviteResult,				// 邀请
	OnTeamUpdate,					// 队伍更新
	OnTeamDelete,					// 队伍删除
	OnTeamStart,					// 组队开始
	OnTeamInviteRequest,			// 被邀请组队
	OnTeamInviteResponse,			// 被邀请组队后的拒绝回复

	OnBattleReportLoad,				// 战报加载
	OnBattleReportPlay,				// 战报播放
	OnBattleReplayFrame,			// 重播刷新进度

	OnCommonDialog,					// 通用提示窗口

	OnBattleDisconnect,				// 战斗中断线

	OnGetLeagueInfoResult,			// 获得当前的锦标赛信息
	OnLeagueListResult,				// 获得锦标赛列表
	OnLeagueSignUpResult,			// 获得锦标赛报名结果
	OnLeagueShowRankData,			// 显示rank时，推送的数据
	OnLeagueRankResult,				// 获得锦标赛排行榜
	OnLeagueMatchResult, 			// 获得锦标赛匹配结果
	OnLeagueRankBack,				// 从锦标赛活动返回

	OnABGetVersions,				// 获取需要更新的版本文件
	OnABDownloadIndex,				// 正在接收文件
	OnABDownloading,				// 下载资源进度
	OnABDownloadingFinished,		// 接收完成
	OnPackageUpdate,				// 整包更新
	OnHttpNotice,					// 获取http公告信息

	OnTouchBegin,					// 开始战斗分兵触摸
	OnTouchPause,					// 退出当前节点
	OnTouchEnd,						// 开始战斗分兵触摸
	OnGetRoomList,					// 更新房间列表
	OnJoinRoom,						// 进入房间
	OnCreateRoom,					// 创建房间
	OnRoomRefresh,					// 刷新房间内信息
	OnRoomListREfresh,				// 刷新大厅内房间信息

	OnStartMatch2,					// 第二种匹配方案

	OnStartMatch3,					// 第三种方案
	OnMatch3Notify,					// 第三章匹配方案的房间更新

	OnStartSelectRace,				// 开始选择种族

	OnGetRaceData,					// 获得所有种族信息
	OnAddArticleEvent,              // 增加物品
    OnDelArticleEvent,              // 删除物品
    OnUpdateArticleEvent,			// 更新物品数量

	OnStatusWindowTabSelect,		// 状态页下标选中

	OnCoinSync,						// 货币更新

	OnChestNotify,					//宝箱更新
	OnChestTime,
	OnChestBattle,
	OnPopulationUp,					//人口上升
	OnPopulationDown,				//人口下降

	OnSkillPop,						//技能提示

	OnRename,						// 改名字
	OnRenameFinished,				// 改名成功

	OnStartSingleBattle,			// 开始单机战斗

	OnSelfDied,						// 自己死亡，进入观战
	
    OnSCRaceNotify,                 // 获得种族

	NetworkStatus,					// 网络状态
	PingRefresh,					// ping刷新
	ReconnectResult,				// 重连结束

	OnSDKLoginResult,				// SDK登陆结果

    OnStorageLoaded,
    OnUpdateNoticy,

	OnManualSelectLeaguePage,		// 结果界面请求主动显示league

	PlayerGiveUp,					// 玩家放弃，显示接管图标
	PlayerDead,						// 玩家死亡，显示死亡图标

	OnStartMatchResult,				// 开始匹配结果
	OnMatchInit,					// 匹配开始
	OnMatchUpdate,					// 匹配更新
	OnMatchQuit,					// 退出匹配

	ShowForWatchMode,				// 观战界面

	OnLoadChaptersResult,				// 加载所有章节信息
	OnLoadOneChapterResult,				// 加载章节所有关卡信息
	OnStartLevelResult,					// 开始关卡
	OnSetLevelStarResult,				// 设置关卡星星结果

	UpdateChapterWindow,
	OpenSelectLevelWindow,				// 打开选关卡界面
	UpdateSelectLevelWindowStar,		// 更新选关卡部分星星

	UpdatePower,					    // 更新体力

    UpdateNodeState,                    // 星球状态变化
    OnPlayerMoveStart,                  // 战队移动开始

    OnNextLevel,
    OnPrevLevel,
}

