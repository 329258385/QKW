using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Plugin;
using Solarmax;





/// <summary>
/// 建筑物管理
/// </summary>
public partial class NodeManager : Lifecycle2
{
    /// <summary>
    /// 建筑检索
    /// </summary>
    Dictionary<string, Node> nodeMap;

    /// <summary>
    /// 有用建筑物
    /// </summary>
    private List<Node> usefulNodeList;

    /// <summary>
    /// 无用建筑物
    /// </summary>
    private List<Node> barrierNodeList;

    /// <summary>
    /// 场景管理
    /// </summary>
    /// <value>The scene manager.</value>
    public SceneManager sceneManager { get; set; }

    /// <summary>
    /// 星球在世界的坐标缩放
    /// </summary>
    private Vector2 positionScale = new Vector2(200, 200);

    /// <summary>sss
    /// 初始化
    /// </summary>
    public NodeManager(SceneManager magr)
    {
        sceneManager = magr;
        nodeMap = new Dictionary<string, Node>();
        usefulNodeList = new List<Node>();
        barrierNodeList = new List<Node>();
    }



    public bool Init()
    {
        return true;
    }

    public void Tick(int frame, float interval)
    {
        for (int i = 0, max = usefulNodeList.Count; i < max; ++i)
        {
            Node node = usefulNodeList[i];
            node.Tick(frame, interval);
        }
    }

    public void Destroy()
    {
        for (int i = 0, max = usefulNodeList.Count; i < max; ++i) {
            Node node = usefulNodeList[i];
            node.Destroy();
        }
        for (int i = 0, max = barrierNodeList.Count; i < max; ++i) {
            Node node = barrierNodeList[i];
            node.Destroy();
        }

        nodeMap.Clear();
        usefulNodeList.Clear();
        barrierNodeList.Clear();
        BarriersKY.Clear();
    }

    /// <summary>
    /// 添加一个节点
    /// </summary>
    public Node AddNode(
        int team, 			    // 队伍
        int kind, 			    // 星球类型
        float x, 			    // 坐标x
        float y, 			    // 坐标y
        float scale, 		    // 缩放
        string tag, 		    // 星球标记
        bool mapEdit,
        string orbitParam1,     // 环绕点坐标p1
        string orbitParam2,	    // 环绕点坐标p2
        bool orbitclockwise,    // 顺时针环绕
        float nodesize,         // 节点半径大小
        string perfab,
        string skills,
        float fAngle)
    {
        if (nodeMap.ContainsKey(tag))
            return null;

        Vector3 dir = Vector3.zero;
        dir.z = fAngle;
        Node node = CreateNode((NodeType)kind, tag, perfab);
        node.nodeManager = this;
        node.hp = 0;
        node.SetAttribute(NodeAttr.HpMax, 100);
        node.SetAttribute(NodeAttr.Poplation, 0);
        node.produceNum = 0;
        node.produceFrame = 0;
        node.initTEAM = (TEAM)team;
        node.SetScale(scale);
        if (mapEdit)
            node.SetPosition(x, y, 0);
        else
            node.SetPosition(x * positionScale.x, -10f, y * positionScale.y);
        node.SetRevolution(0, orbitParam1, orbitParam2, orbitclockwise);
        node.SetRealTeam(sceneManager.teamManager.GetTeam((TEAM)team), true);

        node.SetAttribute(NodeAttr.AttackRange, 0);
        node.SetAttribute(NodeAttr.AttackSpeed, 0);
        node.SetAttribute(NodeAttr.AttackPower, 0);
        node.SetNodeSize(nodesize);
        node.SetColor(new Color(1, 0, 0, 1));
        //添加列表
        nodeMap.Add(node.tag, node);

        if (node.nodeType == NodeType.Barrier || node.nodeType == NodeType.BarrierLine)
        {
            barrierNodeList.Add(node);
        }
        else
        {
            usefulNodeList.Add(node);
        }

        return node;
    }

