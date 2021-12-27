using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Solarmax;






/// <summary>
/// 公转类型
/// </summary>
public enum RevolutionType
{
	RT_None,
	RT_Circular,			//圆形
	RT_Triangle,			//三角形
	RT_Quadrilateral,		//四方形
	RT_Ellipse,				//椭圆
}
/// <summary>
/// 节点公转模块
/// </summary>
public partial class Node
{
	/// <summary>
	/// 当前公转目标点
	/// </summary>
	int curMoveIndex = 0;

	/// <summary>
	/// 公转目标点队列
	/// </summary>
	List<Vector3> posList = new List<Vector3>();

	/// <summary>
	/// 公转速度
	/// </summary>
	float RevoSpeed = 12f;

	/// <summary>
	/// 速度变量
	/// </summary>
	float curAngle = 0f;

	/// <summary>
	/// 公转方向是否顺时针
	/// </summary>
	bool isClockWise = false;

	/// <summary>
	/// 公转类型
	/// </summary>
	public RevolutionType revoType = RevolutionType.RT_None;

	public string revoParam1;
	public string revoParam2;

	/// <summary>
	/// 公转起始位置
	/// </summary>
	Vector3 beginPos = Vector3.zero;

	/// <summary>
	/// 公转起始角度
	/// </summary>
	float revoAngle = 0f;

	/// <summary>
	/// 椭圆时的A参数
	/// </summary>
	private float ellipseA;
	/// <summary>
	/// 椭圆时的B参数
	/// </summary>
	private float ellipseB;
	private float ellipseC;

	/// <summary>
	/// 公转设置
	/// </summary>
	/// <param name="type">type.</param>
	/// <param name="posX">posX.</param>
	/// <param name="posY">posY.</param>
	public void SetRevolution(int type, string param1, string param2, bool clockwise = false)
	{
		revoParam1      = param1;
		revoParam2      = param2;
		Vector3 pos1    = Converter.ConvertVector3D (param1);
		Vector3 pos2    = Converter.ConvertVector3D (param2);

		float posX      = pos1.x;
		float posY      = pos1.y;

		revoType        = (RevolutionType)type;
		if (revoType == RevolutionType.RT_None)
			return;

		isClockWise     = clockwise;
		revoAngle       = 0;
		switch (revoType) 
		{
		case RevolutionType.RT_Circular:
			{
				posList.Add (new Vector3 (posX, posY, 0));
			}
			break;
		case RevolutionType.RT_Triangle:
			{
				Vector3 curPos = GetPosition();
				Vector3 cenPos = new Vector3 (posX, posY, 0);
				posList.Add (curPos);
				Vector3 v = RotateVector3 (curPos, cenPos, 2f / 3f * Mathf.PI, isClockWise);
				posList.Add (v);
				v = RotateVector3 (v, cenPos, 2f / 3f * Mathf.PI, isClockWise);
				posList.Add (v);
			}
			break;
		case RevolutionType.RT_Quadrilateral:
			{
				Vector3 curPos = GetPosition ();
				Vector3 cenPos = new Vector3 (posX, posY, 0);
				posList.Add (curPos);
				Vector3 v = RotateVector3 (curPos, cenPos, 0.5f * Mathf.PI, isClockWise);
				posList.Add (v);
				v = RotateVector3 (v, cenPos, 0.5f * Mathf.PI, isClockWise);
				posList.Add (v);
				v = RotateVector3 (v, cenPos, 0.5f * Mathf.PI, isClockWise);
				posList.Add (v);
			}
			break;
		case RevolutionType.RT_Ellipse:
			{
				// 椭圆旋转轨迹

				Vector3 curPos  = GetPosition();
				float pf1pf2    = Vector3.Distance (curPos, pos1) + Vector3.Distance (curPos, pos2); // 2a
				float f1f2      = Vector3.Distance (pos1, pos2); //2c
				ellipseA        = pf1pf2 / 2; // a
				ellipseC        = f1f2 / 2; // c
				ellipseB        = Mathf.Sqrt (Mathf.Pow (ellipseA, 2) - Mathf.Pow (ellipseC, 2));

				Vector3 centerPos = (pos1 + pos2) / 2;
				posList.Add (centerPos);

				// angel
				Vector3 relativePos = curPos - centerPos; // p.x = Acos@, p.y = Bsin@;
				float angel = Mathf.Acos (relativePos.x / ellipseA);

				//判断象限
				if (relativePos.y >= 0) {
					// sin@ > 0, 在一二象限
					revoAngle = angel;
				} else if (relativePos.y < 0){
					// sin@ < 0. 在三四象限
					revoAngle = 2 * Mathf.PI - angel;
				}
			}
			break;
		default:
			break;
		}

		beginPos = GetPosition();
	}
		
	void CalcRevolutionAngle()
	{
		if (revoAngle >= 2 * Mathf.PI) {
			revoAngle -= 2 * Mathf.PI;
		}

		if (revoAngle <= -2 * Mathf.PI) {
			revoAngle -= -2 * Mathf.PI;
		}
	}

