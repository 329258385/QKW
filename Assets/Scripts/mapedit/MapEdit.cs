using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Solarmax;
using System.Text;

/// <summary>
/// 服务器用表
/// </summary>
public class MatchTable
{
	/// <summary>
	/// 地图id
	/// </summary>
	/// <value>The identifier.</value>
	public string id { get; set; }

	/// <summary>
	/// 玩家数量
	/// </summary>
	/// <value>The players.</value>
	public int players {get;set; }

	/// <summary>
	/// 横版竖版
	/// </summary>
	/// <value><c>true</c> if vertical; otherwise, <c>false</c>.</value>
	public bool vertical { get; set; }
}

public class MapEdit : MonoBehaviour {

	/// <summary>
	/// 当前ui摄像机
	/// </summary>
	public Camera uiCamera;

	/// <summary>
	/// The root.
	/// </summary>
	public GameObject root;

	/// <summary>
	/// 属性菜单
	/// </summary>
	public GameObject menuRoot;

	/// <summary>
	/// 右键菜单
	/// </summary>
	public GameObject rightMenu;

	/// <summary>
	/// The node attribute.
	/// </summary>
	public NodeAttribute nodeAttribute;

	/// <summary>
	/// 地图名字
	/// </summary>
	public UIInput	mapName;

	/// <summary>
	/// 是否是竖屏
	/// </summary>
	public UIToggle vertical;

	/// <summary>
	/// 玩家人数
	/// </summary>
	public UIInput playerCount;

	/// <summary>
	/// 地图播放的音乐
	/// </summary>
	public UIInput mapAudio;

	/// <summary>
	/// 位移
	/// </summary>
	public TweenPosition tweener;

	char ch;

	/// <summary>
	/// 地图列表
	/// </summary>
	public UIScrollView view;

    /// <summary>
    /// 建筑物
    /// </summary>
    public UIScrollView bulidsView;

	/// <summary>
	/// 地图排列
	/// </summary>
	public UIGrid	grid;


    /// <summary>
    /// 地图排列
    /// </summary>
    public UIGrid bulidsgrid;


	/// <summary>
	/// 地图节点
	/// </summary>
	public GameObject itemRoot;

	/// <summary>
	/// tips
	/// </summary>
	public GameObject tips;

	/// <summary>
	/// The item list.
	/// </summary>
	List<GameObject> itemList = new List<GameObject>();

	// Use this for initialization
	void Start () {
		// 启动Framework

		Framework.Instance.SetWritableRootDir (Application.temporaryCachePath);
		Framework.Instance.SetStreamAssetsRootDir (Application.streamingAssetsPath);
		LoggerSystem.Instance.SetConsoleLogger (new Solarmax.Logger (UnityEngine.Debug.Log));

		// 初始化
		if (Framework.Instance.Init())
		{
			LoggerSystem.Instance.Info("系统启动！");
		}
		else
		{
			LoggerSystem.Instance.Error("系统启动失败！");
		}

		ch = 'A';
//		TableMagr.Get().Initialize(true);
		BattleSystem.Instance.battleData.mapEdit = true;
//		BattleSystem.Instance.Init ();


        bulidsgrid.Reposition();
        bulidsView.ResetPosition();
		CreateItemList ();

		NewMap ();
		AssetManager.Get ().LoadBattleResources ();
	}

	/// <summary>
	/// 创建地图节点
	/// </summary>
	void CreateItemList()
	{
		float rate = view.verticalScrollBar.value;

		Dictionary<string, MapConfig> mapDict = MapConfigProvider.Instance.GetAllData ();

		itemList.ForEach (g=>GameObject.Destroy(g));
		itemList.Clear ();

		foreach (var map in mapDict)
		{

			GameObject go = GameObject.Instantiate (itemRoot) as GameObject;

			go.transform.SetParent (grid.gameObject.transform);
			go.transform.localScale = Vector3.one;
			go.SetActive (true);
			UIEventListener listener = go.GetComponentInChildren<UIEventListener> ();
			listener.onClick = OnClickButton;
			UILabel lab = go.GetComponentInChildren<UILabel> ();
			lab.text = (string)map.Key;

			itemList.Add (go);
		}

		grid.Reposition ();
		view.ResetPosition ();

		view.verticalScrollBar.value = rate;
	}

