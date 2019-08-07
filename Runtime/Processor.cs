using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using System;
using Unity.Collections;

namespace Nebukam.JobAssist
{

    public interface IProcessor : IDisposable, ILockable
    {
        
        float deltaMultiplier { get; set; }

        IProcessorGroup group { get; set; }
        int groupIndex { get; set; }

        bool scheduled { get; }
        bool completed { get; }

        IProcessor procDependency { get; }
        JobHandle currentHandle { get; }
        
        JobHandle Schedule(float delta, IProcessor dependsOn = null);
        JobHandle Schedule(float delta, JobHandle dependsOn);

        /// <summary>
        /// Complete the job.
        /// </summary>
        void Complete();

        /// <summary>
        /// Complete the job only if it is finished.
        /// Return false if the job hasn't been scheduled.
        /// </summary>
        /// <returns>Whether the job has been completed or not</returns>
        bool TryComplete();
        
    }


    public abstract class Processor<T> : IProcessor
        where T : struct, IJob
    {

        public float deltaMultiplier { get; set; } = 1.0f;

        protected bool m_locked = false;
        public bool locked { get { return m_locked; } }

        protected bool m_hasJobHandleDependency = false;
        protected JobHandle m_jobHandleDependency = default(JobHandle);

        public IProcessorGroup m_group = null;
        public IProcessorGroup group { get { return m_group; } set { m_group = value; } }

        public int groupIndex { get; set; } = -1;

        protected IProcessor m_procDependency = null;
        public IProcessor procDependency { get { return m_procDependency; } }

        protected T m_currentJob;
        protected JobHandle m_currentHandle;
        public JobHandle currentHandle { get { return m_currentHandle; } }

        protected float m_deltaSum = 0f;

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
        public virtual JobHandle Schedule(float delta, IProcessor dependsOn = null)
        {

#if UNITY_EDITOR
            if (m_disposed)
            {
                throw new Exception("Schedule() called on disposed JobHandler ( " + GetType().Name + " ).");
            }
#endif
            m_deltaSum += delta;

            if (m_scheduled) { return m_currentHandle; }

            m_scheduled = true;
            m_hasJobHandleDependency = false;

            m_currentJob = new T();

            Lock();
            Prepare(ref m_currentJob, m_deltaSum * deltaMultiplier);

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
        public virtual JobHandle Schedule(float delta, JobHandle dependsOn )
        {

#if UNITY_EDITOR
            if (m_disposed)
            {
                throw new Exception("Schedule() called on disposed JobHandler ( " + GetType().Name + " ).");
            }
#endif
            m_deltaSum += delta;

            if (m_scheduled) { return m_currentHandle; }

            m_scheduled = true;
            m_hasJobHandleDependency = true;
            m_procDependency = null;

            m_currentJob = new T();

            Lock();
            Prepare(ref m_currentJob, m_deltaSum * deltaMultiplier);
            
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
            Unlock();

            m_deltaSum = 0f;

        }

        public bool TryComplete()
        {
            if (!m_scheduled) { return false; }
            if (completed)
            {
                Complete();
                return true;
            }
            else
            {
                return false;
            }
        }

        protected abstract void Apply(ref T job);

        #region ILockable

        public void Lock()
        {
            if (m_locked) { return; }
            m_locked = true;
            InternalLock();
        }

        protected abstract void InternalLock();

        public void Unlock()
        {
            if (!m_locked) { return; }
            m_locked = false;
            //Complete the job for safety
            if (m_scheduled) { Complete(); }
            InternalUnlock();

        }

        protected abstract void InternalUnlock();

        #endregion

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

        protected bool TryGetFirstInGroup<P>(out P processor, bool deep = false)
            where P : class, IProcessor
        {

            processor = null;
            if(m_group != null)
            {
                return m_group.TryGetFirst(groupIndex-1, out processor, deep);
            }
            else
            {
                return false;
            }
        }

        #endregion

    }

}
