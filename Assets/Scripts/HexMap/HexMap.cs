using UnityEngine;
using System.Collections.Generic;





public class HexMap : MonoBehaviour
{
	public static HexMap		Instance;

	[HideInInspector] 
	public  HexMapArea			area = new HexMapArea( 10, 10, 10, 1 );

	[HideInInspector] 
	public  float				inclinationMax = 30;



	private void Awake()
    {
        Instance					= this;
    }


    private void Start()
    {
		GenerateStaticMap();
    }


   
    public void GenerateStaticMap()
	{
		area.map = new HexmapNode[area.tilesInX, area.tilesInZ];
		for (int i = 0; i < area.tilesInX; i++)
		{
			for (int j = 0; j < area.tilesInZ; j++)
			{
				float x				= transform.position.x + i * area.tileSize + ((float)area.tileSize) / 2;
				float z				= transform.position.z + j * area.tileSize + ((float)area.tileSize) / 2;
				float y				= transform.position.y;
				area.map[i, j]		= new HexmapNode(new Vector3(x, y, z));
				area.map[i, j].pos	= new Vector3(x, y, z);
				area.map[i, j].walkable = true;
			}
		}


		for (int i = 0; i < area.tilesInX; i++)
		{
			for (int j = 0; j < area.tilesInZ; j++)
			{
				List<HexmapNode> neighbourList = new List<HexmapNode>();
				HexmapNode nodeFrom		 = area.map[i, j];
				HexmapNode nodeTo		 = area.map[i, j];
				if (j - 1 > -1)
				{
					nodeTo				 = area.map[i, j - 1];
					neighbourList		 = AddInNeighbourList(neighbourList, nodeFrom, nodeTo);
				}
				if (j + 1 < area.tilesInZ)
				{
					nodeTo				= area.map[i, j + 1];
					neighbourList		= AddInNeighbourList(neighbourList, nodeFrom, nodeTo);
				}
				if (i - 1 > -1)
				{
					nodeTo				= area.map[i - 1, j];
					neighbourList		= AddInNeighbourList(neighbourList, nodeFrom, nodeTo);
				}
				if (i + 1 < area.tilesInX)
				{
					nodeTo				= area.map[i + 1, j];
					neighbourList		= AddInNeighbourList(neighbourList, nodeFrom, nodeTo);
				}
				area.map[i, j].neighbour = neighbourList.ToArray();
			}
		}
	}

	public float AngleToTangent(float angle)
	{
		return (Mathf.Tan(Mathf.Deg2Rad * (angle)));
	}


	public List<HexmapNode> AddInNeighbourList(List<HexmapNode> neighbourList, HexmapNode nodeFrom, HexmapNode nodeTo)
	{
		float height		= Mathf.Abs(nodeFrom.pos.y - nodeTo.pos.y);
		if (height < (AngleToTangent(inclinationMax)))
		{
			float dist		= Mathf.Abs(Vector3.Distance(nodeFrom.pos, nodeTo.pos));
			nodeTo.cost		= dist;
			neighbourList.Add(nodeTo);
		}
		return neighbourList;
	}


	public HexmapNode Vector3ToNode( Vector3 point )
	{
		HexmapNode[,] map		= area.map;
		float startX			= transform.position.x;
		float startZ			= transform.position.z;

		int x					= (int)((point.x - startX) / area.tileSize);
		int z					= (int)((point.z - startZ) / area.tileSize);
		HexmapNode node			= map[x, z];
		return node;
	}


	/// <summary>
	/// 找到范围内最小的风险值
	/// </summary>
	public Vector3 Find3x3MinCost( Vector3 pos )
    {
		float cost		= float.MaxValue;
		Vector3 result	= Vector3.zero;
		for( int i = -1; i <= 1; i++ )
        {
			for( int j = -1; j <= 1; j++ )
            {
				Vector3 temp	= Vector3.zero;
				temp.x			= pos.x + i * area.tileSize + ((float)area.tileSize) / 2;
				temp.z			= pos.z + j * area.tileSize + ((float)area.tileSize) / 2;

				HexmapNode node = Vector3ToNode(temp);
				if( node != null && node.cost < cost && node.state == State.Open )
                {
					cost		= node.cost;
					result		= node.pos;
                }
			}
        }
		return result;
	}


	public float Get3x3RangeCost( Vector3 pos )
    {
		float cost		= -1f;
		for( int i = -1; i <= 1; i++ )
        {
			for( int j = -1; j <= 1; j++ )
            {
				Vector3 temp	= Vector3.zero;
				temp.x			= pos.x + i * area.tileSize + ((float)area.tileSize) / 2;
				temp.z			= pos.z + j * area.tileSize + ((float)area.tileSize) / 2;

				HexmapNode node = Vector3ToNode(temp);
				if( node != null && node.cost < cost )
                {
					cost		+= node.cost;
                }
			}
        }
		return cost;
    }


	/// <summary>
	/// 找到某个范围内最大的风险值
	/// </summary>
	public Vector3 Find3x3MaxCost(Vector3 pos)
	{
		float cost = float.MinValue;
		Vector3 result = Vector3.zero;
		for( int i = -1; i <= 1; i++ )
        {
			for( int j = -1; j <= 1; j++ )
            {
				Vector3 temp	= Vector3.zero;
				temp.x			= pos.x + i * area.tileSize + ((float)area.tileSize) / 2;
				temp.z			= pos.z + j * area.tileSize + ((float)area.tileSize) / 2;

				HexmapNode node = Vector3ToNode(temp);
				if( node != null  && node.cost > cost && node.state == State.Open )
                {
					cost		= node.cost;
					result		= node.pos;
                }
			}
        }
		return result;
	}


	public void ModifyNodeCost(HexmapNode hexnode, float value)
	{
		if (hexnode != null)
		{
			hexnode.cost += value;
			if (hexnode.cost <= 0f)
				hexnode.state	= State.Open;
			else
				hexnode.state	= State.Clear;
		}
	}


	public float GetNodeCost( Vector3 pos )
    {
		HexmapNode node				= Vector3ToNode(pos);
		if (node != null)
		{
			return node.cost;
		}
		return 0f;
	}



	[HideInInspector] public bool showGizmo = true;
	[HideInInspector] public Color colorLinks = Color.green;
	void OnDrawGizmos()
	{
		
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(transform.position + new Vector3(((float)area.tilesInX * area.tileSize) / 2, (float)(area.height) / 2, ((float)area.tilesInZ * area.tileSize) / 2), new Vector3(area.tilesInX * area.tileSize, area.height * area.tileSize, area.tilesInZ * area.tileSize));
		Gizmos.DrawCube(transform.position, Vector3.one);

		Vector3 yOffset = new Vector3(0, 0.01f, 0);

		if (showGizmo)
		{
			if (area.map != null)
			{
				for (int i = 0; i < area.tilesInX; i++)
				{
					for (int j = 0; j < area.tilesInZ; j++)
					{
						foreach (HexmapNode n in area.map[i, j].neighbour)
						{
							Gizmos.color = colorLinks;
							Gizmos.DrawLine(area.map[i, j].pos + yOffset, n.pos + yOffset);
						}
					}
				}
			}
		}
	}
}

