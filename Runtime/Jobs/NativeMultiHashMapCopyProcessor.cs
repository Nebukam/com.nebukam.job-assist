using Unity.Collections;

namespace Nebukam.JobAssist
{
    public class NativeMultiHashMapCopyProcessor<TKey, TValue> : Processor<NativeMultiHashMapCopyJob<TKey, TValue>>
        where TKey : struct, System.IEquatable<TKey>
        where TValue : struct
    {

        protected NativeMultiHashMap<TKey, TValue> m_outputMap = new NativeMultiHashMap<TKey, TValue>(0, Allocator.Persistent);

        public NativeMultiHashMap<TKey, TValue> inputMap { get; set; }
        public NativeMultiHashMap<TKey, TValue> outputMap { get { return m_outputMap; } set { m_outputMap = value; } }

        protected override void InternalLock() { }
        protected override void InternalUnlock() { }

        protected override void Prepare(ref NativeMultiHashMapCopyJob<TKey, TValue> job, float delta)
        {
            job.inputHashMap = inputMap;
            job.outputHashMap = m_outputMap;
        }

        protected override void Apply(ref NativeMultiHashMapCopyJob<TKey, TValue> job)
        {

        }

        protected override void InternalDispose()
        {
            m_outputMap.Dispose();
        }

    }
}
