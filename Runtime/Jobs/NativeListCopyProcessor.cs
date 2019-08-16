using Unity.Collections;

namespace Nebukam.JobAssist
{
    public class NativeListCopyProcessor<T> : Processor<NativeListCopyJob<T>>
        where T : struct
    {

        protected NativeList<T> m_outputList = new NativeList<T>(0, Allocator.Persistent);
        
        public NativeList<T> inputList { get; set; }
        public NativeList<T> outputList { get{ return m_outputList; } set { m_outputList = value; } }

        protected override void InternalLock() { }
        protected override void InternalUnlock() { }

        protected override void Prepare(ref NativeListCopyJob<T> job, float delta)
        {
            job.inputList = inputList;
            job.outputList = outputList;
        }

        protected override void Apply(ref NativeListCopyJob<T> job)
        {
            
        }

    }
}
