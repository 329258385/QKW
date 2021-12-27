using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Nebukam.JobAssist
{
    [BurstCompile]
    public struct NativeArrayCopyJob<T> : IJob
        where T : struct
    {

        [ReadOnly]
        public NativeArray<T> inputArray;
        public NativeArray<T> outputArray;

        public void Execute()
        {
            for(int i = 0;i < inputArray.Length;i ++)
            {
                outputArray[i] = inputArray[i];
            }
           
        }

    }
}
