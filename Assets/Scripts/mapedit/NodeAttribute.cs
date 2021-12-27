using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Solarmax;

public class NodeAttribute : MonoBehaviour {

	public UIInput 		inTag;
	public UIPopupList 	popSize;
	public UIPopupList 	popCamp;
	public UIInput 		inShipNum;
	public UIInput 		inX;
	public UIInput 		inY;
	public UISprite 	sprite;
	public GameObject	barrierRoot;
    public GameObject   angleRoot;
	public UIInput 		inBarrier;
	public UIPopupList	popBarrier;
	public UIPopupList	popOrbit;
	public UIInput 		orbitX;
	public UIInput 		orbitY;
	public UIToggle 	orbitCW;
    public UIInput      angle;

	public static Node current;
	public float clickTime = 0f;

	/// <summary>
	/// 获取大小
	/// </summary>
	/// <returns>The scale.</returns>
	/// <param name="type">Type.</param>
	/// <param name="scale">Scale.</param>
	public static int GetSize(int type, float scale)
	{
		List<MapNodeConfig> list = MapNodeConfigProvider.Instance.GetAllData ();
		foreach(MapNodeConfig node in list)
		{
			if (node.typeEnum != type)
				continue;

			if (node.size != scale)
				continue;

			return node.sizeType;

		}
		return 0;
	}


    /// <summary>
	/// 切换队伍
	/// </summary>
	/// <param name="node">Node.</param>
	public void ChangeTeam()
    {
        if (current == null)
            return;

        TEAM team           = (TEAM)Enum.Parse(typeof(TEAM), popCamp.value);
        current.currentTeam = BattleSystem.Instance.sceneManager.teamManager.GetTeam(team);
        MapConfig table     = BattleSystem.Instance.battleData.currentTable;
    }


    /// <summary>
	/// 修改大小
	/// </summary>
	public void ChangeSize()
    {
        if (current == null)
            return;

        int size        = int.Parse(popSize.value);
        float scale     = GetScale((int)current.nodeType, size);
        current.SetScale(scale);

        MapConfig table = BattleSystem.Instance.battleData.currentTable;
        MapBuildingConfig item = table.builds.Find(n => n.tag == current.tag);
        item.size = size;
    }

    /// <summary>
	/// 修改位置
	/// </summary>
	public void ChangePosition()
    {
        if (current == null)
            return;


        float x, y;
        x = y = 0f;
        if (inX.value == "-" || inY.value == "-")
            return;

        if (!string.IsNullOrEmpty(inX.value))
            x = float.Parse(inX.value);

        if (!string.IsNullOrEmpty(inY.value))
            y = float.Parse(inY.value);

        current.SetPosition(x, y);
        MapConfig table        = BattleSystem.Instance.battleData.currentTable;
        MapBuildingConfig item = table.builds.Find(n => n.tag == current.tag);
        item.x = x;
        item.y = y;

        if (current.nodeType == NodeType.Barrier)
        {
            RefreshBarrierLine();
        }
    }


    public void RefreshBarrierLine()
    {
        //重新创建障碍物
        BattleSystem.Instance.sceneManager.nodeManager.DestroyBarrierLineNode();
        BattleSystem.Instance.sceneManager.CreateLines(BattleSystem.Instance.battleData.currentTable.lines);
    }


    /// <summary>
	/// 修改公转中心点
	/// </summary>
	public void ChangeOrbitXYAndType()
    {
        if (current == null)
            return;
        //设置动态属性
        RevolutionType type = (RevolutionType)Enum.Parse(typeof(RevolutionType), popOrbit.value);

        // check valid
        string f1 = orbitX.value;
        string f2 = orbitY.value;
        if (!Converter.CanConvertVector3D(f1) || !Converter.CanConvertVector3D(f2))
        {
            Debug.Log("Ellipse Revolution Param error!");
            return;
        }

        current.SetRevolution((int)type, f1, f2, orbitCW.value);

        //设置静态属性
        MapBuildingConfig bi = BattleSystem.Instance.battleData.currentTable.builds.Find(n => n.tag == current.tag);

        bi.orbit = (int)type;
        bi.orbitParam1 = f1;
        bi.orbitParam2 = f2;
        bi.orbitClockWise = orbitCW.value;

        UpdateOrbitLine(current, type, f1, f2);
    }

    /// <summary>
    /// 获取缩放
    /// </summary>
    /// <returns>The scale.</returns>
    /// <param name="type">Type.</param>
    /// <param name="size">Size.</param>
    static public float GetScale(int type, int size)
	{
		List<MapNodeConfig> list = MapNodeConfigProvider.Instance.GetAllData ();

		foreach(MapNodeConfig node in list)
		{
			if (node.typeEnum != type)
				continue;

			if (node.sizeType != size)
				continue;

			return node.size;

		}

		return 0;
	}

