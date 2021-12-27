using UnityEngine;
using System.Collections;
using System.Collections.Generic;




/// <summary>
/// 管理障碍物连线的装饰
/// </summary>
public partial class NodeManager
{
	int BarrierLinetag = 1024;//累加的tag，为了不重复

	/// <summary>
	/// 障碍物节点记录
	/// </summary>
	class BarrierPair
	{
		public Vector3 keyPos;
		public Vector3 keyLocalPos;
		public Vector3 valuePos;
		public Vector3 valuedLocalPos;
		public List<int> barrierLinetags = new List<int>();
		public string barrierX;
		public string barrierY;
		public TEAM barrierTeam;
	}
	List<BarrierPair> BarriersKY = new List<BarrierPair> ();

    /// <summary>
    /// 几率定向传送门的位置
    /// </summary>
    class FixedPortal
    {
        public Vector3 keyPos;
        public float   fAngle;
        public float   fRadius;
    }

    List<FixedPortal> mapPortal = new List<FixedPortal>();
    public void AddFixedPortal( Vector3 pos, float fAngleY, float radius )
    {
        FixedPortal portal = new FixedPortal();
        portal.keyPos      = pos;
        portal.fAngle      = fAngleY;
        portal.fRadius     = radius;
        mapPortal.Add(portal);
    }


	/// <summary>
	/// 添加障碍物线
	/// </summary>
	/// <param name="barrierX">Barrier x.</param>
	/// <param name="barrierY">Barrier y.</param>
	public void AddBarrierLines(string barrierX, string barrierY)
	{
		Node nodeX = GetNode (barrierX);
		Node nodeY = GetNode (barrierY);
		if (null == nodeX || null == nodeY)
			return;

        if (nodeX.nodeType != NodeType.Barrier || nodeY.nodeType != NodeType.Barrier)
			return;

		BarrierPair pair    = new BarrierPair ();
		pair.keyPos         = nodeX.GetPosition ();
		pair.keyLocalPos    = nodeX.GetPosition ();
		pair.valuePos       = nodeY.GetPosition ();
		pair.valuedLocalPos = nodeY.GetPosition ();
		pair.barrierX       = barrierX;
		pair.barrierY       = barrierY;
		pair.barrierTeam    = nodeX.team;
		BarriersKY.Add (pair);
		calculatePoints (pair.keyPos,  pair.valuePos, pair);
	}

	/// <summary>
	/// 计算点的位置
	/// </summary>
	/// <param name="startPos">Start position.</param>
	/// <param name="endPos">End position.</param>
	void calculatePoints(Vector3 startPos, Vector3 endPos, BarrierPair pair)
	{
		Vector3 dir = endPos - startPos;
		dir.Normalize ();

		Vector3 tempPos = startPos;

		pair.barrierLinetags.Add (BarrierLinetag);
		draw (tempPos, dir, pair.barrierTeam);
		//TODO:这个算法需要改进，可能引起莫名的bug。
		while (Vector3.Distance (endPos, tempPos) > 0.1f) 
		{
			tempPos = tempPos + dir * 0.1f;
			pair.barrierLinetags.Add (BarrierLinetag);
			draw (tempPos, dir, pair.barrierTeam);
		}
	}

	/// <summary>
	/// 画线
	/// </summary>
	/// <param name="pos">Position.</param>
	void draw(Vector3 pos, Vector3 dir, TEAM barrierTEAM)
	{
		AddNode ((int)barrierTEAM,
			(int)NodeType.BarrierLine,
			pos.x,
			pos.y,
			0.5f,
			BarrierLinetag.ToString(),
			sceneManager.battleData.mapEdit,
			string.Empty,
			string.Empty,
			false,
            1.0f,
            string.Empty,
            string.Empty,
            0
		);

		float angle = Vector3.Angle (dir, Vector3.right);
		if (dir.y < 0) {
			angle = 360 - angle;
		}

		Node node = GetNode (BarrierLinetag.ToString ());
		node.SetRotation (new Vector3(0, 0, angle));

		BarrierLinetag++;
	}

	/// <summary>
	/// 删除障碍物线
	/// </summary>
	/// <param name="barrierX">Barrier x.</param>
	/// <param name="barrierY">Barrier y.</param>
	public void DelBarrierLines(string barrierX, string barrierY)
	{
		foreach (var pair in BarriersKY) 
		{
			if (barrierX.CompareTo(pair.barrierX)==0 && barrierY.CompareTo(pair.barrierY)==0) 
			{
				foreach(var tag in pair.barrierLinetags)
				{
					RemoveNode (tag.ToString());
				}
				BarriersKY.Remove (pair);
				break;
			}
			if(barrierX.CompareTo(pair.barrierY)==0 && barrierY.CompareTo(pair.barrierX)==0)
			{
				foreach(var tag in pair.barrierLinetags)
				{
					RemoveNode (tag.ToString());
				}
				BarriersKY.Remove (pair);
				break;
			}
		}
	}

		
	/// <summary>
	/// 计算相交
	/// </summary>
	public bool IntersectBarrierLien(Vector3 startPos, Vector3 endPos)
	{
		foreach (BarrierPair pair in BarriersKY) 
		{
			if (detectIntersect (pair.keyPos, pair.valuePos, startPos, endPos)) 
			{
				return true;
			}
		}
		return false;
	}

