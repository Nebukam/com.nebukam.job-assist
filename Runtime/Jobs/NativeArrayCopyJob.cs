using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Nebukam.JobAssist
{
    [BurstCompile]
    public struct NativeArrayCopyJob<T> : IJobParallelFor
        where T : struct
    {

        [ReadOnly]
        public NativeArray<T> inputArray;
        public NativeArray<T> outputArray;

        public void Execute(int index)
        {
            outputArray[index] = inputArray[index];
        }

    }
}
