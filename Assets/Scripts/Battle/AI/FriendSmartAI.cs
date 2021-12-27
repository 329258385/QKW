using System;
using System.Collections.Generic;
using UnityEngine;
using Plugin;




public class FriendSmartAIData
{
    public AIType				aiType;
	public AIStatus				aiStatus;
	public float				actionTime;
	public float				actionDelayDefend;
	public float				actionDelayAttack;
	public float				actionDelayGather;
	public float				actionDelayRebuild;
	public float				aiTimeInterval;
	public float				rebuildInterval;
	public bool					resultInside;
	public bool					resultIntersects;
	public Vector3				resultEnter;
	public Vector3				resultExit;
	public List<Node>			targetList;
	public List<Node>			senderList;
	

	public FriendSmartAIData()
	{
		targetList				= new List<Node> ();
		senderList				= new List<Node> ();
	}

	public virtual void Reset()
	{
		aiType					= AIType.Unknow;
		aiStatus				= AIStatus.Idle;
		actionTime				= 1.0f;
		aiTimeInterval			= 2.0f;
		actionDelayDefend		= 0f;
		actionDelayAttack		= 0f;
		actionDelayGather		= 0f;
		actionDelayRebuild		= 0;
		rebuildInterval			= 5.0f;
		resultInside			= false;
		resultIntersects		= false;
		resultEnter				= Vector3.zero;
		resultExit				= Vector3.zero;
		targetList.Clear ();
		senderList.Clear ();
	}
}

public class FriendSmartAILogic : BaseAILogic
{
    // Difficulty 标示关卡的难度 1 简单、2普通、 3困难
	public override void Init(Team t, AIType type, int level, int Difficulty)
	{
		FriendSmartAIData ad	= GetAiData (t);
		ad.Reset ();
		ad.aiType				= type;
		ad.actionTime			= 1f;

		RegisterCallback(AIStatus.Idle,		Idle);
        RegisterCallback(AIStatus.Defend,	Defend);
		RegisterCallback(AIStatus.Attack,	Attack);
		RegisterCallback(AIStatus.Gather,	Gather);
		RegisterCallback(AIStatus.Rebuild,  Rebuild);
	}

	public override void Release(Team t)
	{
		GetAiData(t).Reset ();
	}

	public static FriendSmartAIData GetAiData(Team t)
	{
		if (t.aiData == null)
		{
			t.aiData = new FriendSmartAIData ();
			t.aiData.aiType = AIType.Simple;
		}

		return (FriendSmartAIData)t.aiData;
	}

	/// <summary>
	/// 防御
	/// </summary>
	private bool Defend(Team t, float dt)
	{
		FriendSmartAIData ad    = t.aiData as FriendSmartAIData;
		Vector3 center          = TraversalAllNodeCenter (t);
		List<Node> allNodes     = sceneManager.nodeManager.GetUsefulNodeList();


		// 获取防御的所有节点
		ad.targetList.Clear();
		for (int i = 0; i < allNodes.Count; ++i)
		{
			var node = allNodes [i];
            if (node.nodeType == NodeType.Barrier)
                continue;

			// nothing to defend
            if (!node.IsOurNode(t.team) )
				continue;

            int nSelfStrength   = node.PredictedTeamStrength(t.team);   // 星球自己的战力
            int nEnemyStrength  = node.PredictedOppStrength(t.team);    // 星球敌方战力
            if (nEnemyStrength == 0)
				continue;

			// odds are good
            if (nSelfStrength > 5 && nSelfStrength > nEnemyStrength * 2)
				continue;

			Vector3 dp          = node.GetPosition () - center;
			float dist          = dp.magnitude;
            float odds          = nEnemyStrength - nSelfStrength;
			node.aiValue        = dist + odds;
			ad.targetList.Add (node);
		}

		// 如果没有目标，则返回错误
		if (ad.targetList.Count == 0)
		{
			return false;
		}

		// 根据目标防御代价进行排序
		ad.targetList.Sort(ComparisonAIValue);
        ad.senderList.Clear ();
		for (int i = 0; i < allNodes.Count; ++i)
		{
			var node            = allNodes [i];
			if (node.aiTimers[(int)t.team] > 0) continue;

            int teamStrength    = node.PredictedTeamStrength(t.team);
            int nEnemyStrength  = node.PredictedOppStrength(t.team);
            if (teamStrength < 5)
				continue;

			if (!node.IsOurNode(t.team) && teamStrength > nEnemyStrength )
				continue;

			node.aiStrength     = -teamStrength;
			ad.senderList.Add (node);
		}

		if (ad.senderList.Count == 0) return false;


        // 根据己方力量进行排序, 进行防御
		ad.senderList.Sort (ComparsionAIStrength);
		CalcTarget(t, ref ad );
		for (int i = 0; i < ad.targetList.Count; ++i)
		{
			Node target = ad.targetList [i];
			for (int j = 0; j < ad.senderList.Count; ++j)
			{
				Node sender = ad.senderList [j];
				if (sender == target)
					continue;

				if (!sender.AICanLink (target, t))
					continue;


                int senderStrength = sender.PredictedTeamStrength(t.team);
                int targetStrength = target.PredictedTeamStrength(t.team);
                int oppStrength    = target.PredictedOppStrength(t.team);
				int nBT			   = sender.CalebeComingBattle(t, oppStrength - targetStrength);
				if (nBT > 0)
				{
					sender.MoveEffect(target, nBT);
					sender.ResetAiTimer(t.team, ad.aiTimeInterval);
					return true;
				}
            }
		}
		return false;
	}