	void OnClickButton(GameObject go)
	{
		UILabel lable = go.GetComponentInChildren<UILabel> ();

		string mapId = lable.text;

		if (mapId == BattleSystem.Instance.battleData.matchId)
			return;

		BattleSystem.Instance.Reset ();
		BattleSystem.Instance.battleData.matchId = mapId;
		BattleSystem.Instance.sceneManager.CreateScene (null,true);

		mapName.value	 	= BattleSystem.Instance.battleData.matchId;
		playerCount.value 	= BattleSystem.Instance.battleData.currentPlayers.ToString ();

		MapConfig mapConfig = MapConfigProvider.Instance.GetData (mapId);
		mapAudio.value = mapConfig.audio;
		vertical.value = true;

		// 更新node的公转
		List<Node> nodes = BattleSystem.Instance.sceneManager.nodeManager.GetUsefulNodeList ();
		foreach (var node in nodes) {
			nodeAttribute.UpdateOrbitLine (node, node.revoType, node.revoParam1, node.revoParam2);
		}
	}
	
	// Update is called once per frame
	void Update () {
		//删除键
		if(Input.GetKey(KeyCode.LeftCommand)){
			if (Input.GetKey (KeyCode.Backspace) || Input.GetKey(KeyCode.Delete)) {
				if (NodeAttribute.current == null)
					return;
				BattleSystem.Instance.sceneManager.RemoveNode (NodeAttribute.current.tag);
			}
		}

		#if !SERVER
		EffectManager.Get ().Tick (Time.frameCount, Time.time);
		#endif
	}

	public void HideUI()
	{
		menuRoot.SetActive (!menuRoot.activeSelf);
	}

	/// <summary>
	/// 创建建筑
	/// </summary>
	/// <param name="name">Name.</param>
	public void SelectNode(UILabel lable)
	{
		int kind = 0;

		switch(lable.text)
		{
		case "star":
			kind = 1;
			break;
		case "teleport":
			kind = 3;
			break;
		case "tower":
			kind = 4;
			break;
		case "castle":
			kind = 2;
			break;
		case "barrier":
			kind = 5;
			break;
		case "master":
			kind = 7;
			break;
		case "defense":
			kind = 8;
			break;
		case "power":
			kind = 9;
			break;
        case "BlackHole":
            kind = 10;
            break;
		default:
			break;
		}

		if (kind == 0)
			return;

		Vector3 position = Input.mousePosition;
		position.z = 0;
		position = uiCamera.ScreenToWorldPoint (position);

		while (true) {

			if (ch > 'z')
				ch = 'A';

			Node temp = BattleSystem.Instance.sceneManager.GetNode (ch.ToString());

			if (temp == null)
				break;

			ch++;
		}

        Node node = null;
        MapNodeConfig mapnode = MapNodeConfigProvider.Instance.GetData(lable.text);
        if( mapnode != null )
		    node     = BattleSystem.Instance.sceneManager.AddNode (ch.ToString(), kind, position.x, position.y, mapnode.perfab );

		if (node != null)
			nodeAttribute.SetNode (node);

		UpdateMenu (position);

		rightMenu.SetActive (false);
	}

	public void UpdateMenu(Vector3 position)
	{
		//Vector3 lp = menuRoot.transform.localPosition;

//		if (position.y >= 0) {
//			menuRoot.transform.localPosition = new Vector3 (-400, -600, 0);
//		} else {
//			menuRoot.transform.localPosition = new Vector3 (400, 600, 0);
//		}
        menuRoot.transform.localPosition = new Vector3(830, -240, 0);
	}