	/// <summary>
	/// 设置星球
	/// </summary>
	public void SetNode(Node node)
	{
		if (node == null)
			return;

		
		current = null;

		float scale = node.GetScale ();

        int size = GetSize((int)node.nodeType, scale);

		List<MapNodeConfig> list = MapNodeConfigProvider.Instance.GetAllData ();
		popSize.Clear ();
		foreach(MapNodeConfig config in list)
		{
            if (config.typeEnum != (int)node.nodeType)
				continue;

			popSize.AddItem (config.sizeType.ToString());
			//break;
		}

		inTag.value = node.tag;
		popCamp.value = node.team.ToString ();
		//popCamp.transform.localPosition = new Vector3 (-1000, 300, 0);
		popSize.value = size.ToString ();
		inX.value = node.GetPosition ().x.ToString();
		inY.value = node.GetPosition ().y.ToString();
		inShipNum.value = node.GetShipCount ((int)node.team).ToString();

		//如果是障碍物的话
		if (node is BarrierNode) {
			sprite.height = 540;
			barrierRoot.SetActive (true);

			BoxCollider collider = sprite.GetComponent<BoxCollider> ();

			collider.size = new Vector3 (300, 540, 1f);
			collider.center = new Vector3 (0, -540 >> 1, 0f);

			MapConfig table =  BattleSystem.Instance.battleData.currentTable;

			popBarrier.Clear ();
			if (table != null && table.lines != null) {

				foreach (List<string> item in table.lines) {

					if (item [0] != node.tag)
						continue;

					popBarrier.AddItem (item[1]);

				}
			}

			if (popBarrier.itemData != null && popBarrier.items.Count >= 1) {
				popBarrier.value = popBarrier.items [0];
			} else {
				popBarrier.value = string.Empty;
			}

			inBarrier.value = string.Empty;
		} 
        else {
			sprite.height = 360;
			barrierRoot.SetActive (false);
            angleRoot.SetActive(false);
			BoxCollider collider    = sprite.GetComponent<BoxCollider> ();
			collider.size           = new Vector3 (300, 360, 1f);
			collider.center         = new Vector3 (0, -360 >> 1, 0f);

		}


		//环绕
		string[] types = Enum.GetNames(typeof(RevolutionType));

		popOrbit.Clear ();

		foreach(string t in types){
			popOrbit.AddItem (t);
		}

		MapBuildingConfig bi = BattleSystem.Instance.battleData.currentTable.builds.Find (n=>n.tag == node.tag);

		popOrbit.value = Enum.GetName(typeof(RevolutionType), (RevolutionType)bi.orbit);
		orbitX.value = bi.orbitParam1;
		orbitY.value = bi.orbitParam2;
		orbitCW.value = bi.orbitClockWise;
		current = node;
		clickTime = Time.realtimeSinceStartup;
	}

	/// <summary>
	/// 添加障碍物
	/// </summary>
	public void AddBarrier()
	{
		if (string.IsNullOrEmpty (inBarrier.value))
			return;

		if (current.tag == inBarrier.value)
			return;

		MapConfig table =  BattleSystem.Instance.battleData.currentTable;
		if (table == null)
			return;

		MapBuildingConfig bitem = table.builds.Find (b=>b.tag == inBarrier.value);

		if (bitem == null)
			return;

		if (bitem.type != "barrier")
			return;

		if (table.lines == null)
			table.lines = new List<List<string>> ();

		List<string> item = table.lines.Find (l=>l[0] == current.tag && l[1]==inBarrier.value);
		if (item != null)
			return;

		item = new List<string> ();
		item.Add (current.tag);
		item.Add (inBarrier.value);
		table.lines.Add (item);
        if (string.IsNullOrEmpty(table.linetags))
            table.linetags = current.tag + "," + inBarrier.value;
        else
            table.linetags = table.linetags + ";" + current.tag + "," + inBarrier.value;

        BattleSystem.Instance.sceneManager.AddBarrierLines(current.tag, inBarrier.value);
		popBarrier.AddItem (inBarrier.value);
		popBarrier.value = inBarrier.value;
		inBarrier.value = string.Empty;
	}

	/// <summary>
	/// 删除当前障碍物
	/// </summary>
	public void DeleteCurrentBarrier()
	{
		if (string.IsNullOrEmpty (popBarrier.value))
			return;

		string tag = popBarrier.value;

		popBarrier.RemoveItem (tag);

		if (popBarrier.itemData != null && popBarrier.items.Count >= 1) {
			popBarrier.value = popBarrier.items [0];
		} else {
			popBarrier.value = string.Empty;
		}

		MapConfig table =  BattleSystem.Instance.battleData.currentTable;

		if (table.lines != null) {

			List<string> item = table.lines.Find (l=>l[0] == current.tag && l[1]==tag);

			if (item != null) {
				table.lines.Remove (item);
			}
		}
		// 此处需要更新linetags
		string tags = string.Empty;
		foreach (List<string> pair in table.lines) {
			if (tags.Length > 0)
				tags += ";";
			tags += pair [0] + "," + pair [1];
		}
		table.linetags = tags;
	}