	/// <summary>
	/// 攻击
	/// </summary>
	private bool Attack(Team t, float dt)
	{
		FriendSmartAIData ad    = t.aiData as FriendSmartAIData;
		Vector3 center          = TraversalAllNodeCenter (t);
		List<Node> allNodes     = sceneManager.nodeManager.GetUsefulNodeList();

		// 生成进攻目标
		ad.targetList.Clear ();
		for (int i = 0; i < allNodes.Count; ++i)
		{
			var node = allNodes [i];
			if (!node.CanBeTarget())
				continue;

            if (node.IsOurNode(t.team))
                continue;

            int selfStrength = node.PredictedTeamStrength(t.team);
            int enmyStrength = node.PredictedOppStrength(t.team);
            if (node.team == TEAM.Neutral && selfStrength > 5 && selfStrength > enmyStrength * 2)
				continue;

			Vector3 dp      = node.GetPosition () - center;
            float dist      = dp.magnitude;
			float odds      = enmyStrength - selfStrength;
			node.aiValue    = dist + odds;
			ad.targetList.Add (node);
		}

		if (ad.targetList.Count == 0)
		{
			return false;
		}
		ad.targetList.Sort(ComparisonAIValue);

        
        // 生成己方攻击队伍
		ad.senderList.Clear ();
		for (int i = 0; i < allNodes.Count; ++i)
		{
			var node = allNodes [i];
            if (node.GetAttribute(NodeAttr.Ice) > 0)
                continue;

			if (node.aiTimers[(int)t.team] > 0) continue;
			if (!node.IsOurNode(t.team))
                continue;

            if (node.state == NodeState.Capturing || node.state == NodeState.Occupied)
                continue;

            int nEnemyStrength = node.PredictedOppStrength(t.team);
            if (nEnemyStrength == 0 && node.state == NodeState.Capturing)
				continue;

			/// 仅本身、本城的力量
            int nSelfStrength = node.PredictedTeamStrength(t.team, false );
            if (nSelfStrength < 5)
				continue;

			/// 本阵营的力量
			nSelfStrength	  = node.PredictedTeamStrength(t.team);
            if (nEnemyStrength > 0 && nSelfStrength > nEnemyStrength)
				continue;

            node.aiStrength	  = -nSelfStrength;
			ad.senderList.Add (node);
		}

		if (ad.senderList.Count == 0)
		{
			return false;
		}
		ad.senderList.Sort (ComparsionAIStrength);

		CalcTarget(t, ref ad );

		// 进攻
		//int nTotalTeamStrenth = 0;
		for (int i = 0; i < ad.targetList.Count; ++i)
		{
			Node target = ad.targetList [i];
			for (int j = 0; j < ad.senderList.Count; ++j)
			{
				Node sender = ad.senderList [j];
				if (sender == target)
					continue;

				if (!sender.AICanLink (target, t))
					continue;

				int selfStrength   = sender.PredictedTeamStrength(t.team, false );
                int targetStrength = target.PredictedTeamStrength(t.team);
                int oppStrength    = target.PredictedOppStrength(t.team);
				int towerDmg	   = (int)(GetLengthInTowerRange( t, sender, target ) * 3.3f + 0.5f );
				if (selfStrength + targetStrength > oppStrength)
				{
					int numSend	   = (int)(oppStrength * 2 - targetStrength * 0.5f + 0.5f);
					numSend		   += towerDmg;
					int nBT = sender.CalebeComingBattle(t, numSend - targetStrength );
					if (nBT > 0)
					{
						sender.MoveEffect(target, nBT);
						sender.ResetAiTimer(t.team, ad.aiTimeInterval);
						return true;
					}
				}
			}
		}

		// 集火攻击
		//for( int i = 0; i < ad.targetList.Count; ++i )
  //      {
		//	Node target = ad.targetList[i];
		//	if( nTotalTeamStrenth + target.PredictedTeamStrength(t.team) > target.PredictedOppStrength(t.team ) )
  //          {
		//		for( int j = 0; j < sen)
  //          }
  //      }
		return false;
	}

