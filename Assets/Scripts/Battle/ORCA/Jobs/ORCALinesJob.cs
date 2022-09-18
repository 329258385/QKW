// Copyright (c) 2019 Timothé Lapetite - nebukam@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Nebukam.ORCA
{

    public struct DVP
    {
        public float distSq;
        public int index;
        public DVP(float dist, int i)
        {
            distSq = dist;
            index = i;
        }
    }

    public struct ORCALine
    {
        public float2 dir;
        public float2 point;
    }


    //[BurstCompile]
    public struct ORCALinesJob : IJob
    {

        const float EPSILON = 0.00001f;

        [ReadOnly]
        public NativeArray<AgentData>               m_inputAgents;
        [ReadOnly]
        public NativeArray<AgentTreeNode>           m_inputAgentTree;

        /// <summary>
        /// 静态障碍数据，外围输入
        /// </summary>
        [ReadOnly]
        public NativeArray<ObstacleInfos>           m_staticObstacleInfos;
        [ReadOnly]
        public NativeArray<ObstacleVertexData>      m_staticRefObstacles;
        [ReadOnly]
        public NativeArray<ObstacleVertexData>      m_staticObstacles;
        [ReadOnly]
        public NativeArray<ObstacleTreeNode>        m_staticObstacleTree;

        /// <summary>
        /// 动态障碍数据，外围输入
        /// </summary>
        [ReadOnly]
        public NativeArray<ObstacleInfos>           m_dynObstacleInfos;
        [ReadOnly]
        public NativeArray<ObstacleVertexData>      m_dynRefObstacles;
        [ReadOnly]
        public NativeArray<ObstacleVertexData>      m_dynObstacles;
        [ReadOnly]
        public NativeArray<ObstacleTreeNode>        m_dynObstacleTree;

        public NativeArray<AgentDataResult>         m_results;
        public float m_timestep;

        /// <summary>
        /// 临时内存数组
        /// </summary>
        public NativeList<DVP>                      agentNeighbors;
        public NativeList<DVP>                      dynObstacleNeighbors;
        public NativeList<DVP>                      staticObstacleNeighbors;
        public NativeList<ORCALine>                 m_orcaLines;
        public NativeList<ORCALine>                 projLines;

        /// <summary>
        /// 防止运行时清理内存，实现内存复用
        /// </summary>
        public int                                  agentIndex;
        public int                                  dynObstacleIndex;
        public int                                  staticObstacleIndex;
        public int                                  orcaLinesIndex;
        public int                                  proLinesIndex;


        public void Execute()
        {
            AgentDataResult result = new AgentDataResult();
            for (int index = 0; index < m_inputAgents.Length; index++)
            {
                AgentData agent         = m_inputAgents[index];
                if (agent.maxNeighbors == 0 || agent.navigationEnabled == 0 )
                {
                    result.position     = agent.position;
                    result.velocity     = agent.velocity;
                    m_results[index]    = result;
                    continue;
                }

                AgentData otherAgent;
                float2  a_position      = agent.position,
                        a_prefVelocity  = agent.prefVelocity,
                        a_velocity      = agent.velocity,
                        a_newVelocity   = a_prefVelocity;

                float   a_maxSpeed      = agent.maxSpeed,
                        a_radius        = agent.radius,
                        a_timeHorizon   = agent.timeHorizon,
                        rangeSq         = lengthsq(agent.radius + agent.neighborDist);

                int numObstLines = 0;

                /// 机器人
                #region agents
                agentIndex = 0;
                QueryAgentTreeRecursive( ref a_position,
                                         ref agent,
                                         ref rangeSq, 0,
                                         ref agentNeighbors);

                float invTimeHorizon = 1.0f / a_timeHorizon;
                for (int i = 0; i < agentIndex; ++i)
                {

                    otherAgent      = m_inputAgents[agentNeighbors[i].index];
                    float2 relPos   = otherAgent.position - a_position;
                    float2 relVel   = a_velocity - otherAgent.velocity;
                    float distSq    = lengthsq(relPos);
                    if( distSq < 0.0001f )
                    {
                        relVel      = new float2(0.1f, 0.1f);
                    }
                    float cRad      = a_radius + otherAgent.radius;
                    float cRadSq    = lengthsq(cRad);

                    ORCALine line   = new ORCALine();
                    float2 u;
                    if (distSq > cRadSq)
                    {
                        // No collision.
                        float2 w            = relVel - invTimeHorizon * relPos;

                        // Vector from cutoff center to relative velocity.
                        float wLengthSq     = lengthsq(w);
                        float dotProduct1   = dot(w, relPos);
                        if (dotProduct1 < 0.0f && lengthsq(dotProduct1) > cRadSq * wLengthSq)
                        {
                            // Project on cut-off circle.
                            float wLength   = sqrt(wLengthSq);
                            float2 unitW    = w / wLength;
                            line.dir        = float2(unitW.y, -unitW.x);
                            u               = (cRad * invTimeHorizon - wLength) * unitW;
                        }
                        else
                        {
                            // Project on legs.
                            float leg = sqrt(distSq - cRadSq);
                            if (Det(relPos, w) > 0.0f)
                            {
                                // Project on left leg.
                                line.dir = RoundMath(float2(relPos.x * leg - relPos.y * cRad, relPos.x * cRad + relPos.y * leg) / distSq);
                            }
                            else
                            {
                                // Project on right leg.
                                line.dir = -RoundMath(float2(relPos.x * leg + relPos.y * cRad, -relPos.x * cRad + relPos.y * leg) / distSq);
                            }

                            float dotProduct2   = dot(relVel, line.dir);
                            u                   = dotProduct2 * line.dir - relVel;
                        }
                    }
                    else
                    {
                        // Collision. Project on cut-off circle of time timeStep.
                        float invTimeStep   = 1.0f / m_timestep;

                        // Vector from cutoff center to relative velocity.
                        float2 w            = relVel - invTimeStep * relPos;

                        float wLength       = length(w);
                        float2 unitW        = w / wLength;

                        line.dir            = RoundMath(float2(unitW.y, -unitW.x));
                        u                   = (cRad * invTimeStep - wLength) * unitW;
                    }

                    line.point              = RoundMath(a_velocity + 0.5f * u);
                    if( orcaLinesIndex >= m_orcaLines.Length)
                    {
                        m_orcaLines.Add(line);
                    }
                    else 
                    {
                        m_orcaLines[orcaLinesIndex] = line;
                    }
                    orcaLinesIndex++;
                }
                #endregion

                /// 更新位置
                #region Compute new velocity
                int lineFail = LP2(m_orcaLines, a_maxSpeed, a_prefVelocity, false, ref a_newVelocity);
                if (lineFail < orcaLinesIndex)
                    LP3(m_orcaLines, numObstLines, lineFail, a_maxSpeed, ref a_newVelocity);

                result.velocity     = RoundMath(a_newVelocity);
                result.position     = a_position + result.velocity * m_timestep;
                m_results[index]    = result;
                #endregion
            }
        }

        #region Agent KDTree Query

        /// <summary>
        /// Recursive method for computing the agent neighbors of the specified agent.
        /// </summary>
        /// <param name="agent">The agent for which agent neighbors are to be computed.</param>
        /// <param name="agent">The agent making the initial query</param>
        /// <param name="rangeSq">The squared range around the agent.</param>
        /// <param name="node">The current agent k-D tree node index.</param>
        /// <param name="agentNeighbors">The list of neighbors to be filled up.</param>
        private void QueryAgentTreeRecursive(ref float2 center, ref AgentData agent, ref float rangeSq, int node, ref NativeList<DVP> agentNeighbors)
        {

            AgentTreeNode treeNode = m_inputAgentTree[node];
            if (treeNode.end - treeNode.begin <= AgentTreeNode.MAX_LEAF_SIZE)
            {
                AgentData a;
                float bottom = agent.baseline, top = bottom + agent.height;
                for (int i = treeNode.begin; i < treeNode.end; ++i)
                {
                    a = m_inputAgents[i];

                    //if (a.index == agent.index
                    //    || a.collisionEnabled == 0
                    //    || (a.layerOccupation & ~agent.layerIgnore) == 0
                    //    || (top < a.baseline || bottom > a.baseline + a.height))
                    //{
                    //    continue;
                    //}
                    if (a.index == agent.index || a.collisionEnabled == 0 )
                    {
                        continue;
                    }

                    float distSq = lengthsq(center - a.position);
                    if (distSq < rangeSq)
                    {
                        if (agentIndex < agent.maxNeighbors)
                        {
                            int nLen = agentNeighbors.Length;
                            if( agentIndex >= nLen )
                            {
                                agentNeighbors.Add(new DVP(distSq, i));
                            }
                            else
                                agentNeighbors[agentIndex] = new DVP(distSq, i);
                            agentIndex++;
                        }

                        int j = agentIndex - 1;
                        while (j != 0 && distSq < agentNeighbors[j - 1].distSq)
                        {
                            agentNeighbors[j] = agentNeighbors[j - 1];
                            --j;
                        }

                        agentNeighbors[j] = new DVP(distSq, i);
                        if (agentIndex == agent.maxNeighbors)
                        {
                            rangeSq = agentNeighbors[agentIndex - 1].distSq;
                        }
                    }
                }
            }
            else
            {

                AgentTreeNode leftNode = m_inputAgentTree[treeNode.left], rightNode = m_inputAgentTree[treeNode.right];

                float distSqLeft = lengthsq(max(0.0f, leftNode.minX - center.x))
                                   + lengthsq(max(0.0f, center.x - leftNode.maxX))
                                   + lengthsq(max(0.0f, leftNode.minY - center.y))
                                   + lengthsq(max(0.0f, center.y - leftNode.maxY));

                float distSqRight = lengthsq(max(0.0f, rightNode.minX - center.x))
                                    + lengthsq(max(0.0f, center.x - rightNode.maxX))
                                    + lengthsq(max(0.0f, rightNode.minY - center.y))
                                    + lengthsq(max(0.0f, center.y - rightNode.maxY));

                if (distSqLeft < distSqRight)
                {
                    if (distSqLeft < rangeSq)
                    {
                        QueryAgentTreeRecursive(ref center, ref agent, ref rangeSq, treeNode.left, ref agentNeighbors);
                        if (distSqRight < rangeSq)
                        {
                            QueryAgentTreeRecursive(ref center, ref agent, ref rangeSq, treeNode.right, ref agentNeighbors);
                        }
                    }
                }
                else
                {
                    if (distSqRight < rangeSq)
                    {
                        QueryAgentTreeRecursive(ref center, ref agent, ref rangeSq, treeNode.right, ref agentNeighbors);

                        if (distSqLeft < rangeSq)
                        {
                            QueryAgentTreeRecursive(ref center, ref agent, ref rangeSq, treeNode.left, ref agentNeighbors);
                        }
                    }
                }

            }

        }

        #endregion

        
        #region Linear programs

        /// <summary>
        /// Solves a one-dimensional linear program on a specified line subject to linear 
        /// constraints defined by lines and a circular constraint.
        /// </summary>
        /// <param name="lines">Lines defining the linear constraints.</param>
        /// <param name="lineNo">The specified line constraint.</param>
        /// <param name="radius">The radius of the circular constraint.</param>
        /// <param name="optVel">The optimization velocity.</param>
        /// <param name="dirOpt">True if the direction should be optimized.</param>
        /// <param name="result">A reference to the result of the linear program.</param>
        /// <returns>True if successful.</returns>
        private bool LP1(NativeList<ORCALine> lines, int lineNo, float radius, float2 optVel, bool dirOpt, ref float2 result)
        {

            ORCALine line = lines[lineNo];
            float2 dir = line.dir, pt = line.point;

            float dotProduct = dot(pt, dir);
            float discriminant = lengthsq(dotProduct) + lengthsq(radius) - lengthsq(pt);

            if (discriminant < 0.0f)
            {
                // Max speed circle fully invalidates line lineNo.
                return false;
            }

            ORCALine lineA;
            float2 dirA, ptA;

            float sqrtDiscriminant = sqrt(discriminant);
            float tLeft = -dotProduct - sqrtDiscriminant;
            float tRight = -dotProduct + sqrtDiscriminant;

            for (int i = 0; i < lineNo; ++i)
            {

                lineA = lines[i]; dirA = lineA.dir; ptA = lineA.point;

                float denominator = Det(dir, dirA);
                float numerator = Det(dirA, pt - ptA);

                if (abs(denominator) <= EPSILON)
                {
                    // Lines lineNo and i are (almost) parallel.
                    if (numerator < 0.0f)
                    {
                        return false;
                    }

                    continue;
                }

                float t = numerator / denominator;

                if (denominator >= 0.0f)
                {
                    // Line i bounds line lineNo on the right.
                    tRight = min(tRight, t);
                }
                else
                {
                    // Line i bounds line lineNo on the left.
                    tLeft = max(tLeft, t);
                }

                if (tLeft > tRight)
                {
                    return false;
                }
            }

            if (dirOpt)
            {
                // Optimize direction.
                if (dot(optVel, dir) > 0.0f)
                {
                    // Take right extreme.
                    result = pt + tRight * dir;
                }
                else
                {
                    // Take left extreme.
                    result = pt + tLeft * dir;
                }
            }
            else
            {
                // Optimize closest point.
                float t = dot(dir, (optVel - pt));

                if (t < tLeft)
                {
                    result = pt + tLeft * dir;
                }
                else if (t > tRight)
                {
                    result = pt + tRight * dir;
                }
                else
                {
                    result = pt + t * dir;
                }
            }

            return true;
        }

        /// <summary>
        /// Solves a two-dimensional linear program subject to linear 
        /// constraints defined by lines and a circular constraint.
        /// </summary>
        /// <param name="lines">Lines defining the linear constraints.</param>
        /// <param name="radius">The radius of the circular constraint.</param>
        /// <param name="optVel">The optimization velocity.</param>
        /// <param name="dirOpt">True if the direction should be optimized.</param>
        /// <param name="result">A reference to the result of the linear program.</param>
        /// <returns>The number of the line it fails on, and the number of lines if successful.</returns>
        private int LP2(NativeList<ORCALine> lines, float radius, float2 optVel, bool dirOpt, ref float2 result)
        {
            if (dirOpt)
            {
                // Optimize direction. Note that the optimization velocity is of
                // unit length in this case.
                result = optVel * radius;
            }
            else if (lengthsq(optVel) > (radius * radius))
            {
                // Optimize closest point and outside circle.
                result = normalize(optVel) * radius;
            }
            else
            {
                // Optimize closest point and inside circle.
                result = optVel;
            }

            for (int i = 0, count = orcaLinesIndex; i < count; ++i)
            {
                if (Det(lines[i].dir, lines[i].point - result) > 0.0f)
                {
                    // Result does not satisfy constraint i. Compute new optimal result.
                    float2 tempResult = result;
                    if (!LP1(lines, i, radius, optVel, dirOpt, ref result))
                    {
                        result = tempResult;
                        return i;
                    }
                }
            }

            return orcaLinesIndex;
        }

        private int LP4(NativeList<ORCALine> lines, float radius, float2 optVel, bool dirOpt, ref float2 result)
        {
            if (dirOpt)
            {
                // Optimize direction. Note that the optimization velocity is of
                // unit length in this case.
                result = optVel * radius;
            }
            else if (lengthsq(optVel) > (radius * radius))
            {
                // Optimize closest point and outside circle.
                result = normalize(optVel) * radius;
            }
            else
            {
                // Optimize closest point and inside circle.
                result = optVel;
            }

            for (int i = 0, count = proLinesIndex; i < count; ++i)
            {
                if (Det(lines[i].dir, lines[i].point - result) > 0.0f)
                {
                    // Result does not satisfy constraint i. Compute new optimal result.
                    float2 tempResult = result;
                    if (!LP1(lines, i, radius, optVel, dirOpt, ref result))
                    {
                        result = tempResult;
                        return i;
                    }
                }
            }

            return proLinesIndex;
        }

        /// <summary>
        /// Solves a two-dimensional linear program subject to linear
        /// constraints defined by lines and a circular constraint.
        /// </summary>
        /// <param name="lines">Lines defining the linear constraints.</param>
        /// <param name="numObstLines">Count of obstacle lines.</param>
        /// <param name="beginLine">The line on which the 2-d linear program failed.</param>
        /// <param name="radius">The radius of the circular constraint.</param>
        /// <param name="result">A reference to the result of the linear program.</param>
        private void LP3(NativeList<ORCALine> lines, int numObstLines, int beginLine, float radius, ref float2 result)
        {
            float distance = 0.0f;

            ORCALine lineA, lineB;
            float2 dirA, ptA, dirB, ptB;

            for (int i = beginLine, iCount = lines.Length; i < iCount; ++i)
            {
                lineA = lines[i]; dirA = lineA.dir; ptA = lineA.point;

                if (Det(dirA, ptA - result) > distance)
                {
                    // Result does not satisfy constraint of line i.
                    //NativeList<ORCALine> projLines = new NativeList<ORCALine>(numObstLines, Allocator.Temp);
                    proLinesIndex = 0;
                    for (int ii = 0; ii < numObstLines; ++ii)
                    {
                        if(proLinesIndex >= projLines.Length)
                        {
                            projLines.Add(lines[ii]);
                        }
                        else
                        {
                            projLines[proLinesIndex] = lines[ii];
                        }

                        proLinesIndex++;
                    }

                    for (int j = numObstLines; j < i; ++j)
                    {

                        lineB = lines[j]; dirB = lineB.dir; ptB = lineB.point;

                        ORCALine line = new ORCALine();
                        float determinant = Det(dirA, dirB);

                        if (abs(determinant) <= EPSILON)
                        {
                            // Line i and line j are parallel.
                            if (dot(dirA, dirB) > 0.0f)
                            {
                                // Line i and line j point in the same direction.
                                continue;
                            }
                            else
                            {
                                // Line i and line j point in opposite direction.
                                line.point = 0.5f * (ptA + ptB);
                            }
                        }
                        else
                        {
                            line.point = ptA + (Det(dirB, ptA - ptB) / determinant) * dirA;
                        }

                        line.dir = normalize(dirB - dirA);
                        if (proLinesIndex >= projLines.Length)
                        {
                            projLines.Add(line);
                        }
                        else
                        {
                            projLines[proLinesIndex] = line;
                        }
                        proLinesIndex++;
                    }

                    float2 tempResult = result;
                    if (LP4(projLines, radius, float2(-dirA.y, dirA.x), true, ref result) < proLinesIndex)
                    {
                        // This should in principle not happen. The result is by
                        // definition already in the feasible region of this
                        // linear program. If it fails, it is due to small
                        // floating point error, and the current result is kept.
                        result = tempResult;
                    }

                    distance = Det(dirA, ptA - result);

                    //projLines.Dispose(); //Burst doesn't like this.
                }
            }
        }

        #endregion

        #region maths

        /// <summary>
        /// Computes the determinant of a two-dimensional square matrix 
        /// with rows consisting of the specified two-dimensional vectors.
        /// </summary>
        /// <param name="a">The top row of the two-dimensional square matrix</param>
        /// <param name="b">The bottom row of the two-dimensional square matrix</param>
        /// <returns>The determinant of the two-dimensional square matrix.</returns>
        private float Det(float2 a, float2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        private float2 RoundMath( float2 value )
        {
            value *= 1000;
            return value * 0.001f;
        }
        #endregion
    }
}
