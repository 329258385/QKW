using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Plugin;







[Serializable]
public class MovePacket
{
	public string from;
	public string to;
	public List<string> tags = new List<string>();
	public float rate;
}

[Serializable]
public class SkillPacket
{
	public int skillID;
	public TEAM from;
	public TEAM to;
	public string tag;
}

[Serializable]
public class GiveUpPacket
{
	public TEAM team;
}

public class FramePacket
{
	public byte type;
	public MovePacket move;
	public SkillPacket skill;
	public GiveUpPacket giveup;
}

public class Packet
{
	public TEAM team;
	public FramePacket packet;
}


public enum TEAM : byte
{
	Neutral,		// 中立
	Team_1,			// 蓝方
	Team_2,			// 红方

	TeamMax,
}


/// <summary>
/// Vector 数学运算.
/// </summary>
public static class VectorMath
{
	/// <summary>
	/// Angles the axis.
	/// </summary>
	/// <returns>The axis.</returns>
	/// <param name="axis">Axis.</param>
	/// <param name="angle">Angle.</param>
	static public Quaternion AngleAxis(float angle, Vector3 axis)
	{
		float mag = axis.magnitude;

		if (mag > 0.000001F) {
			float halfAngle = angle * 0.5F;

			Quaternion q = new Quaternion ();
			q.w = Mathf.Cos (halfAngle);

			float s = Mathf.Sin (halfAngle) / mag;
			q.x = s * axis.x;
			q.y = s * axis.y;
			q.z = s * axis.z;

			return q;
		} else {
			return Quaternion.identity;
		}

	}

	/// <summary>
	/// 围绕某点旋转, 角度的单位为弧度
	/// </summary>
	/// <returns>The around.</returns>
	/// <param name="point">Point.</param>
	/// <param name="axis">Axis.</param>
	/// <param name="angle">Angle.</param>
	static public Vector3 RotateAround(this Vector3 position, Vector3 point, Vector3 axis, float angle)
	{
		Vector3 vector = position;
		Quaternion rotation = VectorMath.AngleAxis(angle, axis);
		Vector3 vector2 = vector - point;
		vector2 = rotation * vector2;
		vector = point + vector2;
		position = vector;

		return position;
	}
}