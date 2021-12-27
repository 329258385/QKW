using System;
using System.Collections.Generic;
using UnityEngine;






namespace Plugin
{
	public class FrameNode
	{
		/// <summary>
		/// 帧号
		/// </summary>
		/// <value>The frame.</value>
		public int frame { get; set; }

		/// <summary>
		/// 帧同步包
		/// </summary>
		/// <value>The message list.</value>
		public object[] msgList { get; set; }
	}
	
	/// <summary>
	/// 帧包逻辑委托
	/// </summary>
	public delegate void RunPacketHandler(FrameNode node);

	/// <summary>
	/// 帧同步逻辑
	/// </summary>
	public delegate void RunLockStepLogic(int frame, float dt);

	/// <summary>
	/// 开始帧同步委托
	/// </summary>
	public delegate void RunLockStepEvent();

	/// <summary>
	/// 加速委托
	/// </summary>
	public delegate float RunLockStepSpeed(int len);

	/// <summary>
	/// 当帧包发生错误
	/// </summary>
	public delegate void RunLockStepFrameError(int logicFrame, FrameNode node);

	/// <summary>
	/// 帧同步
	/// 沙盒
	/// </summary>
	public class LockStep
	{
		/// <summary>
		/// 帧包集合
		/// </summary>
		Queue<FrameNode> frameQueue = new Queue<FrameNode>();

		/// <summary>
		/// 执行帧包的委托事件
		/// </summary>
		RunPacketHandler packetHandler;

		/// <summary>
		/// 帧同步逻辑委托事件
		/// </summary>
		RunLockStepLogic lockstepHandler;

		/// <summary>
		/// 开始帧同步委托事件
		/// </summary>
		RunLockStepEvent beginHandler;

		/// <summary>
		/// 帧同步停止委托事件
		/// </summary>
		RunLockStepEvent endHandler;

		/// <summary>
		/// 加速委托事件
		/// </summary>
		RunLockStepSpeed speedHandler;

		/// <summary>
		/// 当前帧包
		/// </summary>
		/// <value>The current node.</value>
		FrameNode currentNode { get; set; }

		/// <summary>
		/// 帧同步时间
		/// </summary>
		/// <value>The total frame time.</value>
		float totalFrameTime { get; set; }

		/// <summary>
		/// 当前帧间隔时间
		/// </summary>
		/// <value>The current frame timer.</value>
		float currentFrameTimer { get; set; }

		/// <summary>
		/// 当前帧号
		/// </summary>
		/// <value>The current frame.</value>
		int currentFrame { get; set; }

		/// <summary>
		/// 1秒中要跑多少帧
		/// </summary>
		/// <value>The max frame.</value>
		int maxFrame { get; set; }

		/// <summary>
		/// 逻辑帧
		/// </summary>
		/// <value>The logic frame.</value>
		int logicFrame { get; set; }

		/// <summary>
		/// 帧间隔时间
		/// </summary>
		float FRAME_TIME { get; set; }

		/// <summary>
		/// 超速阀值
		/// </summary>
		/// <value>The frame threshold.</value>
		public int frameThreshold { get; set; }

		/// <summary>
		/// FPS
		/// </summary>
		/// <value>The fps.</value>
		public int FPS { get { return currentFPS; } }

		/// <summary>
		/// 速度比例
		/// </summary>
		/// <value>The speed level.</value>
		public float playSpeed { get; set; }

		/// <summary>
		/// 设置是否是重播状态
		/// </summary>
		/// <value><c>true</c> if re play; otherwise, <c>false</c>.</value>
		public bool replay { get; set; }

		/// <summary>
		/// 在帧包超过阀值的情况下，每一帧处理帧包的数量
		/// 帧包为逻辑帧包，不包括平滑帧
		/// = 0 全部处理
		/// > 1 为每一帧处理的逻辑帧包数量
		/// </summary>
		/// <value>The run frame count.</value>
		public int runFrameCount { get; set; }

		/// <summary>
		/// 当前消息数量
		/// </summary>
		/// <value>The message count.</value>
		public int messageCount { get { return frameQueue.Count; } }

		/// <summary>
		/// 运行到某逻辑帧
		/// </summary>
		/// <value>The run to frame.</value>
		int runToFrame { get; set; }

		/// <summary>
		/// 帧数统计
		/// </summary>
		/// <value>The frame count.</value>
		int frameCount { get; set; }

		/// <summary>
		/// Gets or sets the current frame.
		/// </summary>
		/// <value>The current frame.</value>
		int currentFPS { get; set; }

		/// <summary>
		/// 总时间
		/// </summary>
		/// <value>The total time.</value>
		float totalTime { get; set; }

