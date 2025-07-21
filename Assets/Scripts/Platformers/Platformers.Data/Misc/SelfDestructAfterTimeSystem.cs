using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct SelfDestructAfterTimeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var job = new SelfDestructAfterTimeJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            ECB = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW
                .CreateCommandBuffer(state.WorldUnmanaged)
        };
        job.Schedule();
    }

    [BurstCompile]
    public partial struct SelfDestructAfterTimeJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer ECB;

        private void Execute(Entity entity, ref SelfDestructAfterTime selfDestructAfterTime)
        {
            selfDestructAfterTime.TimeSinceAlive += DeltaTime;
            if (selfDestructAfterTime.TimeSinceAlive > selfDestructAfterTime.LifeTime) ECB.DestroyEntity(entity);
        }
    }
}