	/// <summary>
	/// 更改地图名字
	/// </summary>
	public void ChangeMapName()
	{
		BattleSystem.Instance.battleData.matchId = mapName.value;
	}

	/// <summary>
	/// 更改玩家数量
	/// </summary>
	public void ChangePlayerCount()
	{
		if (string.IsNullOrEmpty (playerCount.value))
			BattleSystem.Instance.battleData.currentPlayers = 0;
		else
			BattleSystem.Instance.battleData.currentPlayers = int.Parse (playerCount.value);
	}

	public void ChangeMapAudio()
	{
		BattleSystem.Instance.battleData.mapAudio = mapAudio.value;
	}

	/// <summary>
	/// 更改是否横屏
	/// </summary>
	public void ChangeVertical()
	{
		//BattleManager.Get ().vertical = vertical.value;
		BattleSystem.Instance.battleData.vertical = true;
	}

	/// <summary>
	/// 创建一个空地图
	/// </summary>
	public void NewMap()
	{
		BattleSystem.Instance.Reset ();

		//创建新的表
		BattleSystem.Instance.battleData.currentTable = new MapConfig ();

		mapName.value	 	= BattleSystem.Instance.battleData.matchId;
		playerCount.value 	= BattleSystem.Instance.battleData.currentPlayers.ToString ();
		mapAudio.value = BattleSystem.Instance.battleData.mapAudio;
		//vertical.value 		= BattleManager.Get ().vertical;
		vertical.value = true;

		ch = 'A';
	}

	/// <summary>
	/// 删除当前地图
	/// </summary>
	public void DeleteMap()
	{
		// 将map从源数据表中删除
		MapConfig map = BattleSystem.Instance.battleData.currentTable;

		// 先从map表中删除
		var maps = MapConfigProvider.Instance.GetAllData();
		maps.Remove (map.id);
		// 删除build里面的项目
		var builds = MapBuildingConfigProvider.Instance.GetAllData ();
		foreach (var b in map.builds) {
			if (builds.ContainsKey (b.id))
				builds.Remove (b.id);
		}
		// 删除player里面的项目
		var players = MapPlayerConfigProvider.Instance.GetAllData ();
		foreach (var p in map.players) {
			if (null != p && players.ContainsKey (p.id))
				players.Remove (p.id);
		}

		// 清除，刷新列表
		BattleSystem.Instance.Reset ();

		CreateItemList ();

		GameObject first = itemList [0];
		UIEventListener listener = first.GetComponentInChildren<UIEventListener> ();
		listener.onClick (first);
	}

	/// <summary>
	/// 显示tips
	/// </summary>
	/// <param name="message">Message.</param>
	public void ShowTips(string message)
	{
		UISprite sprite = tips.GetComponentInChildren<UISprite> ();
		sprite.alpha = 0f;
		sprite.color = Color.white;

		UILabel lable = tips.GetComponentInChildren<UILabel> ();
		lable.text = message;

		tips.transform.localPosition = Vector3.zero;
		tips.SetActive (true);

		TweenPosition tween = TweenPosition.Begin (tips, 0.5f, new Vector3(0, 200, 0));
		TweenAlpha.Begin (tips, 0.5f, 1f);

		EventDelegate.Add (tween.onFinished, ()=>{
			TweenAlpha.Begin(tips, 1.5f, 0f);
		}, true);
	}

	/// <summary>
	/// 隐藏面板
	/// </summary>
	public void HidePanel()
	{
		TweenPosition.Begin(tweener.gameObject, 0.5f, new Vector3 (600, 0, 0));
	}

	/// <summary>
	/// 显示面板
	/// </summary>
	public void ShowPanel()
	{
		TweenPosition.Begin(tweener.gameObject, 0.5f, new Vector3 (0, 0, 0));
	}

	public void OnBackBtnClick ()
	{
		BattleSystem.Instance.Reset ();
	}

