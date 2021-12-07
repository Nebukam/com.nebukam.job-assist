using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using static Nebukam.JobAssist.CollectionsUtils;

namespace Nebukam.JobAssist
{

    public interface IProcessorGroup : IProcessorCompound
    {

    }

    /// <summary>
    /// A ProcessorGroup starts its child processors at the same time 
    /// and return a combined handle
    /// </summary>
    public abstract class ProcessorGroup : AbstractProcessorCompound, IProcessorGroup
    {

        protected NativeArray<JobHandle> m_groupHandles = new NativeArray<JobHandle>(0, Allocator.Persistent);

        #region Scheduling

        internal override void OnPrepare()
        {
            MakeLength(ref m_groupHandles, Count);
            base.OnPrepare();
        }

        internal override JobHandle OnScheduled(IProcessor dependsOn = null)
        {

            if (m_isEmptyCompound) { return ScheduleEmpty(dependsOn); }

            int count = Count;

            for (int i = 0; i < count; i++)
                m_groupHandles[i] = m_childs[i].Schedule(m_scaledLockedDelta, dependsOn);

            return JobHandle.CombineDependencies(m_groupHandles);
        }

        internal override JobHandle OnScheduled(JobHandle dependsOn)
        {

            if (m_isEmptyCompound) { return ScheduleEmpty(dependsOn); }

            int count = Count;

            for (int i = 0; i < count; i++)
                m_groupHandles[i] = m_childs[i].Schedule(m_scaledLockedDelta, dependsOn);

            return JobHandle.CombineDependencies(m_groupHandles);
        }

        #endregion

        #region Complete & Apply

        protected override void OnCompleteEnds() { }
        
        #endregion

        #region IDisposable

        protected override void InternalDispose()
        {
            m_groupHandles.Dispose();
        }

        #endregion

        #region Abstracts
        /*
        protected override void InternalLock() { }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }
        */
        #endregion

    }
}