	/// <summary>
	/// 公转计算
	/// </summary>
	/// <param name="frame">Frame.</param>
	/// <param name="dt">Dt.</param>
	protected void UpdateRevolution(int frame, float dt)
	{
		switch (revoType) 
		{
		case RevolutionType.RT_Circular:
			{
				if(isClockWise) revoAngle += dt * RevoSpeed * 0.01f;
				else revoAngle -= dt * RevoSpeed * 0.01f;

				CalcRevolutionAngle ();

				Vector3 v = RotateVector3 (beginPos, posList [curMoveIndex], revoAngle, isClockWise);
				SetPosition(v);
			}
			break;
		case RevolutionType.RT_Triangle:
			{
				Vector3 targetPos = posList [curMoveIndex + 1];
				curAngle = RevoSpeed * dt * 0.05f;
				//间距多少
				float dist = Vector3.Distance (entity.GetPosition (), targetPos);

				if (dist > curAngle) {
                    SetPosition(Vector3.MoveTowards(entity.GetPosition(), targetPos, curAngle));
				} 
                else {
					SetPosition( targetPos);
					curMoveIndex++;
					if (curMoveIndex >= 2)
						curMoveIndex = -1;
				}
			}
			break;
		case RevolutionType.RT_Quadrilateral:
			{
				Vector3 targetPos = posList [curMoveIndex + 1];
				curAngle = RevoSpeed * dt * 0.05f;
				//间距多少
				float dist = Vector3.Distance (entity.GetPosition (), targetPos);

				if (dist > curAngle) {
					SetPosition(Vector3.MoveTowards (entity.GetPosition (), targetPos, curAngle));
				} else {
					SetPosition( targetPos);
					curMoveIndex++;
					if (curMoveIndex >= 3)
						curMoveIndex = -1;
				}
			}
			break;
		case RevolutionType.RT_Ellipse:
			{
				// 椭圆旋转轨迹
				if(isClockWise) revoAngle += dt * RevoSpeed * 0.01f;
				else revoAngle -= dt * RevoSpeed * 0.01f;

				CalcRevolutionAngle ();

				Vector3 v = Vector3.zero;
				v.x = ellipseA * Mathf.Cos (revoAngle);
				v.y = ellipseB * Mathf.Sin (revoAngle);

				v += posList [curMoveIndex];
				
				SetPosition(v);
			}
			break;
		default:
			break;
		}
	}

	
	/// <summary>
	/// 返回点P围绕点A旋转弧度rad后的坐标
	/// </summary>
	/// <param name="P">待旋转点坐标</param>
	/// <param name="A">旋转中心坐标</param>
	/// <param name="rad">旋转弧度</param>
	/// <param name="isClockwise">true:顺时针/false:逆时针</param>
	/// <returns>旋转后坐标</returns>
	Vector3 RotateVector3(Vector3 P, Vector3 A, float rad, bool isClockwise = true)
	{
		//return P.RotateAround (A, isClockwise ? Vector3.forward : Vector3.back, rad);
		return VectorMath.RotateAround(P, A, Vector3.forward, /*(isClockwise ? 1 : -1) */ rad);
	}

	/// <summary>
	//获取目标时间后该公转星球的位置
	/// </summary>
	/// <param name="runTime">时间</param>
	/// <returns>旋转后坐标</returns>
	public Vector3 GetNodeRunPosition(float runTime)
	{
		if (revoType == RevolutionType.RT_Ellipse) {

			float angle = runTime * RevoSpeed * 0.01f;
			if (isClockWise)
				angle = revoAngle + angle;
			else
				angle = revoAngle - angle;

			while (Mathf.Abs (angle) >= 2 * Mathf.PI) {
				if (angle >= 2 * Mathf.PI) {
					angle -= 2 * Mathf.PI;
				}
				if (angle <= -2 * Mathf.PI) {
					angle -= -2 * Mathf.PI;
				}
			}

			Vector3 v = Vector3.zero;
			v.x = ellipseA * Mathf.Cos (angle);
			v.y = ellipseB * Mathf.Sin (angle);

			v += posList [curMoveIndex];

			return v;
		} else {
			Vector3 pos = beginPos;

			float angle = runTime * RevoSpeed * 0.01f;

			if (isClockWise)
				angle = revoAngle + angle;
			else
				angle = revoAngle - angle;

			if (angle >= 2 * Mathf.PI) {
				angle -= 2 * Mathf.PI;
			}

			if (angle <= -2 * Mathf.PI) {
				angle -= -2 * Mathf.PI;
			}

			Vector3 v = RotateVector3 (pos, posList [curMoveIndex], angle, isClockWise);
			return v;
		}
	}


    /// <summary>
    /// 设置星球隐藏状态
    /// </summary>
    private void UpdateHideStatus()
    {

    }
}