	/// <summary>
	/// 空闲
	/// </summary>
	private bool Idle( Team t, float dt )
    {
		FriendSmartAIData ad = t.aiData as FriendSmartAIData;
		ad.aiStatus			 = AIStatus.Defend;
		return true;
	}

	/// <summary>
	/// 汇集
	/// </summary>
	private bool Gather( Team t, float dt )
    {
		FriendSmartAIData ad = t.aiData as FriendSmartAIData;
		Vector3 center		 = TraversalAllNodeCenter( t );
		List<Node> allNodes  = sceneManager.nodeManager.GetUsefulNodeList();

		ad.senderList.Clear();
		for( int i = 0; i < allNodes.Count; ++i )
        {
			Node node		= allNodes[i];
			if (node.aiTimers[(int)t.team] > 0) continue;

			int selfCount   = node.PredictedTeamStrength(t.team);
			int entityCount = node.PredictedOppStrength(t.team);
			if (node.PredictedTeamStrength(t.team, false ) < 10)
				continue;
			if (!node.IsOurNode(t.team) && selfCount > entityCount )
				continue;
			if (entityCount > 0 && selfCount > entityCount)
				continue;
			if (node.GetAttribute(NodeAttr.Ice) > 0)
				continue;
			node.aiStrength = -node.PredictedTeamStrength(t.team, false) - entityCount;
			node.aiValue	= -node.GetOppLinks( t, allNodes );
			if( node.nodeType == NodeType.WarpDoor )
            {
				node.aiValue--;
				node.aiValue += node.aiStrength;
            }
			ad.senderList.Add(node);
        }

		if (ad.senderList.Count == 0)
			return false;

		ad.senderList.Sort(ComparsionAIStrength);
		ad.targetList.Clear();
		for(int i = 0; i < allNodes.Count; ++i )
        {
			Node node = allNodes[i];
			if (!node.CanBeTarget())
				continue;
			if (!node.IsOurNode(t.team))
				continue;
			node.aiStrength = -node.PredictedTeamStrength(t.team, false) - node.PredictedOppStrength(t.team);
			node.aiValue	= -node.GetOppLinks(t, allNodes);
			if( node.nodeType == NodeType.WarpDoor )
            {
				node.aiValue--;
				node.aiValue += node.aiStrength;
            }
			ad.targetList.Add(node);
        }

		if (ad.targetList.Count <= 0)
			return false;

		ad.targetList.Sort(ComparisonAIValue);
		CalcTarget(t, ref ad );
		for( int i = 0; i < ad.senderList.Count; ++i )
        {
			Node sender			= ad.senderList[i];
			for( int j = 0; j < ad.targetList.Count; ++j )
            {
				Node target		= ad.targetList[j];
				if (sender == target) continue;
				if (!sender.AICanLink(target, t)) continue;
				if (target.aiValue > sender.aiValue) continue;
				int numSend		= sender.PredictedTeamStrength(t.team, false);
				int towerDmg	= (int)(GetLengthInTowerRange(t, sender, target) * 3.3f + 0.5f);
				numSend += towerDmg;
				if (towerDmg > 0 && sender.PredictedTeamStrength(t.team, false) < towerDmg * 0.5f)
					continue;

				int nBT			= sender.CalebeComingBattle(t, numSend);
				if( nBT > 0 )
                {
					sender.MoveEffect( target, nBT );
					sender.ResetAiTimer(t.team, ad.aiTimeInterval);
					return true;
                }
            }
        }
		return false;
    }