    /// <summary>
    /// 判断线段与圆是否相交
    /// </summary>
    public bool IntersectCircle( Vector3 vBegin, Vector3 vEnd, Vector3 center, float radius )
    {
        float a, b, c, dist_1, dist_2, angle_1, angle_2;
        if (vBegin.x == vEnd.x)                           //  当x相等
        {
            a = 1f; b = 0; c = -vBegin.x;
        }
        else if (vBegin.y == vEnd.y)                      //  当y相等
        {
            a = 0f; b = 1f; c = -vBegin.y;
        }
        else
        {
            a = vBegin.y - vEnd.y;
            b = vEnd.x   - vBegin.x;
            c = vBegin.x * vEnd.y - vBegin.y * vEnd.x;
        }
        dist_1 = a * center.x + b * center.y + c;
        dist_1 *= dist_1;
        dist_2 = (a * a + b * b) * radius * radius;
        if (dist_1 > dist_2)
        {
            return false;
        }
        angle_1 = (center.x - vBegin.x) * (vEnd.x - vBegin.x) + (center.y - vBegin.y) * (vEnd.y - vBegin.y);
        angle_2 = (center.x - vEnd.x) * (vBegin.x - vEnd.x) + (center.y - vEnd.y) * (vBegin.y - vEnd.y);
        if (angle_1 > 0 && angle_2 > 0)
        {
            return true;
        }
 
        return false;
    }

    /// <summary>
    /// 计算平面内两个向量的夹角
    /// </summary>
    float VectorAngle(Vector2 from, Vector2 to)
    {
        float angle;
        Vector3 cross = Vector3.Cross(from, to);
        angle         = Vector2.Angle(from, to);
        return cross.z > 0 ? -angle : angle;
    }


    /// <summary>
    /// 判断两点是否能定向传送
    /// </summary>
    public bool IsFixedPortal( Vector3 vBegin, Vector3 vEnd )
    {
        foreach( FixedPortal pair in mapPortal )
        {
            if (IntersectCircle(vBegin, vEnd, pair.keyPos, pair.fRadius))
            {
                float fa = VectorAngle(vBegin - vEnd, Vector3.up  );
                if ( Mathf.Abs(fa - pair.fAngle ) < 5.0f )
                {
                    return true;
                }
                return false;
            }
        }
        return false;
    }
		
	/// <summary>
	/// Between the specified a, X0 and X1.
	/// </summary>
	bool between(float a, float X0, float X1)  
	{  
		float temp1= a-X0;  
		float temp2= a-X1;  
		if ( ( temp1<1e-8 && temp2>-1e-8 ) || ( temp2<1e-6 && temp1>-1e-8 ) )  
		{  
			return true;  
		}  
		else  
		{  
			return false;  
		}  
	}  

	// 判断两条直线段是否有交点，有则计算交点的坐标  
	// p1,p2是直线一的端点坐标  
	// p3,p4是直线二的端点坐标  
	bool detectIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)  
	{  
		float line_x,line_y; //交点  
		if ( (Mathf.Abs(p1.x-p2.x)<1e-6) && (Mathf.Abs(p3.x-p4.x)<1e-6) )  
		{  
			return false;  
		}  
		else if ( (Mathf.Abs(p1.x-p2.x)<1e-6) ) //如果直线段p1p2垂直与y轴  
		{  
			if (between(p1.x,p3.x,p4.x))  
			{  
				float k = (p4.y-p3.y)/(p4.x-p3.x);  
				line_x = p1.x;  
				line_y = k*(line_x-p3.x)+p3.y;  

				if (between(line_y,p1.y,p2.y))  
				{  
					return true;  
				}  
				else  
				{  
					return false;  
				}  
			}  
			else   
			{  
				return false;  
			}  
		}  
		else if ( (Mathf.Abs(p3.x-p4.x)<1e-6) ) //如果直线段p3p4垂直与y轴  
		{  
			if (between(p3.x,p1.x,p2.x))  
			{  
				float k = (p2.y-p1.y)/(p2.x-p1.x);  
				line_x = p3.x;  
				line_y = k*(line_x-p2.x)+p2.y;  

				if (between(line_y,p3.y,p4.y))  
				{  
					return true;  
				}  
				else  
				{  
					return false;  
				}  
			}  
			else   
			{  
				return false;  
			}  
		}  
		else  
		{  
			float k1 = (p2.y-p1.y)/(p2.x-p1.x);   
			float k2 = (p4.y-p3.y)/(p4.x-p3.x);  

			if (Mathf.Abs(k1-k2)<1e-6)  
			{  
				return false;  
			}  
			else   
			{  
				line_x = ((p3.y - p1.y) - (k2*p3.x - k1*p1.x)) / (k1-k2);  
				line_y = k1*(line_x-p1.x)+p1.y;  
			}  

			if (between(line_x,p1.x,p2.x)&&between(line_x,p3.x,p4.x))  
			{  
				return true;  
			}  
			else   
			{  
				return false;  
			}  
		}  
	}
}