    public Node AddNode(float x, float y, MapBuildingConfig item, MapNodeConfig nodecfg, bool mapEdit)
    {
        if (nodeMap.ContainsKey(item.tag))
            return null;

        Vector3 dir = Vector3.zero;
        dir.z = item.fAngle;
        Node node = CreateNode((NodeType)nodecfg.typeEnum, item.tag, nodecfg.perfab);
        node.nodeManager = this;
        node.hp = 0;
        node.SetAttribute(NodeAttr.HpMax, nodecfg.hp);
        node.SetAttribute(NodeAttr.Poplation, nodecfg.food);
        node.produceNum = nodecfg.createshipnum;
        node.produceFrame = (int)(25 * nodecfg.createship);
        node.SetScale(nodecfg.size);
        if (mapEdit)
            node.SetPosition(x, y, 0);
        else
            node.SetPosition(x * positionScale.x, -10f, y * positionScale.y);

        Team ta = sceneManager.teamManager.GetTeam((TEAM)item.camption);
        node.SetRevolution(item.orbit, item.orbitParam1, item.orbitParam2, item.orbitClockWise);
        node.SetRealTeam(ta, true);
        node.initTEAM = (TEAM)item.camption;
        node.SetAttribute(NodeAttr.AttackRange, nodecfg.attackrange);
        node.SetAttribute(NodeAttr.AttackSpeed, nodecfg.attackspeed);
        node.SetAttribute(NodeAttr.AttackPower, nodecfg.attackpower);
        node.SetNodeSize(nodecfg.nodesize);
		//添加列表
		nodeMap.Add (node.tag, node);
        if (node.nodeType == NodeType.Barrier || node.nodeType == NodeType.BarrierLine) 
        {
			barrierNodeList.Add (node);
		} 
        else {
			usefulNodeList.Add (node);
		}

        return node;
	}


    public Node CreateAndModifyNode(NodeType type, string tag, int camption, GameObject go )
    {
        if (nodeMap.ContainsKey(tag))
            return null;

        MapNodeConfig nodecfg = MapNodeConfigProvider.Instance.GetDataByType( (int)type );
        if (nodecfg == null)
            return null;

        Node node           = CreateNode(type, tag, "", go );
        node.nodeManager    = this;
        node.hp             = 0;
        node.SetAttribute(NodeAttr.HpMax,       nodecfg.hp);
        node.SetAttribute(NodeAttr.Poplation,   nodecfg.food);
        node.produceNum     = 1;
        node.produceFrame   = (int)(25 * 1);

        Vector3 pos         = go.transform.position;
        node.SetPosition(pos.x, pos.y, pos.z);

        Team ta             = sceneManager.teamManager.GetTeam((TEAM)camption);
        //node.SetRevolution(item.orbit, item.orbitParam1, item.orbitParam2, item.orbitClockWise);
        node.SetRealTeam(ta, true);
        node.initTEAM        = TEAM.Neutral;
        node.SetAttribute(NodeAttr.AttackRange, nodecfg.attackrange);
        node.SetAttribute(NodeAttr.AttackSpeed, nodecfg.attackspeed);
        node.SetAttribute(NodeAttr.AttackPower, nodecfg.attackpower);
        node.SetNodeSize(nodecfg.size);

        //添加列表
        nodeMap.Add(node.tag, node);
        if (node.nodeType == NodeType.Barrier || node.nodeType == NodeType.BarrierLine)
        {
            barrierNodeList.Add(node);
        }
        else
        {
            usefulNodeList.Add(node);
        }
        return node;
    }


    /// <summary>
    /// 添加星球
    /// </summary>
    public Node AddNode(string tag, int kind, float dx, float dy, float scale, bool mapedit=false, string perfab = "NULL" )
	{
		if (nodeMap.ContainsKey (tag))
			return null;


        Node node           = CreateNode((NodeType)kind, tag, perfab);
		node.nodeManager    = this;
	
		node.SetScale (scale);
        if (mapedit)
            node.SetPosition(dx, dy, 0);
        else
            node.SetPosition(dx * positionScale.x, -10f, dy * positionScale.y);

        //node.SetPosition (dx * positionScale.x, 0, dy * positionScale.y);
		node.currentTeam    = sceneManager.teamManager.GetTeam (TEAM.Neutral);
        if (node.nodeType == NodeType.Castle || node.nodeType == NodeType.Tower) 
        {
            node.SetAttribute(NodeAttr.AttackRange, 5f);
		}

		//添加列表
		nodeMap.Add (node.tag, node);

        if (node.nodeType == NodeType.Barrier || node.nodeType == NodeType.BarrierLine) 
        {
			barrierNodeList.Add (node);
		} 
        else {
			usefulNodeList.Add (node);
		}

		return node;
	}

