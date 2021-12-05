using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace Nebukam.JobAssist
{

    public interface IProcessorCompound : IProcessor
    {

        int Count { get; }
        IProcessor this[int i] { get; }
        void DisposeAll();
        bool TryGetFirst<P>(int startIndex, out P processor, bool deep = false) where P : class, IProcessor;

    }

    public abstract class AbstractProcessorCompound : AbstractProcessor, IProcessorCompound
    {

        
        protected List<IProcessor> m_childs = new List<IProcessor>();
        public int Count { get { return m_childs.Count; } }

        public IProcessor this[int i] { get { return m_childs[i]; } }

        #region Child management

        public IProcessor Add(IProcessor proc)
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot add new processors to a locked chain");
            }
#endif

            if (m_childs.Contains(proc)) { return proc; }
            m_childs.Add(proc);
            return OnChildAdded(proc, Count-1);
        }

        public P Add<P>(P proc)
            where P : class, IProcessor
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot add new processors to a locked chain");
            }
#endif

            if (m_childs.Contains(proc)) { return proc; }
            m_childs.Add(proc);
            return OnChildAdded(proc, Count-1) as P;
        }

        /// <summary>
        /// Create (if null) and add item
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="proc"></param>
        /// <returns></returns>
        public P Add<P>(ref P proc)
            where P : class, IProcessor, new()
        {
#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot add new processors to a locked chain");
            }
#endif
            if (proc != null) { return Add(proc); }
            proc = new P();
            return Add(proc);
        }

        public P Insert<P>(int atIndex, P proc)
            where P : class, IProcessor
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot insert new processors to a locked chain");
            }
#endif
            if (m_childs.Contains(proc)) { return proc; } //TODO: Handle situation gracefully, re-order ?
            if (atIndex > m_childs.Count - 1) { return Add(proc); }

            m_childs.Insert(atIndex, proc);
            return OnChildAdded(proc, atIndex) as P;
        }

        public P InsertBefore<P>(IProcessor beforeItem, P proc)
            where P : class, IProcessor
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot insert new processors to a locked chain");
            }
#endif
            int atIndex = m_childs.IndexOf(beforeItem);
            if (atIndex == -1) { return Add(proc); }
            return Insert(atIndex, proc);
        }

        public P InsertBefore<P>(IProcessor beforeProc, ref P proc)
            where P : class, IProcessor, new()
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot insert new processors to a locked chain");
            }
#endif
            if (proc != null) { return InsertBefore(beforeProc, proc); }
            proc = new P();
            return InsertBefore(beforeProc, proc);
        }

        /// <summary>
        /// Removes a processor from the chain
        /// </summary>
        /// <param name="proc"></param>
        public IProcessor Remove(IProcessor proc)
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot remove processors from a locked chain");
            }
#endif

            int index = m_childs.IndexOf(proc);
            if (index == -1) { return null; }

            m_childs.RemoveAt(index);            
            return OnChildRemoved(proc, index);

        }

        protected IProcessor OnChildAdded(IProcessor child, int childIndex)
        {
            child.compound = this;
            RefreshChildIndices(childIndex);
            return child;
        }

        protected IProcessor OnChildRemoved(IProcessor child, int childIndex)
        {
            child.compound = null;
            RefreshChildIndices(childIndex);
            return child;
        }

        protected void RefreshChildIndices(int from)
        {
            int count = Count;
            if (from <= count - 1)
            {
                for (int i = from; i < count; i++)
                    m_childs[i].compoundIndex = i;
            }
        }

        #endregion

        #region Scheduling

        internal override void OnPrepare()
        {
            Prepare(m_scaledLockedDelta);
        }

        /// <summary>
        /// In a ProcessorGroup, Prepare is called right before scheduling the existing group for the job.
        /// If you intend to dynamically modify the group childs list, do so in InternalLock(), right before base.InternalLock().
        /// </summary>
        /// <param name="delta"></param>
        protected abstract void Prepare(float delta);

        #endregion

        #region Complete & Apply

        protected override void OnCompleteBegins()
        {
            
            int count = m_childs.Count;
            for (int i = 0; i < count; i++)
            {
                m_childs[i].Complete();
            }

            Apply();

        }

        protected abstract void Apply();

        #endregion

        #region ILockable

        public override void Lock()
        {

            if (m_locked) { return; }
            base.Lock();

            for (int i = 0, count = m_childs.Count; i < count; i++)
                m_childs[i].Lock();

        }

        public override void Unlock()
        {
            if (!m_locked) { return; }
            base.Unlock();

            for (int i = 0, count = m_childs.Count; i < count; i++)
                m_childs[i].Unlock();
        }

        #endregion

        #region Hierarchy

        public bool TryGetFirst<P>(int startIndex, out P processor, bool deep = false)
            where P : class, IProcessor
        {

            processor = null;

            if (startIndex < 0) { startIndex = m_childs.Count - 1; }

            IProcessor child;
            IProcessorCompound childCompound;

            for (int i = startIndex; i >= 0; i--)
            {
                child = m_childs[i];
                processor = child as P;

                if (processor != null)
                {
                    return true;
                }

                if (!deep) { continue; }

                childCompound = child as IProcessorCompound;

                if (childCompound != null
                    && childCompound.TryGetFirst(-1, out processor, deep))
                        return true;

            }

            return TryGetFirstInCompound(out processor, deep);

        }

        #endregion

        #region IDisposable

        public void DisposeAll()
        {

#if UNITY_EDITOR
            if (m_disposed)
            {
                throw new Exception("DisposeAll() called on already disposed Compound.");
            }
#endif

            if (m_scheduled) { m_currentHandle.Complete(); }

            IProcessor p;

            for (int i = 0, count = m_childs.Count; i < count; i++)
            {
                p = m_childs[i];

                if (p is IProcessorCompound)
                    (p as IProcessorCompound).DisposeAll();
                else
                    p.Dispose();

            }

            m_scheduled = false; // Avoid Completting current handle twice

            Dispose();

        }

        #endregion

    }
}
