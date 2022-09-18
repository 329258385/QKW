using System.Collections;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace Nebukam.ORCA
{
    public class ORCASimulator : Singleton<ORCASimulator>
    {
        protected AgentGroup<Agent>         mAgents = new AgentGroup<Agent>();          // 机器人
        protected ObstacleGroup             mStaticObstacles = new ObstacleGroup();     // 静态阻挡
        protected ObstacleGroup             mDynamicObstacles = new ObstacleGroup();    // 动态阻挡
        public ORCA                         simulation;
        public AgentGroup<Agent>            agents { get { return mAgents; } }
        public ObstacleGroup                staticObstacles { get { return mStaticObstacles; } }
        public ObstacleGroup                dynamicObstacles { get { return mDynamicObstacles; } }


        /// -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 初始化集群寻路接口
        /// </summary>
        /// -------------------------------------------------------------------------------------------------------
        public void InitORCA()
        {
            mAgents                 = new AgentGroup<Agent>();
            mStaticObstacles        = new ObstacleGroup();
            mDynamicObstacles       = new ObstacleGroup();
            simulation              = new ORCA();
            simulation.plane        = AxisPair.XY;
            simulation.agents       = mAgents;
            simulation.staticObstacles     = mStaticObstacles;
            simulation.dynamicObstacles    = mDynamicObstacles;
        }

        /// -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 集群寻路
        /// </summary>
        /// -------------------------------------------------------------------------------------------------------
        public void Tick( float interval)
        {
            Profiler.BeginSample("ORCA.run");
            if(simulation != null )
            {
                simulation.Run(interval);
                simulation.RunComplete();
            }
            Profiler.EndSample();
        }

        /// -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 增加机器人
        /// </summary>
        /// -------------------------------------------------------------------------------------------------------
        public Agent AddAgent( float3 pos )
        {
            return mAgents.Add( pos );
        }

        /// -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 增加机器人
        /// </summary>
        /// -------------------------------------------------------------------------------------------------------
        public void Destroy()
        {
            if(simulation != null )
            {
                simulation.DisposeAll();
                simulation = null;
            }
        }
    }
}