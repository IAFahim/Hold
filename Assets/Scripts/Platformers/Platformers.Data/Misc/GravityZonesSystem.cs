using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.CharacterController;


using StatefulEventState = BovineLabs.Core.PhysicsStates.StatefulEventState;
using StatefulTriggerEvent = BovineLabs.Core.PhysicsStates.StatefulTriggerEvent;

//
// [UpdateInGroup(
//     typeof(SimulationSystemGroup))] // update in variable update because the camera can use gravity to adjust its up direction
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateBefore(typeof(PlatformerCharacterVariableUpdateSystem))]
public partial class GravityZonesSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Update transforms so we have the proper interpolated position of our entities to calculate spherical gravities from
        // (without this, we'd see jitter on the planet)
        World.GetOrCreateSystem<TransformSystemGroup>().Update(World.Unmanaged);

        var resetGravitiesJob = new ResetGravitiesJob();
        resetGravitiesJob.Schedule();

        var sphericalGravityJob = new SphericalGravityJob
        {
            CustomGravityFromEntity = SystemAPI.GetComponentLookup<CustomGravity>(false),
            LocalToWorldFromEntity = SystemAPI.GetComponentLookup<LocalToWorld>(true)
        };
        sphericalGravityJob.Schedule();

        if (SystemAPI.TryGetSingleton(out GlobalGravityZone globalGravityZone))
        {
            var globalGravityJob = new GlobalGravityJob
            {
                GlobalGravityZone = globalGravityZone
            };
            globalGravityJob.Schedule();
        }

        var applyGravityJob = new ApplyGravityJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        applyGravityJob.Schedule();
    }

    [BurstCompile]
    public partial struct ResetGravitiesJob : IJobEntity
    {
        private void Execute(ref CustomGravity customGravity)
        {
            customGravity.LastZoneEntity = customGravity.CurrentZoneEntity;
            customGravity.TouchedByNonGlobalGravity = false;
        }
    }

    [BurstCompile]
    public unsafe partial struct SphericalGravityJob : IJobEntity
    {
        public ComponentLookup<CustomGravity> CustomGravityFromEntity;
        [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldFromEntity;

        private void Execute(Entity entity, in SphericalGravityZone sphericalGravityZone,
            in PhysicsCollider physicsCollider, in DynamicBuffer<StatefulTriggerEvent> triggerEventsBuffer)
        {
            if (triggerEventsBuffer.Length > 0)
            {
                var sphereCollider = (SphereCollider*)physicsCollider.ColliderPtr;
                var sphereGeometry = sphereCollider->Geometry;

                for (var i = 0; i < triggerEventsBuffer.Length; i++)
                {
                    var triggerEvent = triggerEventsBuffer[i];
                    if (triggerEvent.State == StatefulEventState.Stay)
                    {
                        var otherEntity = triggerEvent.EntityB;

                        var fromOtherToSelfVector = LocalToWorldFromEntity[entity].Position -
                                                    LocalToWorldFromEntity[otherEntity].Position;
                        var distanceRatio = math.clamp(math.length(fromOtherToSelfVector) / sphereGeometry.Radius,
                            0.01f, 0.99f);
                        var gravityToApply = (1f - distanceRatio) * (math.normalizesafe(fromOtherToSelfVector) *
                                                                     sphericalGravityZone.GravityStrengthAtCenter);

                        if (CustomGravityFromEntity.HasComponent(otherEntity))
                        {
                            var customGravity = CustomGravityFromEntity[otherEntity];
                            customGravity.Gravity = gravityToApply * customGravity.GravityMultiplier;
                            customGravity.TouchedByNonGlobalGravity = true;
                            customGravity.CurrentZoneEntity = entity;
                            CustomGravityFromEntity[otherEntity] = customGravity;
                        }
                    }
                }
            }
        }
    }

    [BurstCompile]
    public partial struct GlobalGravityJob : IJobEntity
    {
        public GlobalGravityZone GlobalGravityZone;

        private void Execute( ref CustomGravity customGravity)
        {
            if (!customGravity.TouchedByNonGlobalGravity)
            {
                customGravity.Gravity = GlobalGravityZone.Gravity * customGravity.GravityMultiplier;
                customGravity.CurrentZoneEntity = Entity.Null;
            }
        }
    }

    [BurstCompile]
    public partial struct ApplyGravityJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref PhysicsVelocity physicsVelocity, in PhysicsMass physicsMass,
            in CustomGravity customGravity)
        {
            if (physicsMass.InverseMass > 0f)
                CharacterControlUtilities.AccelerateVelocity(ref physicsVelocity.Linear, customGravity.Gravity,
                    DeltaTime);
        }
    }
}