	/// <summary>
	/// 存储地图
	/// </summary>
	public void SaveMap()
	{
		if (string.IsNullOrEmpty (BattleSystem.Instance.battleData.matchId)) 
		{
			ShowTips ("地图名字不可为空");
			return;
		}

		if (BattleSystem.Instance.battleData.currentPlayers == 0) 
		{
			ShowTips ("参与玩家人数不得少于1");
			return;
		}

		//添加队伍出生点检测
		List<int> camptions = new List<int>();
		foreach(var item in BattleSystem.Instance.battleData.currentTable.players)
		{
			if (null != item && 0 != item.camption) 
			{
				if( item.camption != camptions.Find (n => n == item.camption) )
					camptions.Add (item.camption);
			}
		}
		if (camptions.Count != BattleSystem.Instance.battleData.currentPlayers) 
		{
			ShowTips ("参与玩家人数与出生队伍不一致");
			return;
		}

		//把当前地图生成json表
		MapConfig map = MapConfigProvider.Instance.GetData (BattleSystem.Instance.battleData.matchId);
	
		if (map != null && map != BattleSystem.Instance.battleData.currentTable) 
		{
			ShowTips ("地图名字重复");
			return;
		}

		// CONVERT by fangjun
		/*if (BattleSystem.Instance.battleData.currentTable.vertical) {
			ConvertMap.ConvertOne (BattleSystem.Instance.battleData.currentTable);
		}*/


		// 对于地图，需要分成三个表进行分割，此时，需要对MapConfig分割存储
		Dictionary<string, MapBuildingConfig> allBuilding = MapBuildingConfigProvider.Instance.GetAllData ();
		List<MapBuildingConfig> allBuildingList = new List<MapBuildingConfig> ();
		Dictionary<string, MapPlayerConfig> allPlayer = MapPlayerConfigProvider.Instance.GetAllData ();
		List<MapPlayerConfig> allPlayerList = new List<MapPlayerConfig> ();
		Dictionary<string, MapConfig> allMap = MapConfigProvider.Instance.GetAllData();
		List<MapConfig> allMapList = new List<MapConfig> ();

		// 1，加入地图
		if (map == null) {
			map = BattleSystem.Instance.battleData.currentTable.Clone() as MapConfig;
			map.id = BattleSystem.Instance.battleData.matchId;
			allMap.Add (map.id, map);
		}
		foreach (var m in allMap) {
			allMapList.Add (m.Value);
		}

		// 2，加入建筑物
		// 2.1 先删除当前的地图相关的数据
		List<string> buildingKeys = new List<string>();
		buildingKeys.AddRange (allBuilding.Keys);
		string deleteKey = map.id + "_";
		foreach (var key in buildingKeys) {
			if (key.StartsWith (deleteKey)) {
				allBuilding.Remove (key);
			}
		}
		// 2.2 加入当前的
		map.buildingIds = string.Empty;
		for (int i = 0; i < map.builds.Count; ++i) {
			MapBuildingConfig mbc = map.builds [i];
			mbc.id = string.Format ("{0}_{1}", map.id, i);
			if (!allBuilding.ContainsKey (mbc.id))
			{
				allBuilding.Add (mbc.id, mbc);
			}

			// 加入地图的id索引
			if (string.IsNullOrEmpty (map.buildingIds))
				map.buildingIds = i.ToString ();
			else
				map.buildingIds = string.Format ("{0},{1}", map.buildingIds, i);
			
		}
		foreach (var bc in allBuilding) {
			allBuildingList.Add (bc.Value);
		}
		allBuildingList.Sort ((arg1, arg2) => {
			return arg1.id.CompareTo(arg2.id);
		});

		// 3，加入玩家
		// 3.1 删除当前的玩家
		List<string> playerKeys = new List<string>();
		playerKeys.AddRange (allPlayer.Keys);
		foreach (var key in playerKeys) {
			if (key.StartsWith (deleteKey)) {
				allPlayer.Remove (key);
			}
		}
		// 3.2 加入当前地图玩家
		map.playerIds = string.Empty;
		for (int i = 0; i < map.players.Count; ++i) {
			MapPlayerConfig mpc = map.players [i];
			mpc.id = string.Format ("{0}_{1}", map.id, i);
			if (!allPlayer.ContainsKey (mpc.id))
			{
				allPlayer.Add (mpc.id, mpc);
			}

			// 加入玩家id索引
			if (string.IsNullOrEmpty (map.playerIds))
				map.playerIds = i.ToString ();
			else
				map.playerIds = string.Format ("{0},{1}", map.playerIds, i);
		}
		foreach (var pc in allPlayer) {
			allPlayerList.Add (pc.Value);
		}

		allPlayerList.Sort ((arg1, arg2) => {
			return arg1.id.CompareTo(arg2.id);
		});

        // 将地图，建筑物，玩家三张表格分别存储起来
        string mapTxt       = GenerateText<MapConfig>(allMapList);
        SaveText(mapTxt,        "map");
        string buildingTxt  = GenerateText<MapBuildingConfig>(allBuildingList);
        SaveText(buildingTxt,   "mapbuilding");
        string playerTxt    = GenerateText<MapPlayerConfig>(allPlayerList);
        SaveText(playerTxt,     "mapplayer");


        //reload map
        DataProviderSystem.Instance.Destroy();
		DataProviderSystem.Instance.Init ();

		//刷新列表
		CreateItemList ();

		BattleSystem.Instance.battleData.currentTable = MapConfigProvider.Instance.GetData(BattleSystem.Instance.battleData.matchId);
		//提示tip
		ShowTips("生成map.txt, match.json成功");
	}
		
