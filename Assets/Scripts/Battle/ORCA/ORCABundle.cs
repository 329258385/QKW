using Unity.Mathematics;





namespace Nebukam.ORCA
{
    public class ORCABundle<AgentType> : System.IDisposable, IPlanar
        where AgentType : Agent, IAgent, new()
    {

        protected AxisPair m_plane = AxisPair.XY;
        protected AgentGroup<AgentType> m_agents = new AgentGroup<AgentType>();
        protected ObstacleGroup m_staticObstacles = new ObstacleGroup();
        protected ObstacleGroup m_dynamicObstacles = new ObstacleGroup();
        protected ORCA m_orca;

        public AxisPair plane
        {
            get { return m_plane; }
            set
            {
                m_plane = value;
                m_orca.plane = value;
            }
        }
        public AgentGroup<AgentType> agents { get { return m_agents; } }
        public ObstacleGroup staticObstacles { get { return m_staticObstacles; } }
        public ObstacleGroup dynamicObstacles { get { return m_dynamicObstacles; } }
        public ORCA orca { get { return m_orca; } }

        public ORCABundle()
        {
            m_orca                  = new Nebukam.ORCA.ORCA();
            m_orca.plane            = m_plane;

            m_orca.agents           = m_agents;
            m_orca.staticObstacles  = m_staticObstacles;
            m_orca.dynamicObstacles = m_dynamicObstacles;
        }

        public AgentType NewAgent(float3 position)
        {
            return m_agents.Add(position) as AgentType;
        }

        #region System.IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) { return; }

            m_orca.DisposeAll();
            m_agents.Clear(true);
            m_staticObstacles.Clear(true);
            m_dynamicObstacles.Clear(true);

        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        #endregion

    }
}
