using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using System;
using Unity.Collections;

namespace Nebukam.JobAssist
{

    public interface IProcessor : IDisposable
    {
        
        IProcessorChain chain { get; set; }
        int chainIndex { get; set; }

        bool scheduled { get; }
        bool completed { get; }

        IProcessor procDependency { get; }
        JobHandle currentHandle { get; }
        
        JobHandle Schedule(float delta, IProcessor dependsOn = null);
        JobHandle Schedule(float delta, JobHandle dependsOn);

        void Complete();
        
    }


    public abstract class Processor<T> : IProcessor
        where T : struct, IJob
    {

        protected bool m_hasJobHandleDependency = false;
        protected JobHandle m_jobHandleDependency = default(JobHandle);

        public IProcessorChain m_chain = null;
        public IProcessorChain chain { get { return m_chain; } set { m_chain = value; } }

        public int chainIndex { get; set; } = -1;

        protected IProcessor m_procDependency = null;
        public IProcessor procDependency { get { return m_procDependency; } }

        protected T m_currentJob;
        protected JobHandle m_currentHandle;
        public JobHandle currentHandle { get { return m_currentHandle; } }

        protected bool m_scheduled = false;
        public bool scheduled { get { return m_scheduled; } }        
        public bool completed { get { return m_scheduled ? m_currentHandle.IsCompleted : false; } }

#if UNITY_EDITOR
        protected bool m_disposed = false;
#endif

        /// <summary>
        /// Schedule this job, with an optional dependency.
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

            m_currentJob = new T();
            Prepare(ref m_currentJob, delta);

            if (dependsOn != null)
            {
                m_procDependency = dependsOn;
                m_currentHandle = m_currentJob.Schedule(m_procDependency.currentHandle);
            }
            else
            {
                m_procDependency = null;
                m_currentHandle = m_currentJob.Schedule();
            }

            return m_currentHandle;
        }

        /// <summary>
        /// Schedule this job, with a JobHandle dependency.
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        /// <remark>
        /// This method is provided to support integration in regular Unity's Job System workflow
        /// </remark>
        public JobHandle Schedule(float delta, JobHandle dependsOn )
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

            m_currentJob = new T();
            Prepare(ref m_currentJob, delta);
            
            m_currentHandle = m_currentJob.Schedule(dependsOn);

            return m_currentHandle;
        }

        protected abstract void Prepare(ref T job, float delta);

        /// <summary>
        /// Complete the job.
        /// </summary>
        public void Complete()
        {

#if UNITY_EDITOR
            if (m_disposed)
            {
                throw new Exception("Complete() called on disposed JobHandler ( "+GetType().Name+" ).");
            }
#endif

            if (!m_scheduled) { return; }

            if (m_hasJobHandleDependency)
                m_jobHandleDependency.Complete();

            m_procDependency?.Complete();
            m_currentHandle.Complete();

            m_scheduled = false;

            Apply(ref m_currentJob);

        }

        protected abstract void Apply(ref T job);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) { return; }
#if UNITY_EDITOR
            m_disposed = true;
#endif

            //Complete the job first so we can rid of unmanaged resources.
            if (m_scheduled) { m_currentHandle.Complete(); }

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

        #region utils

        protected bool TryGetFirstInChain<T>(out T processor)
            where T : class, IProcessor
        {
            processor = null;
            if (m_chain == null)
            {
                processor = m_procDependency as T;
                return processor != null;
            }

            for(int i = chainIndex; i >= 0; i--)
            {
                processor = m_chain[i] as T;
                if (processor != null) { return true; }
            }

            return false;
        }

        #endregion

    }

}
