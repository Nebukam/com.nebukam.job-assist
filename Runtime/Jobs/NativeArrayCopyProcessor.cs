using Unity.Collections;
using static Nebukam.JobAssist.CollectionsUtils;

namespace Nebukam.JobAssist
{
    public class NativeArrayCopyProcessor<T> : ParallelProcessor<NativeArrayCopyJob<T>>
        where T : struct
    {
        protected NativeArray<T> m_outputArray = new NativeArray<T>(0, Allocator.Persistent);

        public NativeArray<T> inputArray { get; set; }
        public NativeArray<T> outputArray { get { return m_outputArray; } }

        protected override void InternalLock() { }
        protected override void InternalUnlock() { }

        protected override int Prepare(ref NativeArrayCopyJob<T> job, float delta)
        {

            int length = inputArray.Length;

            EnsureLength(ref m_outputArray, length);

            job.inputArray = inputArray;
            job.outputArray = outputArray;

            return length;

        }

        protected override void Apply(ref NativeArrayCopyJob<T> job)
        {

        }
    }
}
