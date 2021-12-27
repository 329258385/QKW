using Nebukam.JobAssist;





namespace Nebukam.ORCA
{

    public class ORCA : ProcessorChain, IPlanar
    {

        #region IPlanar

        protected AxisPair m_plane = AxisPair.XY;
        public AxisPair plane
        {
            get { return m_plane; }
            set { m_plane = m_orcaPreparation.plane = m_orcaLines.plane /*= m_orcaApply.plane = m_raycasts.plane*/ = value; }
        }

        #endregion

        /// 
        /// Fields
        /// 
        protected ORCAPreparation       m_orcaPreparation;
        protected ORCALinesProcessor    m_orcaLines;
        protected ORCAApplyProcessor    m_orcaApply;

        /// 
        /// Properties
        /// 
        public IObstacleGroup           staticObstacles { get { return m_orcaPreparation.staticObstacles; } set { m_orcaPreparation.staticObstacles = value; } }
        public IObstacleGroup           dynamicObstacles { get { return m_orcaPreparation.dynamicObstacles; } set { m_orcaPreparation.dynamicObstacles = value; } }
        public IAgentGroup<IAgent>      agents { get { return m_orcaPreparation.agents; } set { m_orcaPreparation.agents = value; } }
      
        public ORCA()
        {
            Add(ref m_orcaPreparation);
            Add(ref m_orcaLines);

            Add(ref m_orcaApply);
        }

        protected override void Apply()
        {
            base.Apply();
        }
    }
}
