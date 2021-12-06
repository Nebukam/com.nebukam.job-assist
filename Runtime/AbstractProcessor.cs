using System;
using Unity.Collections;
using Unity.Jobs;

namespace Nebukam.JobAssist
{

    public interface IProcessor : IDisposable, ILockable
    {

        float deltaMultiplier { get; set; }

        IProcessorCompound compound { get; set; }
        int compoundIndex { get; set; }

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

    public abstract class AbstractProcessor : IProcessor
    {

        public float deltaMultiplier { get; set; } = 1.0f;

        protected bool m_locked = false;
        public bool locked { get { return m_locked; } }

        protected IProcessorCompound m_compound = null;
        public IProcessorCompound compound { get { return m_compound; } set { m_compound = value; } }

        public int compoundIndex { get; set; } = -1;

        protected bool m_hasJobHandleDependency = false;
        protected JobHandle m_jobHandleDependency = default(JobHandle);

        protected IProcessor m_procDependency = null;
        public IProcessor procDependency { get { return m_procDependency; } }

        protected JobHandle m_currentHandle;
        public JobHandle currentHandle { get { return m_currentHandle; } }

        protected float m_lockedDelta = 0f;
        protected float m_scaledLockedDelta = 0f;
        protected float m_deltaSum = 0f;

        protected bool m_scheduled = false;
        public bool scheduled { get { return m_scheduled; } }
        public bool completed { get { return m_scheduled ? m_currentHandle.IsCompleted : false; } }

#if UNITY_EDITOR
        protected bool m_disposed = false;
#endif

        #region Scheduling

        public JobHandle Schedule(float delta, IProcessor dependsOn = null)
        {

#if UNITY_EDITOR
            if (m_disposed)
            {
                throw new Exception("Schedule() called on disposed Processor ( " + GetType().Name + " ).");
            }
#endif
            m_deltaSum += delta;

            if (m_scheduled) { return m_currentHandle; }

            m_scheduled = true;
            m_hasJobHandleDependency = false;
            m_procDependency = dependsOn;

            Lock();

            OnPrepare();

            m_currentHandle = OnScheduled(m_procDependency);

            return m_currentHandle;

        }

        public JobHandle Schedule(float delta, JobHandle dependsOn)
        {

#if UNITY_EDITOR
            if (m_disposed)
            {
                throw new Exception("Schedule() called on disposed Processor ( " + GetType().Name + " ).");
            }
#endif
            m_deltaSum += delta;

            if (m_scheduled) { return m_currentHandle; }

            m_scheduled = true;
            m_hasJobHandleDependency = true;
            m_jobHandleDependency = dependsOn;
            m_procDependency = null;

            Lock();

            OnPrepare();

            m_currentHandle = OnScheduled(dependsOn);

            return m_currentHandle;
        }

        internal abstract void OnPrepare();

        internal abstract JobHandle OnScheduled(IProcessor dependsOn = null);

        internal abstract JobHandle OnScheduled(JobHandle dependsOn);

        #endregion

        #region Complete & Apply

        /// <summary>
        /// Complete the job.
        /// </summary>
        public void Complete()
        {

#if UNITY_EDITOR
            if (m_disposed)
            {
                throw new Exception("Complete() called on disposed Processor ( " + GetType().Name + " ).");
            }
#endif

            if (!m_scheduled) { return; }

            // Complete dependencies

            if (m_hasJobHandleDependency)
                m_jobHandleDependency.Complete();

            m_procDependency?.Complete();

            // Complete self

            OnCompleteBegins();
            
            m_scheduled = false;
            
            OnCompleteEnds();

            Unlock();

        }

        protected abstract void OnCompleteBegins();
        //m_currentHandle.Complete(); // in Processor & Parallel processor

        protected abstract void OnCompleteEnds();
        // Apply(ref m_currentJob); // in Processor & Parallel processor
        // protected abstract void Apply(ref T job); // in Processor & Parallel processor

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
        

        #endregion

        #region ILockable

        public virtual void Lock()
        {
            if (m_locked) { return; }
            m_lockedDelta = m_deltaSum;
            m_scaledLockedDelta = m_lockedDelta * deltaMultiplier;
            m_deltaSum = 0f;
            InternalLock();
            m_locked = true;
        }

        protected abstract void InternalLock();

        public virtual void Unlock()
        {
            if (!m_locked) { return; }
            m_locked = false;
            if (m_scheduled) { Complete(); } //Complete the job for safety
            InternalUnlock();
        }

        protected abstract void InternalUnlock();

        #endregion

        #region Hierarchy

        protected bool TryGetFirstInCompound<P>(out P processor, bool deep = false)
            where P : class, IProcessor
        {
            processor = null;
            if (m_compound != null && compoundIndex >= 0)
            {
                //TODO : If compoundIndex == 0, need to go upward in compounds
                return m_compound.TryGetFirst(compoundIndex - 1, out processor, deep);

            }
            else
            {
                return false;
            }
        }

        #endregion

        #region IDisposable

        protected void Dispose(bool disposing)
        {
            if (!disposing) { return; }
#if UNITY_EDITOR
            m_disposed = true;
#endif

            //Complete the job first so we can rid of unmanaged resources.
            if (m_scheduled) { m_currentHandle.Complete(); }

            InternalDispose();

            m_procDependency = null;
            m_scheduled = false;
        }

        protected abstract void InternalDispose();

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
