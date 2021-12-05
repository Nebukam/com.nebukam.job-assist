using System;
using System.Collections.Generic;
using Unity.Jobs;

namespace Nebukam.JobAssist
{

    public interface IProcessorChain : IProcessorCompound
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class ProcessorChain : AbstractProcessorCompound, IProcessorChain
    {

        #region Scheduling

        internal override JobHandle OnScheduled(IProcessor dependsOn = null)
        {

            int count = m_childs.Count;
            IProcessor proc, prevProc = dependsOn;
            JobHandle handle = default(JobHandle);

            for (int i = 0; i < count; i++)
            {
                proc = m_childs[i];
                proc.compoundIndex = i; // Redundant ?

                handle = prevProc == null 
                    ? proc.Schedule(m_scaledLockedDelta) 
                    : proc.Schedule(m_scaledLockedDelta, prevProc); ;
                prevProc = proc;

            }

            return handle;

        }

        internal override JobHandle OnScheduled(JobHandle dependsOn)
        {
            int count = m_childs.Count;
            IProcessor proc, prevProc = null;
            JobHandle handle = default(JobHandle);

            for (int i = 0; i < count; i++)
            {
                proc = m_childs[i];
                proc.compoundIndex = i; // Redundant ?

                handle = prevProc == null 
                    ? proc.Schedule(m_scaledLockedDelta, m_jobHandleDependency) 
                    : proc.Schedule(m_scaledLockedDelta, prevProc);
                prevProc = proc;

            }

            return handle;
        }

        #endregion

        #region Complete & Apply

        protected override void OnCompleteEnds() { }

        #endregion

        #region Abstracts
        /*
        protected override void InternalLock() { }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }
        */
        #endregion

    }
}