		/// <summary>
		/// 是否存在
		/// </summary>
		/// <value><c>true</c> if is alive; otherwise, <c>false</c>.</value>
		bool isAlive { get; set; }

		/// <summary>
		/// 帧包报错
		/// </summary>
		/// <value>The frame error.</value>
		public RunLockStepFrameError frameError { get; set; }

		/// <summary>
		/// 最后填充的帧号
		/// </summary>
		/// <value>The last frame.</value>
		int lastFrame { get; set; }

		/// <summary>
		/// 初始化
		/// </summary>
		public LockStep ()
		{
			//设置总帧数
			maxFrame = 10;

			//设置逻辑帧
			logicFrame = 5;

			//帧间隔
			FRAME_TIME = 1f/(maxFrame*logicFrame);

			//超速阀值
			frameThreshold = 3;

			//速度比例
			playSpeed = 0f;

			//清理
			Release ();
		}

		/// <summary>
		/// 释放无用资源
		/// </summary>
		private void Release()
		{
			Reset ();

			currentNode         = null;
			packetHandler       = null;
			lockstepHandler     = null;
			beginHandler        = null;
			endHandler          = null;
			runFrameCount       = 0;
		}

		private void Reset()
		{
			isAlive             = false;
			currentNode         = null;
			frameQueue.Clear ();
			totalFrameTime      = 0f;
			currentFrame        = 0;
			currentFPS          = 0;
			totalTime           = 0f;
			totalFrameTime      = 0;
			runToFrame          = 0;
			lastFrame           = 0;
			playSpeed           = 1;
			replay              = false;
		}

		/// <summary>
		/// 获取当前帧数
		/// </summary>
		/// <returns>The current frame.</returns>
		public int GetCurrentFrame()
        {
			return currentFrame;
		}

		/// <summary>
		/// 帧同步开始
		/// </summary>
		public void StarLockStep()
		{
			if (isAlive) throw new Exception("LockStep is alive.");
			//清理数据
			//frameQueue.Clear ();
			totalFrameTime  = 0f;
			currentNode     = null;
			currentFrame    = 0;
			isAlive         = true;
		}

		/// <summary>
		/// 停止帧同步
		/// </summary>
		public void StopLockStep(bool release=false)
		{
			if (release) {
				Release ();
			} else {
				Reset ();
			}
		}

		/// <summary>
		/// 添加一个帧包
		/// </summary>
		/// <param name="frame">Frame.</param>
		/// <param name="msgList">Message list.</param>
		public void AddFrame(int frame, object[] msgList)
		{
			if (lastFrame >= frame) return;

			FrameNode node  = new FrameNode ();
			node.frame      = frame;
			node.msgList    = msgList;
			frameQueue.Enqueue (node);
			lastFrame       = frame;
		}

		/// <summary>
		/// 执行到某一帧
		/// </summary>
		/// <param name="frame">Frame.</param>
		public void RunToFrame(int frame)
		{
			runToFrame      = frame*logicFrame;
		}

		/// <summary>
		/// 设置加速委托事件
		/// </summary>
		/// <param name="handler">Handler.</param>
		public void AddListennerSpeed(RunLockStepSpeed handler)
		{
			speedHandler = handler;

			if (speedHandler == null)
				speedHandler = TimeScaleChange;
		}

		/// <summary>
		/// 监听停止帧同步事件
		/// </summary>
		/// <param name="handler">Handler.</param>
		public void AddListennerEnd(RunLockStepEvent handler)
		{
			endHandler = endHandler == null ? handler : endHandler + handler;
		}

		/// <summary>
		/// 添加监听开始函数
		/// </summary>
		/// <param name="handler">Handler.</param>
		public void AddListennerBegin(RunLockStepEvent handler)
		{
			beginHandler = beginHandler == null ? handler : beginHandler + handler;
		}

		/// <summary>
		/// 添加监听帧包委托
		/// </summary>
		/// <param name="handler">Handler.</param>
		public void AddListennerPacket(RunPacketHandler handler)
		{
			packetHandler = packetHandler == null ? handler : packetHandler + handler;
		}

		/// <summary>
		/// 添加监听帧逻辑处理
		/// </summary>
		/// <param name="handler">Handler.</param>
		public void AddListennerLogic(RunLockStepLogic handler)
		{
			lockstepHandler = lockstepHandler == null ? handler : lockstepHandler + handler;
		}

		/// <summary>
		/// 心跳
		/// </summary>
		public void Tick(float dt)
		{
			if (!isAlive) return;

			//统计fps
			CalcFPS(dt);

			//无消息，则需要等待帧包
			if (frameQueue.Count == 0)
				return;

			if (replay) ReplayProcess(dt);	//重播模式
			else NormalProcess(dt);			//正常播放
		}

