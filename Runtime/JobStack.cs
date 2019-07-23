using System.Collections.Generic;
using Unity.Jobs;

namespace Nebukam.JobAssist
{
    public class JobStack : IJobHandler
    {

        protected List<IJobHandler> m_stack = new List<IJobHandler>();

        protected bool m_hasJobHandleDependency = false;
        protected JobHandle m_jobHandleDependency = default(JobHandle);

        protected IJobHandler m_jobDependency = null;
        public IJobHandler jobDependency { get { return m_jobDependency; } }

        //Last dependency of the stack
        protected JobHandle m_currentHandle;
        public JobHandle currentHandle { get { return m_currentHandle; } }

        public void Add(IJobHandler job)
        {
            if (m_stack.Contains(job)) { return; }
            m_stack.Add(job);
        }

        protected bool m_scheduled = false;
        
        /// <summary>
        /// Schedule the job tasks.
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public JobHandle Schedule(float delta, IJobHandler dependsOn = null)
        {

            if (m_scheduled) { return m_currentHandle; }
            m_scheduled = true;
            m_hasJobHandleDependency = false;

            m_jobDependency = dependsOn;

            int count = m_stack.Count;
            IJobHandler job, prevJob = dependsOn;

            for (int i = 0; i < count; i++)
            {
                job = m_stack[i];

                if (prevJob == null)
                {
                    m_currentHandle = job.Schedule(delta);
                }
                else
                {
                    m_currentHandle = job.Schedule(delta, prevJob);
                }

                prevJob = job;

            }

            return m_currentHandle;

        }

        /// <summary>
        /// Schedule this job stack, with a JobHandle dependency.
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        /// <remark>
        /// This method is provided to support integration in regular Unity's Job System workflow
        /// </remark>
        public JobHandle Schedule(float delta, JobHandle dependsOn)
        {

            if (m_scheduled) { return m_currentHandle; }

            m_scheduled = true;
            m_hasJobHandleDependency = true;
            m_jobDependency = null;
            m_jobHandleDependency = dependsOn;

            int count = m_stack.Count;
            IJobHandler job, prevJob = null;

            for (int i = 0; i < count; i++)
            {
                job = m_stack[i];

                if (prevJob == null)
                {
                    m_currentHandle = job.Schedule(delta, m_jobHandleDependency);
                }
                else
                {
                    m_currentHandle = job.Schedule(delta, prevJob);
                }

                prevJob = job;

            }

            return m_currentHandle;

        }

        /// <summary>
        /// Complete all the jobs in the stack.
        /// </summary>
        public void Complete()
        {
            if (!m_scheduled) { return; }

            m_jobDependency?.Complete();

            int count = m_stack.Count;
            for(int i = 0; i < count; i++)
            {
                m_stack[i].Complete();
            }

            m_scheduled = false;
        }
    }
}
