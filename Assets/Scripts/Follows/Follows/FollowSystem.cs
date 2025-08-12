using Focuses.Focuses.Data;
using Follows.Follows.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Follows.Follows
{
    public partial struct FollowSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var mainEntity = SystemAPI.GetSingleton<FocusSingletonComponent>().Entity;
            if (mainEntity == Entity.Null) return;
            var position = SystemAPI.GetComponent<LocalTransform>(mainEntity).Position;
            new FollowSystemJobEntity()
            {
                MainEntity = mainEntity,
                PositionMainEntity = position,
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }

    [BurstCompile]
    [WithPresent(typeof(FollowEnableComponent))]
    public partial struct FollowSystemJobEntity : IJobEntity
    {
        public Entity MainEntity;
        public float3 PositionMainEntity;

        [BurstCompile]
        private void Execute(
            Entity entity,
            EnabledRefRW<FollowEnableComponent> follow,
            ref FollowEnableComponent followEnableComponent,
            in LocalTransform localTransform,
            ref PlatformerCharacterControl characterControl
        )
        {
            if (entity == MainEntity) return;
            if (!follow.ValueRO) return;
            var diff = PositionMainEntity - localTransform.Position;
            var normalize = math.normalize(diff);
            var lengthSq = math.lengthsq(diff);
            var far = lengthSq > followEnableComponent.StoppingDistanceSq;
            followEnableComponent.Reached = !far;
            characterControl.MoveVector = far ? normalize : float3.zero;
        }
    }
}