using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Nebukam.JobAssist
{
    [BurstCompile]
    public struct NativeMultiHashMapCopyJob<TKey, TValue> : IJob
        where TKey : struct, System.IEquatable<TKey>
        where TValue : struct
    {

        [ReadOnly]
        public NativeMultiHashMap<TKey, TValue> inputHashMap;
        public NativeMultiHashMap<TKey, TValue> outputHashMap;

        public void Execute()
        {
            outputHashMap.Clear();
            outputHashMap.Capacity = inputHashMap.Count();

            NativeMultiHashMapIterator<TKey> it;
            NativeArray<TKey> keys = inputHashMap.GetKeyArray(Allocator.Temp);
            TKey key;
            TValue value;
            for (int k = 0, count = keys.Length; k < count; k++)
            {
                key = keys[k];
                if (inputHashMap.TryGetFirstValue(key, out value, out it))
                {
                    outputHashMap.Add(key, value);
                    while (inputHashMap.TryGetNextValue(out value, ref it))
                    {
                        outputHashMap.Add(key, value);
                    }
                }
            }

            keys.Dispose();

        }

    }
}
