using System;
using System.Collections.Generic;
using Unity.Jobs;

namespace Nebukam.JobAssist
{

    public interface IProcessorChain : IProcessor
    {

    }
    
    public class ProcessorChain : IProcessor, IProcessorChain
    {

        public IProcessorChain m_chain = null;
        public IProcessorChain chain { get { return m_chain; } set { m_chain = value; } }

        public int chainIndex { get; set; } = -1;

        protected List<IProcessor> m_stack = new List<IProcessor>();

        protected bool m_hasJobHandleDependency = false;
        protected JobHandle m_jobHandleDependency = default(JobHandle);

        protected IProcessor m_procDependency = null;
        public IProcessor procDependency { get { return m_procDependency; } }

        //Last dependency of the stack
        protected JobHandle m_currentHandle;
        public JobHandle currentHandle { get { return m_currentHandle; } }

        public void Add(IProcessor proc)
        {
            if (m_stack.Contains(proc)) { return; }
            proc.chain = this;
            m_stack.Add(proc);
        }

        protected bool m_scheduled = false;
        public bool scheduled { get { return m_scheduled; } }
        public bool completed { get { return m_scheduled ? m_currentHandle.IsCompleted : false; } }

#if UNITY_EDITOR
        protected bool m_disposed = false;
#endif

        /// <summary>
        /// Schedule the job tasks.
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public JobHandle Schedule(float delta, IProcessor dependsOn = null)
        {

#if UNITY_EDITOR
            if (m_disposed)
            {
                throw new Exception("Schedule() called on disposed JobHandler ( " + GetType().Name + " ).");
            }
#endif

            if (m_scheduled) { return m_currentHandle; }
            m_scheduled = true;
            m_hasJobHandleDependency = false;

            m_procDependency = dependsOn;

            int count = m_stack.Count;
            IProcessor proc, prevProc = dependsOn;

            for (int i = 0; i < count; i++)
            {
                proc = m_stack[i];
                proc.chainIndex = i;

                if (prevProc == null)
                {
                    m_currentHandle = proc.Schedule(delta);
                }
                else
                {
                    m_currentHandle = proc.Schedule(delta, prevProc);
                }

                prevProc = proc;

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

#if UNITY_EDITOR
            if (m_disposed)
            {
                throw new Exception("Schedule() called on disposed JobHandler ( " + GetType().Name + " ).");
            }
#endif

            if (m_scheduled) { return m_currentHandle; }

            m_scheduled = true;
            m_hasJobHandleDependency = true;
            m_procDependency = null;
            m_jobHandleDependency = dependsOn;

            int count = m_stack.Count;
            IProcessor proc, prevProc = null;

            for (int i = 0; i < count; i++)
            {
                proc = m_stack[i];
                proc.chainIndex = i;

                if (prevProc == null)
                {
                    m_currentHandle = proc.Schedule(delta, m_jobHandleDependency);
                }
                else
                {
                    m_currentHandle = proc.Schedule(delta, prevProc);
                }

                prevProc = proc;

            }

            return m_currentHandle;

        }

        /// <summary>
        /// Complete all the jobs in the stack.
        /// </summary>
        public void Complete()
        {

#if UNITY_EDITOR
            if (m_disposed)
            {
                throw new Exception("Complete() called on disposed JobHandler ( " + GetType().Name + " ).");
            }
#endif

            if (!m_scheduled) { return; }

            m_procDependency?.Complete();

            int count = m_stack.Count;
            for(int i = 0; i < count; i++)
            {
                m_stack[i].Complete();
            }

            m_scheduled = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) { return; }
#if UNITY_EDITOR
            m_disposed = true;
#endif
            m_procDependency = null;
            m_scheduled = false;
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }
}