	/// <summary>
	/// 匹配地图
	/// </summary>
	/// <param name="table">Table.</param>
	void CreateMatchJson(List<MapConfig> allMapList)
	{
		
	}

	/// <summary>
	/// 创建json文件
	/// </summary>
	/// <param name="json">Json.</param>
	void SaveText(string text, string name)
	{
		string oldPath = string.Format ("{0}/StreamingAssets/data/{1}.txt", Application.dataPath, name);
		string newPath = string.Format ("{0}/StreamingAssets/data/{1}_bak.txt", Application.dataPath, name);

        /*
		if (!Application.isEditor) {
			//APP模式
			oldPath = string.Format ("{0}/{1}.txt", Application.persistentDataPath, name);
			newPath = string.Format ("{0}/{1}_bak.txt", Application.persistentDataPath, name);
		}
        */
		//备份文件
		File.Move (oldPath, newPath);

		using(FileStream stream = new FileStream(oldPath, FileMode.CreateNew))
		{
			byte[] buf = System.Text.Encoding.Default.GetBytes (text);
			stream.Write (buf, 0, buf.Length);
			stream.Flush ();
			stream.Close ();
		}

		//删除备份文件
		File.Delete (newPath);
	}

	/// <summary>
	/// 上传文件
	/// </summary>
	public void UploadFile ()
	{
		
	}


    public static string GenerateText<T>(List<T> list)
    {
        if (list.Count == 0)
            return string.Empty;

        T data = list[0];
        System.Type type = data.GetType();

        // write title
        StringBuilder sb = new StringBuilder();
        sb.Append("#");
        foreach (var prop in type.GetProperties())
        {
            sb.Append(prop.Name);
            sb.Append('\t');
        }
        sb.Append('\r');
        sb.Append('\n');
        sb.Append("#");
        foreach (var prop in type.GetProperties())
        {
            sb.Append(prop.PropertyType);
            sb.Append('\t');
        }
        sb.Append('\r');
        sb.Append('\n');

        // write values
        foreach (var i in list)
        {
            foreach (var prop in type.GetProperties())
            {
                sb.Append(prop.GetValue(i, null));
                sb.Append('\t');
            }

            sb.Append('\r');
            sb.Append('\n');
        }

        string text = sb.ToString();
        return text;
    }
}
