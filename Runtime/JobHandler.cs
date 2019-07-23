﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

namespace Nebukam.JobAssist
{

    public interface IJobHandler
    {

        IJobHandler jobDependency { get; }
        JobHandle currentHandle { get; }

        JobHandle Schedule(float delta, IJobHandler dependsOn = null);
        JobHandle Schedule(float delta, JobHandle dependsOn);
        void Complete();
        
    }


    public abstract class JobHandler<T> : IJobHandler
        where T : struct, IJob
    {

        protected bool m_hasJobHandleDependency = false;
        protected JobHandle m_jobHandleDependency = default(JobHandle);

        protected IJobHandler m_jobDependency = null;
        public IJobHandler jobDependency { get { return m_jobDependency; } }

        protected T m_currentJob;
        protected JobHandle m_currentHandle;
        public JobHandle currentHandle { get { return m_currentHandle; } }

        protected bool m_scheduled = false;
        protected bool m_completed = false;
        public bool completed {
            get {
                if (m_scheduled)
                {
                    if (m_currentHandle.IsCompleted)
                    {
                        Complete();
                        return m_completed;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return m_completed;
                }
            }
        }

        /// <summary>
        /// Schedule this job, with an optional dependency.
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public JobHandle Schedule(float delta, IJobHandler dependsOn = null)
        {
            if (m_scheduled) { return m_currentHandle; }

            m_scheduled = true;
            m_completed = false;
            m_hasJobHandleDependency = false;

            m_currentJob = new T();
            Prepare(ref m_currentJob, delta);

            if (dependsOn != null)
            {
                m_jobDependency = dependsOn;
                m_currentHandle = m_currentJob.Schedule(m_jobDependency.currentHandle);
            }
            else
            {
                m_jobDependency = null;
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
            if (m_scheduled) { return m_currentHandle; }

            m_scheduled = true;
            m_completed = false;
            m_hasJobHandleDependency = true;
            m_jobDependency = null;

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
            if (!m_scheduled) { return; }

            if (m_hasJobHandleDependency)
                m_jobHandleDependency.Complete();

            m_jobDependency?.Complete();
            m_currentHandle.Complete();

            m_scheduled = false;
            m_completed = true;

            Apply(ref m_currentJob);

        }

        protected abstract void Apply(ref T job);


    }

}