	/// <summary>
	/// 更新位置
	/// </summary>
	/// <param name="node">Node.</param>
	public void UpdatePosition(Node node)
	{
		inX.value = node.GetPosition ().x.ToString();
		inY.value = node.GetPosition ().y.ToString();

		//设置动态属性
		RevolutionType type = (RevolutionType)Enum.Parse(typeof(RevolutionType), popOrbit.value);

		UpdateOrbitLine (node, type, orbitX.value, orbitY.value);
	}


	/// <summary>
	/// 刷新圈
	/// </summary>
	/// <param name="type">Type.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public void UpdateOrbitLine(Node node, RevolutionType type, string p1, string p2)
	{
		if (type == RevolutionType.RT_None)
			return;

		Vector3 pos1 = Converter.ConvertVector3D (p1);
		Vector3 pos2 = Converter.ConvertVector3D (p2);

		//画圈
		LineRenderer render = node.GetGO ().GetComponent<LineRenderer> ();
		if (render == null) {
			render = node.GetGO ().AddComponent<LineRenderer> ();
		}

		render.material = (Material) (Resources.Load ("UI/MapShowLineM"));

		Color c = new Color32 (0xcc, 0xcc, 0xcc, 0x66);
		render.startWidth = render.endWidth = 0.02f;
		render.startColor = render.endColor = c;

		List<Vector3> pos = new List<Vector3>();
		if (type == RevolutionType.RT_Circular) {
			pos = CalculatePositions (node.GetPosition(), new Vector3(pos1.x, pos1.y, -1), CalcDragPoint(type));

		} else if (type == RevolutionType.RT_Ellipse) {
			pos = CalculatePositionsEllipse (node.GetPosition(), new Vector3(pos1.x, pos1.y, -1), new Vector3(pos2.x, pos2.y, -1), CalcDragPoint(type));
		}

		render.useWorldSpace = true;
		render.positionCount = pos.Count;
		render.SetPositions (pos.ToArray ());
	}

	/// <summary>
	/// 计算点数
	/// </summary>
	/// <returns>The drag point.</returns>
	/// <param name="type">Type.</param>
	int CalcDragPoint(RevolutionType type)
	{
		switch (type) {
		case RevolutionType.RT_Circular:
			return 128;
		case RevolutionType.RT_Quadrilateral:
			return 4;
		case RevolutionType.RT_Triangle:
			return 3;
		case RevolutionType.RT_Ellipse:
			return 128;
		default:
			return 0;
		}
	}

	List<Vector3> CalculatePositions(Vector3 basePos, Vector3 centerPos, int count)
	{
		List<Vector3> positions = new List<Vector3> ();

		basePos.z = -1f;

		basePos -= centerPos; // 首先获得相对位置

		positions.Add(basePos);

		// 使用四元数连续旋转
		float perAngle = 360.0f / count;
		Quaternion perQ = Quaternion.AngleAxis(perAngle, Vector3.forward);
		Vector3 temp;
		for (int i = 1; i < count; ++i)
		{
			temp = perQ * positions [i - 1];
			positions.Add(temp);
		}

		positions.Add (basePos);

		// 计算反相对后的实际位置
		for (int i = 0; i < positions.Count; ++i)
		{
			positions [i] += centerPos;
		}

		return positions;
	}

	private List<Vector3> CalculatePositionsEllipse(Vector3 basePos, Vector3 pos1, Vector3 pos2, int count)
	{
		basePos.z = -1;
		pos1.z = -1;
		pos2.z = -1;

		List<Vector3> positions = new List<Vector3>();

		Vector3 centerPos = (pos1 + pos2) / 2;

		float pf1pf2 = Vector3.Distance (basePos, pos1) + Vector3.Distance (basePos, pos2); // 2a
		float f1f2 = Vector3.Distance (pos1, pos2); //2c
		float ellipseA = pf1pf2 / 2; // a
		float ellipseC = f1f2 / 2; // c
		float ellipseB = Mathf.Sqrt (Mathf.Pow (ellipseA, 2) - Mathf.Pow (ellipseC, 2));

		float perAngle = 2 * Mathf.PI / count;
		for (int i = 0; i <= count; ++i) {
			float angle = perAngle * i;
			Vector3 v = Vector3.zero;
			v.x = ellipseA * Mathf.Cos (angle);
			v.y = ellipseB * Mathf.Sin (angle);
			v.z = basePos.z;

			v += centerPos;

			positions.Add (v);
		}

		return positions;
	}

	Vector2 modify;

	void OnPress(bool isPressed)
	{
		if (isPressed && enabled)
		{
			modify = gameObject.transform.position - UICamera.currentCamera.ScreenToWorldPoint (Input.mousePosition);
		} 
	}

	void OnDrag(Vector2 delta)
	{
		if (!enabled)
			return;

		Vector2 pos = UICamera.currentCamera.ScreenToWorldPoint (Input.mousePosition);
		gameObject.transform.position = modify + pos;
	}
}
