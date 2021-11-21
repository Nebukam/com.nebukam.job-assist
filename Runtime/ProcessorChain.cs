using System;
using System.Collections.Generic;
using Unity.Jobs;

namespace Nebukam.JobAssist
{

    public interface IProcessorChain : IProcessor, IProcessorGroup
    {

    }

    public class ProcessorChain : IProcessor, IProcessorChain
    {

        public float deltaMultiplier { get; set; } = 1.0f;

        protected bool m_locked = false;
        public bool locked { get { return m_locked; } }

        protected IProcessorGroup m_group = null;
        public IProcessorGroup group { get { return m_group; } set { m_group = value; } }

        public int groupIndex { get; set; } = -1;

        protected bool m_hasJobHandleDependency = false;
        protected JobHandle m_jobHandleDependency = default(JobHandle);

        protected IProcessor m_procDependency = null;
        public IProcessor procDependency { get { return m_procDependency; } }

        //Last dependency of the stack
        protected JobHandle m_currentHandle;
        public JobHandle currentHandle { get { return m_currentHandle; } }

        protected float m_deltaSum = 0f;

        #region items

        protected List<IProcessor> m_childs = new List<IProcessor>();
        public int Count { get { return m_childs.Count; } }

        public IProcessor this[int i] { get { return m_childs[i]; } }

        public bool TryGetFirst<P>(int startIndex, out P processor, bool deep = false)
            where P : class, IProcessor
        {

            processor = null;

            if (startIndex == -1) { startIndex = m_childs.Count - 1; }

            IProcessor child;
            IProcessorGroup subGroup;
            for (int i = startIndex; i >= 0; i--)
            {
                child = m_childs[i];
                processor = child as P;
                if (processor != null)
                {
                    return true;
                }

                if (!deep) { continue; }

                subGroup = child as IProcessorGroup;
                if (subGroup != null)
                {
                    if (subGroup.TryGetFirst(-1, out processor, deep))
                        return true;
                }

            }

            if (m_group != null && groupIndex > 0)
            {
                return m_group.TryGetFirst(groupIndex - 1, out processor, deep);
            }

            return false;
        }

        public IProcessor Add(IProcessor proc)
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot add new processors to a locked chain");
            }
#endif

            if (m_childs.Contains(proc)) { return proc; }
            proc.groupIndex = m_childs.Count;
            proc.group = this;
            m_childs.Add(proc);
            return proc;
        }

        public P Add<P>(P proc)
            where P : IProcessor
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot add new processors to a locked chain");
            }
#endif

            if (m_childs.Contains(proc)) { return proc; }
            proc.groupIndex = m_childs.Count;
            proc.group = this;
            m_childs.Add(proc);
            return proc;
        }

        /// <summary>
        /// Create (if null) and add item
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public P Add<P>(ref P item)
            where P : IProcessor, new()
        {
            if (item != null) { return Add(item); }
            item = new P();
            return Add(item);
        }

        #endregion

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
        public virtual JobHandle Schedule(float delta, IProcessor dependsOn = null)
        {
            //TODO : Allow the delta NOT to be summed with previous values
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

            m_procDependency = dependsOn;

            Lock();

            m_currentHandle = ScheduleJobList(m_deltaSum * deltaMultiplier, dependsOn);
            return m_currentHandle;

        }

        protected virtual JobHandle ScheduleJobList(float delta, IProcessor dependsOn)
        {

            int count = m_childs.Count;
            IProcessor proc, prevProc = dependsOn;
            JobHandle handle = default(JobHandle);

            for (int i = 0; i < count; i++)
            {
                proc = m_childs[i];
                proc.groupIndex = i;

                handle = prevProc == null ? proc.Schedule(delta) : proc.Schedule(delta, prevProc); ;
                prevProc = proc;

            }

            return handle;
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
        public virtual JobHandle Schedule(float delta, JobHandle dependsOn)
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
            m_jobHandleDependency = dependsOn;

            Lock();

            m_currentHandle = ScheduleJobList(m_deltaSum * deltaMultiplier);
            return m_currentHandle;

        }

        protected virtual JobHandle ScheduleJobList(float delta)
        {
            int count = m_childs.Count;
            IProcessor proc, prevProc = null;
            JobHandle handle = default(JobHandle);

            for (int i = 0; i < count; i++)
            {
                proc = m_childs[i];
                proc.groupIndex = i;

                handle = prevProc == null ? proc.Schedule(delta, m_jobHandleDependency) : proc.Schedule(delta, prevProc); ;
                prevProc = proc;

            }

            return handle;
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

            int count = m_childs.Count;
            for (int i = 0; i < count; i++)
            {
                m_childs[i].Complete();
            }

            m_scheduled = false;

            Apply();
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

        protected virtual void Apply() { }

        #region ILockable

        public void Lock()
        {
            if (m_locked) { return; }
            m_locked = true;
            InternalLock();
        }

        protected virtual void InternalLock()
        {
            for (int i = 0, count = m_childs.Count; i < count; i++)
                m_childs[i].Lock();
        }

        public void Unlock()
        {
            if (!m_locked) { return; }
            m_locked = false;
            //Complete the job for safety
            if (m_scheduled) { Complete(); }
            InternalUnlock();

        }

        protected virtual void InternalUnlock()
        {
            for (int i = 0, count = m_childs.Count; i < count; i++)
                m_childs[i].Unlock();
        }

        #endregion

        #region IDisposable

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

        /// <summary>
        /// Dispose along with chain nodes
        /// </summary>
        public void DisposeAll()
        {
            if (m_scheduled) { m_currentHandle.Complete(); }
            IProcessor p;
            for (int i = 0, count = m_childs.Count; i < count; i++)
            {
                p = m_childs[i];
                if (p is IProcessorGroup)
                {
                    (p as IProcessorGroup).DisposeAll();
                }
                else
                {
                    p.Dispose();
                }

            }
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);

        }

        #endregion

    }
}