		/// <summary>
		/// 重播模式
		/// </summary>
		void ReplayProcess(float dt)
		{
			if (runToFrame > currentFrame)
			{
				int runMaxFrame = logicFrame * frameQueue.Count;

				int targetFrame = runFrameCount <= 0 ? runMaxFrame : runFrameCount * logicFrame;
				targetFrame = targetFrame >= runMaxFrame ? runMaxFrame : targetFrame;

				//加速
				for (int i = 0; i < targetFrame; ++i)
				{
					NextFrameLogic();
				}
			}
			else
			{
				// 此处的逻辑可以简化为减速的情况

				if (playSpeed > 1f) {
					int max = Mathf.RoundToInt (playSpeed);

					for (int i = 0; i < max; ++i) {
						//执行帧
						NextFrameLogic ();
					}
				} else if (playSpeed < 1f) {
					// 减速
					totalFrameTime += dt * playSpeed;
					if (totalFrameTime >= FRAME_TIME)
					{
						totalFrameTime -= FRAME_TIME;

						//执行帧
						NextFrameLogic();
					}
				}
				else
				{
					//正常速度
					//时间累加
					totalFrameTime += dt;

					//满足执行时间的话
					if (totalFrameTime >= FRAME_TIME)
					{
						totalFrameTime -= FRAME_TIME;

						//执行帧
						NextFrameLogic();
					}
				}
			}
		}

		/// <summary>
		/// 正常播放模式
		/// </summary>
		void NormalProcess(float dt)
		{ 
			//超速
			if (frameQueue.Count > frameThreshold)
			{
				if (runToFrame > currentFrame)
				{
					int runMaxFrame = logicFrame * frameQueue.Count;

					int targetFrame = runFrameCount <= 0 ? runMaxFrame : runFrameCount * logicFrame;
					targetFrame = targetFrame >= runMaxFrame ? runMaxFrame : targetFrame; 

					//加速
					for (int i = 0; i < targetFrame; ++i)
					{
						NextFrameLogic();
					}
				}
				else
				{ 
					if (currentFPS >= 10)
					{
						//加速logicFrame帧，防止卡的太厉害
						for (int i = 0; i < logicFrame; ++i)
						{
							NextFrameLogic();
						}
					}
					else
					{
						//全部执行
						for (int i = 0; i < logicFrame * frameQueue.Count; ++i)
						{
							NextFrameLogic();
						}
					}
				}
			}
			else
			{
				//是否加速
				if (speedHandler != null)
					Time.timeScale = speedHandler.Invoke(frameQueue.Count);

				//时间累加
				totalFrameTime += dt;

				//满足执行时间的话
				if (totalFrameTime >= FRAME_TIME)
				{
					totalFrameTime -= FRAME_TIME;

					//执行帧
					NextFrameLogic();
				}
			}
		}

		/// <summary>
		/// 计算fps
		/// </summary>
		void CalcFPS(float dt)
		{
			++frameCount;

			totalTime += dt;
			if (totalTime >= 1f)
			{
				currentFPS = frameCount;
				frameCount = 0;
				totalTime = 0;
			}
		}

		/// <summary>
		/// 执行下一帧逻辑
		/// </summary>
		void NextFrameLogic()
		{
			//如果没有逻辑包，等待逻辑包
			if (frameQueue.Count == 0) return;

			//累计帧号
			++ currentFrame;

			//确定是否执行逻辑帧
			if (((currentFrame % logicFrame) == 0))
			{
				//取出当前逻辑帧
				currentNode = frameQueue.Dequeue();
				if ((currentNode.frame * logicFrame) != currentFrame)
				{
					//TODO 打印log
					if (frameError != null) 
                        frameError(currentFrame, currentNode);
				}

				//执行逻辑包
				if (packetHandler != null) 
                    packetHandler(currentNode);
			}

			//设置当前帧消耗的时间
			currentFrameTimer = FRAME_TIME;

			//帧逻辑
			if (lockstepHandler != null) 
                lockstepHandler (currentFrame, currentFrameTimer);
		}

		/// <summary>
		/// 加速变化
		/// >= frameThreshold speed=1f
		/// other speed=3f
		/// </summary>
		/// <returns>The scale change.</returns>
		/// <param name="len">Length.</param>
		float TimeScaleChange(int len)
		{
			//确定是否加速
			if (len <= 1)
				return 1f;
				
			//超过阀值，普通速度
			if(len <= frameThreshold)
				return 1f;
			
			//其他3倍速度
			return 3f;
		}

	}
}