	/// <summary>
	/// 删除星球
	/// </summary>
	/// <param name="tag">Tag.</param>
	public void RemoveNode(string tag)
	{
		Node node = GetNode (tag);

		if (node == null)
			return;

		node.Destroy ();

		nodeMap.Remove (tag);

        if (node.nodeType == NodeType.Barrier || node.nodeType == NodeType.BarrierLine)
        {
			barrierNodeList.Remove (node);
		} else {
			usefulNodeList.Remove (node);
		}
	}
		
	/// <summary>
	/// 创建一个对象
	/// </summary>
	/// <returns>The node.</returns>
	/// <param name="nodeType">Node type.</param>
	Node CreateNode(NodeType nodeType, string name, string perfab, GameObject go = null  )
	{
		Node ret = null;
		switch (nodeType) 
        {
		case NodeType.Planet:
			ret = new PlanetNode(name);
			break;
		case NodeType.Tower:
			ret = new TowerNode(name);
			break;
		case NodeType.Castle:
			ret = new CastleNode(name);
			break;
		case NodeType.WarpDoor:
			ret = new WarpDoorNode(name);
			break;
		case NodeType.Barrier:
			ret = new BarrierNode(name);
			break;
		case NodeType.BarrierLine:
			ret = new BarrierLineNode(name);
			break;
		case NodeType.MasterA:
        case NodeType.MasterB:
            ret = new MasterNode(name);
			break;
		case NodeType.Power:
			ret = new PowerNode(name);
			break;
		default:
			break;
		}
        if( ret != null )
        {
            ret.perfab   = perfab;
            ret.nodeType = nodeType;
            ret.Init(go);
        }
		return ret;
	}

	
	/// <summary>
	/// 获取一个节点
	/// </summary>
	public Node GetNode(string tag)
	{
		Node node = null;
		nodeMap.TryGetValue (tag, out node);
		return node;
	}


    public Node GetNodeByType( NodeType eType )
    {
        foreach( var i in nodeMap )
        {
            if (i.Value.nodeType == eType)
                return i.Value;
        }

        return null;
    }

	/// <summary>
	/// 获取有用的星球，即可被占领的，也即除去障碍物点的
	/// </summary>
	public List<Node> GetUsefulNodeList()
	{
		return usefulNodeList;
	}

	/// <summary>
	/// 获得障碍物星球
	/// </summary>
	public List<Node> GetBarrierNodeList()
	{
		return barrierNodeList;
	}

	
	/// <summary>
	/// 所有星球都被占领，参数为占领者的team
	/// </summary>
	public bool AllOccupied(int eTEAM)
	{
		int eTeamGroup = sceneManager.teamManager.GetTeam ((TEAM)eTEAM).groupID;
		Node node;
		for (int i = 0; i < usefulNodeList.Count; ++i)
		{
			node = usefulNodeList [i];
			if ((int)node.team != eTEAM) 
			{
				if (!node.currentTeam.IsFriend(eTeamGroup))
					return false;
			}
		}

		return true;
	}

	/// <summary>
	/// 检测，队伍是否还有星球
	/// </summary>
	public bool CheckHaveNode(int eTEAM)
	{
		Node node;
		for (int i = 0; i < usefulNodeList.Count; ++i)
		{
			node = usefulNodeList [i];
			if ((int)node.team == eTEAM) 
			{
				return true;
			}
		}

		return false;
	}

	public void SilentMode (bool status)
	{
		foreach (var i in nodeMap)
		{
			i.Value.entity.SilentMode (status);
		}
	}

    //只清空障碍连线但是不清空数据
    public void DestroyBarrierLineNode()
    {
        for (int i = 0, max = barrierNodeList.Count; i < max; ++i)
        {
            Node node = barrierNodeList[i];
            if (node.nodeType == NodeType.BarrierLine)
                node.Destroy();
        }
    }
}
