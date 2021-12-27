using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Solarmax;


public class ChapterLevelInfo
{
	public int sn;	//显示在UI上面的序列号
	public string id;
	public int star;
	public bool isMain;
	public bool unLock;

	public ChapterLevelInfo()
	{
		sn = 0;	//显示在UI上面的序列号
		id = "";
		star = 0;
		isMain = true;
		unLock = false;
	}
}

public class ChapterInfo
{
	public int sn;	//用于在UI的排列
	public string id;
	public List<ChapterLevelInfo> levelList;
	public int star;	//章节奖杯总数
	public bool achieveMainLine;	//主线关卡是否完成
	public int achieveLevels;		//通关数量
	public bool unLock;	//是否解锁
	public int mainLineNum;	//主线关卡数量

	public ChapterInfo()
	{
		sn = 0;	//用于在UI的排列
		id = "";
		star = 0;
		levelList = new List<ChapterLevelInfo>();		//主线关卡
		unLock = false;
		achieveMainLine = false;	//主线关卡是否完成
		achieveLevels = 0;		//通关数量
	}

	/// <summary>
	/// 获取关卡总数
	/// </summary>
	/// <returns>The level count.</returns>
	public int GetLevelCount()
	{
		return levelList.Count;
	}
}
	
/// <summary>
/// 关卡管理器基本信息管理
/// </summary>
public class LevelDataHandler : Solarmax.Singleton<LevelDataHandler>, IDataHandler
{

	/// <summary>
	/// 章节数据
	/// </summary>
	public List<ChapterInfo> chapterList;

	/// <summary>
	/// 星星总数
	/// </summary>
	public int allStars;


	public ChapterInfo currentChapter;				// 玩家当前选择的章节
	public ChapterLevelInfo currentLevel;			// 玩家当前选择的关卡


	public LevelDataHandler()
	{
		currentChapter = null;
		currentLevel = null;
	}

	public bool Init()
	{
		chapterList = new List<ChapterInfo>();
		Release ();

		initChapterInfo ();
		EventSystem.Instance.RegisterEvent(EventId.OnLoadChaptersResult, this, null, OnEventHandler);
		EventSystem.Instance.RegisterEvent(EventId.OnLoadOneChapterResult, this, null, OnEventHandler);
		EventSystem.Instance.RegisterEvent(EventId.OnSetLevelStarResult, this, null, OnEventHandler);
		return true;
	}

	public void Tick(float interval)
	{

	}