	/// <summary>
	/// 整理期
	/// </summary>
	private bool Rebuild( Team t, float dt )
    {
		FriendSmartAIData ad	 = t.aiData as FriendSmartAIData;
		ad.actionDelayDefend	 += dt;
		if ( ad.actionDelayDefend < ad.rebuildInterval )
        {
			ad.actionDelayDefend = 0;
			return true;
        }
		return false;
    }
	/// <summary>
	/// 计算目标
	/// </summary>
	private List<Node> mTarget = new List<Node>();
	private bool CalcTarget( Team t, ref FriendSmartAIData ad )
    {
		mTarget.Clear();
		for( int i = 0; i < ad.targetList.Count; i++ )
        {
			int j = 0;
			for( j = 0; j < ad.senderList.Count; j++ )
            {
				if (ad.senderList[j].AICanLink(ad.targetList[i], t))
					break;
            }

			if( j < ad.senderList.Count )
            {
				mTarget.Add(ad.targetList[i]);
            }
        }

		if (mTarget.Count == 0) return false;

		Node node  = null;
		int nodes  = mTarget.Count;
		int nValue = BattleSystem.Instance.battleData.rand.Range(1, 15);
		if( nValue >= 1 && nValue <= 9 && nodes >= 1 )
			node   = mTarget[0];
		if( nValue > 9 && nValue <= 14 && nodes >= 2 )
        {
			node   = mTarget[1];
			mTarget[1] = mTarget[0];
			mTarget[0] = node;
        }
		if( nValue > 14 && nValue <= 15 && nodes >= 3 )
        {
			node	= mTarget[2];
			mTarget[2] = mTarget[0];
			mTarget[0] = node;
		}

		ad.targetList.Clear();
		ad.targetList.AddRange( mTarget );
		return true;
    }

	public float GetLengthInTowerRange(Team t, Node arg0, Node arg1)
	{
		float total = 0;
		List<Node> allNodes = sceneManager.nodeManager.GetUsefulNodeList();
		int count = 1;
		for (int i = 0; i < allNodes.Count; ++i)
		{
			var node = allNodes[i];
			if (node.team == t.team)
				continue;
			if (node.nodeType == NodeType.Tower || node.nodeType == NodeType.Castle)
			{
				if (node.IsOurNode(t.team) || node.team == TEAM.Neutral)
					continue;

				Vector3 pa = arg0.GetPosition();
				Vector3 pb = arg1.GetPosition();
				Vector3 pc = node.GetPosition();
				LineIntersectCircle(t, pa, pb, pc, node.GetWidth() * node.GetAttackRage());
				FriendSmartAIData ad = GetAiData(t);
				if (ad.resultIntersects)
				{
					if (ad.resultEnter != Vector3.forward && ad.resultExit != Vector3.forward)
					{
						total += Vector3.Distance(ad.resultEnter, ad.resultExit) * count;
					}
					else if (ad.resultEnter != Vector3.forward && ad.resultExit == Vector3.forward)
					{
						total += Vector3.Distance(ad.resultEnter, pb) * count;
					}
					else if (ad.resultEnter == Vector3.forward && ad.resultExit != Vector3.forward)
					{
						total += Vector3.Distance(pa, ad.resultExit) * count;
					}
					else
					{
						total += Vector3.Distance(pa, pb) * count;
					}
					++count;
				}
				else if (ad.resultInside)
				{
					total += Vector3.Distance(pa, pb) * count;
					++count;
				}
			}
		}
		return total;
	}

	public void LineIntersectCircle(Team t, Vector3 A, Vector3 B, Vector3 C, float r)
	{
		FriendSmartAIData ad	= GetAiData(t);
		ad.resultInside			= false;
		ad.resultIntersects		= false;
		ad.resultEnter			= Vector3.forward;
		ad.resultExit			= Vector3.forward;
		var a					= (B.x - A.x) * (B.x - A.x) + (B.y - A.y) * (B.y - A.y);
		var b					= 2 * ((B.x - A.x) * (A.x - C.x) + (B.y - A.y) * (A.y - C.y));
		var cc					= C.x * C.x + C.y * C.y + A.x * A.x + A.y * A.y - 2 * (C.x * A.x + C.y * A.y) - r * r;
		var deter				= b * b - 4 * a * cc;
		if (deter <= 0)
		{
			ad.resultInside = false;
		}
		else
		{
			var e = Mathf.Sqrt(deter);
			var u1 = (-b + e) / (2 * a);
			var u2 = (-b - e) / (2 * a);
			if ((u1 < 0 || u1 > 1) && (u2 < 0 || u2 > 1))
			{
				if ((u1 < 0 && u2 < 0) || (u1 > 1 && u2 > 1))
				{
					ad.resultInside = false;
				}
				else
				{
					ad.resultInside = true;
				}
			}
			else
			{
				if (0 <= u2 && u2 <= 1)
				{
					ad.resultEnter = Vector3.Lerp(A, B, 1 - u2);
				}
				if (0 <= u1 && u1 <= 1)
				{
					ad.resultExit = Vector3.Lerp(A, B, 1 - u1);
				}
				ad.resultIntersects = true;
			}
		}
	}
}

