using Times.Time.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Times.Time
{
    [BurstCompile]
    public partial struct AliveTimeJobEntity : IJobEntity
    {
        [ReadOnly] public float DeltaTime;
        [BurstCompile]
        private void Execute(ref AliveTimeComponent aliveTimeComponent)
        {
            aliveTimeComponent.Value += DeltaTime;
        }
    }
}