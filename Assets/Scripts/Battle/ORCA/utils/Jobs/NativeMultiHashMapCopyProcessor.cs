using Unity.Collections;

namespace Nebukam.JobAssist
{
    public class NativeMultiHashMapCopyProcessor<TKey, TValue> : Processor<NativeMultiHashMapCopyJob<TKey, TValue>>
        where TKey : struct, System.IEquatable<TKey>
        where TValue : struct
    {

        protected NativeParallelMultiHashMap<TKey, TValue> m_outputMap = new NativeParallelMultiHashMap<TKey, TValue>(0, Allocator.Persistent);

        public NativeParallelMultiHashMap<TKey, TValue> inputMap { get; set; }
        public NativeParallelMultiHashMap<TKey, TValue> outputMap { get { return m_outputMap; } set { m_outputMap = value; } }

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

    }
}
