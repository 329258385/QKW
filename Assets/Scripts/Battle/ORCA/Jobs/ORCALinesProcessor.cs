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

using Nebukam.JobAssist;
using Unity.Collections;

namespace Nebukam.ORCA
{

    public interface IORCALinesProvider : IProcessor
    {
        IAgentProvider agentProvider { get; }
        NativeArray<AgentDataResult> results { get; }
    }

    public class ORCALinesProcessor : Processor<ORCALinesJob>, IORCALinesProvider
    {

        public AxisPair plane { get; set; } = AxisPair.XY;

        /// 
        /// Fields
        /// 
        /// Job内需要数据的提供者
        protected IAgentProvider                    m_agentProvider;
        protected IAgentKDTreeProvider              m_agentKDTreeProvider;
        protected IStaticObstacleProvider           m_staticObstaclesProvider;
        protected IStaticObstacleKDTreeProvider     m_staticObstacleKDTreeProvider;
        protected IDynObstacleProvider              m_dynObstaclesProvider;
        protected IDynObstacleKDTreeProvider        m_dynObstacleKDTreeProvider;

        /// <summary>
        /// Job 内运临时的内存
        /// </summary>
        protected NativeList<DVP>                   agentNeighbors = new NativeList<DVP>(10, Allocator.Persistent);
        protected NativeList<DVP>                   dynObstacleNeighbors = new NativeList<DVP>(10, Allocator.Persistent);
        protected NativeList<DVP>                   staticObstacleNeighbors = new NativeList<DVP>(10, Allocator.Persistent);
        protected NativeList<ORCALine>              orcaLines = new NativeList<ORCALine>(100, Allocator.Persistent);
        protected NativeList<ORCALine>              projLines = new NativeList<ORCALine>(100, Allocator.Persistent);


        /// <summary>
        /// Job 返回的结果
        /// </summary>
        protected NativeArray<AgentDataResult>      m_results = new NativeArray<AgentDataResult>(0, Allocator.Persistent);
       
        /// 
        /// Properties
        /// 

        public IAgentProvider               agentProvider { get { return m_agentProvider; } }
        public NativeArray<AgentDataResult> results { get { return m_results; } }

        protected override void             InternalLock() { }
        protected override void             InternalUnlock() { }

        protected override void Prepare(ref ORCALinesJob job, float delta)
        {

            if (!TryGetFirstInGroup(out m_agentProvider, true)
                || !TryGetFirstInGroup(out m_agentKDTreeProvider, true)
                || !TryGetFirstInGroup(out m_staticObstaclesProvider, true)
                || !TryGetFirstInGroup(out m_staticObstacleKDTreeProvider, true)
                || !TryGetFirstInGroup(out m_dynObstaclesProvider, true)
                || !TryGetFirstInGroup(out m_dynObstacleKDTreeProvider, true))
            {
                string msg = string.Format("Missing provider : Agents = {0}, Static obs = {1}, Agent KD = {2}, Static obs KD= {3}, " +
                    "Dyn obs = {5}, Dyn obs KD= {6}, group = {4}",
                    m_agentProvider,
                    m_staticObstaclesProvider,
                    m_agentKDTreeProvider,
                    m_staticObstacleKDTreeProvider,
                    m_dynObstaclesProvider,
                    m_dynObstacleKDTreeProvider, m_group);

                throw new System.Exception(msg);
            }

            int agentCount = m_agentProvider.outputAgents.Length;
            if (m_results.Length != agentCount)
            {
                m_results.Dispose();
                m_results = new NativeArray<AgentDataResult>(agentCount, Allocator.Persistent);
            }

            //Agent data
            job.m_inputAgents               = m_agentProvider.outputAgents;
            job.m_inputAgentTree            = m_agentKDTreeProvider.outputTree;

            //Static obstacles data
            job.m_staticObstacleInfos       = m_staticObstaclesProvider.outputObstacleInfos;
            job.m_staticRefObstacles        = m_staticObstaclesProvider.referenceObstacles;
            job.m_staticObstacles           = m_staticObstaclesProvider.outputObstacles;
            job.m_staticObstacleTree        = m_staticObstacleKDTreeProvider.outputTree;

            //Dynamic obstacles data
            job.m_dynObstacleInfos          = m_dynObstaclesProvider.outputObstacleInfos;
            job.m_dynRefObstacles           = m_dynObstaclesProvider.referenceObstacles;
            job.m_dynObstacles              = m_dynObstaclesProvider.outputObstacles;
            job.m_dynObstacleTree           = m_dynObstacleKDTreeProvider.outputTree;

            // job 内临时数据
            job.agentNeighbors              = agentNeighbors;
            job.dynObstacleNeighbors        = dynObstacleNeighbors;
            job.staticObstacleNeighbors     = staticObstacleNeighbors;
            job.m_orcaLines                 = orcaLines;
            job.projLines                   = projLines;

            job.m_results                   = m_results;
            job.m_timestep                  = delta;// / 0.25f;
        }

        protected override void Apply(ref ORCALinesJob job)
        {
            
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) { return; }

            agentNeighbors.Dispose();
            dynObstacleNeighbors.Dispose();
            staticObstacleNeighbors.Dispose();
            orcaLines.Dispose();
            projLines.Dispose();
            m_results.Dispose();
        }

    }
}
