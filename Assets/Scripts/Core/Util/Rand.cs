using System;
using UnityEngine;

namespace Plugin
{
	public class Rand
	{
		/// <summary>
		/// 设置种子
		/// </summary>
		public int seed {  
			set { 
				this.holdSeed = value;
			} 

			get { return holdSeed; }
		}

		/// <summary>
		/// double随机参数
		/// </summary>
		static double MBIG = 1f / (ushort.MaxValue / 2);

		/// <summary>
		/// 真正的种子
		/// </summary>
		int holdSeed;

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="seed">Seed.</param>
		public Rand(int seed)
		{
			holdSeed = seed;
		}

		/// <summary>
		/// 随机数
		/// </summary>
		public int Random()
		{
			return ((holdSeed = holdSeed * 214013 + 2531011) >> 16) & 0x7fff;
		}

		/// <summary>
		/// double随机数
		/// </summary>
		/// <returns>The double.</returns>
		public double RandomDouble()
		{
			return Random() * MBIG;
		}

		/// <summary>
		/// 浮点随机数
		/// </summary>
		/// <returns>The float.</returns>
		public float RandomFloat()
		{
			return (float)(Random() * (float)MBIG);
		}

		/// <summary>
		/// 闭区间 [min, max]
		/// </summary>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public int Range(int min, int max)
		{
			if (min > max)
			{
				throw new ArgumentOutOfRangeException();
			}

			return (Random() % (max-min)) + min;
		}

		/// <summary>
		/// 闭区间 [min, max]
		/// </summary>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public float Range(float min, float max)
		{
			if (min > max)
			{
				throw new ArgumentOutOfRangeException();
			}

			return ((max - min)*RandomFloat()) + min;
		}
	}
}