	public void Destroy()
	{
		Release ();
		EventSystem.Instance.UnRegisterEvent(EventId.OnLoadChaptersResult, this);
		EventSystem.Instance.UnRegisterEvent(EventId.OnLoadOneChapterResult, this);
		EventSystem.Instance.UnRegisterEvent(EventId.OnSetLevelStarResult, this);
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	public void Release()
	{
		allStars = 0;
		currentChapter = null;	// 玩家当前选择的章节
		currentLevel = null;		// 玩家当前选择的关卡
		chapterList.Clear();
	}
		
	/// <summary>
	/// 从数据表读取数据。一定要先于网络处理。
	/// </summary>
	void initChapterInfo()
	{
		List<ChapterConfig> chapterCfgs = ChapterConfigProvider.Instance.GetAllData ();
		if (chapterCfgs.Count == 0) 
		{
			return;
		}

		List<LevelConfig> LevelCfgs = LevelConfigConfigProvider.Instance.GetAllData ();
		if (LevelCfgs.Count == 0) 
		{
			return;
		}
			
		int chaptersn = 1;
		ChapterConfig chapterCfg;
		LevelConfig levelCfg;
		for (int i = 0; i < chapterCfgs.Count; ++i) 
		{
			chapterCfg = chapterCfgs [i];
			ChapterInfo chapter = new ChapterInfo();
			chapter.sn = chaptersn;
			chapter.id = chapterCfg.id;
			chapter.unLock = false;
			chapterList.Add (chapter);

			int mainLevelsn = 0;
			int branchLinesn = 0;
			for (int j = 0; j < LevelCfgs.Count; ++j) 
			{
				levelCfg = LevelCfgs [j];	
				if (levelCfg.chapter == chapterCfg.id) 
				{
					ChapterLevelInfo level = new ChapterLevelInfo ();
					level.id = levelCfg.id;
					level.isMain = levelCfg.mainLine > 0;
					if (level.isMain) {
						level.sn = ++mainLevelsn;
					} else {
						level.sn = ++branchLinesn;
					}
					if (string.IsNullOrEmpty (levelCfg.dependLevel))
						level.unLock = true;
					chapter.levelList.Add (level);
				}
			}
				
			chapter.mainLineNum = mainLevelsn;
			chaptersn++;
		}
	}

	/// <summary>
	/// Queries the index of the chapter.
	/// 0:查找失败 。 > 0 真实sn
	/// </summary>
	/// <returns>The chapter index.</returns>
	/// <param name="id">Identifier.</param>
	public int QueryChapterSN(string id)
	{
		foreach (var info in chapterList) 
		{
			if (info.id == id)
				return info.sn;
		}
		return 0;
	}

	public ChapterInfo QueryChapterInfo(string id)
	{
		foreach (var info in chapterList) 
		{
			if (info.id == id)
				return info;
		}
		return null;
	}


	private void OnEventHandler(int eventId, object data, params object[] args)
	{
		if (eventId == (int)EventId.OnLoadChaptersResult) 
		{
			// 所有章节信息
			NetMessage.SCLoadChapters proto = args [0] as NetMessage.SCLoadChapters;
			for (int i = 0; i < proto.chapter.Count; ++i) {
				string chapter = proto.chapter[i];
				int chapterStars = proto.star [i];
				int finishedLevelNum = proto.finish_level_num [i];
				SetChapterStars (chapter, chapterStars, finishedLevelNum);
			}

			// 此处需要解锁章节
			UnlockChapters ();

			// 更新选择章节页面
			EventSystem.Instance.FireEvent (EventId.UpdateChapterWindow);

		} 
		else if (eventId == (int)EventId.OnLoadOneChapterResult) 
		{
			// 某个章节
			NetMessage.SCLoadChapter proto = args [0] as NetMessage.SCLoadChapter;
			string chapterId = proto.chapter;
			for (int i = 0; i < proto.level.Count; ++i) {
				string level = proto.level[i];
				int star = proto.star [i];
				SetChapterLevelStar (chapterId, level, star);
			}

			// 此处需要解锁关卡
			UnlockLevels (chapterId);

			// 打开关卡列表
			EventSystem.Instance.FireEvent (EventId.OpenSelectLevelWindow);
		} 
		else if (eventId == (int)EventId.OnSetLevelStarResult) 
		{
			// 设置关卡星星数
			NetMessage.SCSetLevelStar proto = args [0] as NetMessage.SCSetLevelStar;
			if (proto.code == NetMessage.ErrCode.EC_Ok) {
				string chapterId = proto.chapter_name;
				string levelId = proto.level_name;
				int levelStar = proto.star;
				ChapterInfo nowChapter = GetChapterInfo (chapterId);
				ChapterLevelInfo nowLevel;
				// 更新cup值
				for (int j = 0; j < nowChapter.levelList.Count; ++j) {
					nowLevel = nowChapter.levelList [j];
					if (nowLevel.id.Equals (levelId)) {
						int tempStar = nowLevel.star;
						nowLevel.star = levelStar;

						nowChapter.star += levelStar - tempStar;
						allStars += levelStar - tempStar;
					}
				}

				// 生成完成数据
				GenerateAchievement (chapterId);
				// 解锁关卡
				UnlockLevels (chapterId);
				// 解锁章节
				UnlockChapters ();

				EventSystem.Instance.FireEvent (EventId.UpdateSelectLevelWindowStar);
			}
		}
	}

	private ChapterInfo GetChapterInfo (string chapterId)
	{
		ChapterInfo ret = null;
		for (int i = 0; i < chapterList.Count; ++i) 
		{
			if (chapterList [i].id.Equals (chapterId)) 
			{
				ret = chapterList [i];
			}
		}

		if (ret == null) 
		{
			Debug.LogError ("获取当前章节失败:" + chapterId);
		}

		return ret;
	}

	/// <summary>
	/// 设置章节星星数
	/// </summary>
	private void SetChapterStars (string chapterId, int star, int finishedLevelNum)
	{
		ChapterInfo chapter = GetChapterInfo (chapterId);
		allStars -= chapter.star; // 这句话必须要，因为连续两次打开页面，在上次数据中，allstar已经有数据，此时allstar要减掉上次的数据
		chapter.star = star;
		allStars += star;
		chapter.achieveLevels = finishedLevelNum;

		// 此处默认必须完成所有主线关卡，才开启支线关卡。所以采用此方式。。。。
		if (finishedLevelNum >= chapter.mainLineNum) 
		{
			chapter.achieveMainLine = true;
		}
	}

	/// <summary>
	/// 解锁章节
	/// mark： 此处还需要判断依赖章节是否主线通关
	/// 但是，由于第一次获取所有章节信息时，并不能知道这些章节是否主线通关，所以，此方法在获取新的星星时调用是有效的，而第一次调用时无效
	/// 所以此处，需要协商一下，何时获得主线通关的数据（见SetChapterStars中的对于主线和支线关卡的处理方式。）
	/// 
	/// </summary>
	private void UnlockChapters ()
	{
		for (int i = 0; i < chapterList.Count; ++i) 
		{
			var chapter = chapterList [i];
			var chapterConfig = ChapterConfigProvider.Instance.GetData (chapter.id);
			bool dependChapterMainlineComplete = true;
			if (!string.IsNullOrEmpty (chapterConfig.dependChapter)) 
			{
				var dependChapter = GetChapterInfo (chapterConfig.dependChapter);
				if (null != dependChapter) 
				{
					dependChapterMainlineComplete = GetChapterInfo (chapterConfig.dependChapter).achieveMainLine;
				}
			}
			if (allStars >= chapterConfig.needStar && dependChapterMainlineComplete) 
			{
				chapter.unLock = true;
			}
		}
	}

	/// <summary>
	/// 设置关卡星星数
	/// </summary>
	private void SetChapterLevelStar (string chapterId, string levelId, int star)
	{
		ChapterInfo chapter = GetChapterInfo (chapterId);
		for (int j = 0; j < chapter.levelList.Count; ++j) {
			var level = chapter.levelList [j];
			if (level.id.Equals (levelId)) {
				level.star = star;
			}
		}
	}

	/// <summary>
	/// 解锁章节中的关卡
	/// </summary>
	private void UnlockLevels (string chapterId)
	{
		ChapterInfo chapter = GetChapterInfo (chapterId);

		for (int j = 0; j < chapter.levelList.Count; ++j) {
			var level = chapter.levelList [j];
			LevelConfig levelConfig = LevelConfigConfigProvider.Instance.GetData (level.id);
			var dependLevelId = levelConfig.dependLevel;
			if (string.IsNullOrEmpty (dependLevelId)) {
				level.unLock = true;
			} else {
				ChapterLevelInfo dependLevelInfo = null;
				for (int k = 0; k < chapter.levelList.Count; ++k) {
					if (chapter.levelList [k].id.Equals (dependLevelId)) {
						dependLevelInfo = chapter.levelList [k];
						break;
					}
				}
				if (dependLevelInfo != null) {
					level.unLock = dependLevelInfo.unLock;
				}
			}
		}
	}

	/// <summary>
	/// 生成通关数据
	/// </summary>
	private void GenerateAchievement (string chapterId)
	{
		var chapter = GetChapterInfo (chapterId);
		chapter.achieveLevels = 0;
		chapter.achieveMainLine = true;
		for (int j = 0; j < chapter.levelList.Count; ++j) {
			var level = chapter.levelList [j];
			if (level.star > 0) {
				chapter.achieveLevels++;
			}
			if (level.isMain) {
				chapter.achieveMainLine &= level.star > 0;
			}
		}
	}

	/// <summary>
	/// 设置关卡星星数量,,,单机战斗结束后，调这个，会向服务器发送
	/// </summary>
	public void SetLevelStarToServer(int star)
	{
		if (null != currentLevel) 
		{
			if (currentLevel.star < star) 
			{
				NetSystem.Instance.helper.SetLevelStar (currentLevel.id, star);
			} 
		}
	}

	/// <summary>
	/// Queries the level star.
	/// </summary>
	/// <returns>The level star.</returns>
	/// <param name="levelID">Level I.</param>
	public int QueryLevelStar(string levelId)
	{
		int ret = 0;
		for (int i = 0; i < currentChapter.levelList.Count; ++i) {
			if (levelId.Equals (currentChapter.levelList [i].id)) {
				ret = currentChapter.levelList [i].star;
			}
		}

		return ret;
	}

	/// <summary>
	/// Sets the select chapter.
	/// </summary>
	/// <param name="chapterId">Chapter identifier.</param>
	public void SetSelectChapter(string chapterId)
	{
		currentChapter = GetChapterInfo (chapterId);
		NetSystem.Instance.helper.RequestOneChapter (chapterId);
	}

	/// <summary>
	/// Sets the select level.
	/// </summary>
	/// <param name="levelId">Level identifier.</param>
	public void SetSelectLevel (string levelId)
	{
		for (int i = 0; i < currentChapter.levelList.Count; ++i) {
			if (levelId.Equals (currentChapter.levelList [i].id)) {
				currentLevel = currentChapter.levelList [i];
			}
		}

		if (currentLevel == null) {
			Debug.LogError ("获取当前关卡失败:" + levelId);
		}
	}

	/// <summary>
	/// Bes the unlock chapter.
	/// </summary>
	/// <returns><c>true</c>, if unlock chapter was been, <c>false</c> otherwise.</returns>
	/// <param name="chapterId">Chapter identifier.</param>
	public bool BeUnlockChapter(string chapterId)
	{
		var chapter = GetChapterInfo (chapterId);
		if (null != chapter) {
			return chapter.unLock;
		}
		return false;
	}


}

