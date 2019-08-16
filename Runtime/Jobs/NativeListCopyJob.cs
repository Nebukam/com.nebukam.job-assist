using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Nebukam.JobAssist
{
    [BurstCompile]
    public struct NativeListCopyJob<T> : IJob
        where T : struct
    {

        [ReadOnly]
        public NativeList<T> inputList;
        [NativeDisableParallelForRestriction]
        public NativeList<T> outputList;

        public void Execute()
        {
            outputList.Clear();
            outputList.Capacity = inputList.Length;
            for(int i = 0, count = inputList.Length; i < count; i++)
                outputList.Add(inputList[i]);
        }

    }
}
