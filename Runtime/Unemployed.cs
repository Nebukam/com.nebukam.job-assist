using Unity.Burst;
using Unity.Jobs;

namespace Nebukam.JobAssist
{
    [BurstCompile]
    public struct Unemployed : IJob { public void Execute() { } }
}
