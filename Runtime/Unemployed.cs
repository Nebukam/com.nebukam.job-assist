using Unity.Burst;
using Unity.Jobs;

namespace Nebukam.JobAssist
{
    [BurstCompile]
    public struct Unemployed : IJob { public void Execute() { } }
    public struct UnemployedParallel : IJobParallelFor { public void Execute(int index) { } }
}
