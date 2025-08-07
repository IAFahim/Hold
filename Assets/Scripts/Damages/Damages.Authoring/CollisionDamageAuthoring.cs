using BovineLabs.Core.PhysicsStates;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;


public struct CollisionDamage : IComponentData
{
    public float BaseDamage;
    public float HeadOnMultiplier;
    public float MinDotProduct;
}

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateBefore(typeof(KinematicCharacterPhysicsUpdateGroup))]
public partial struct HeadOnCollisionDamageSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (collisionBuffer, entity) in
                 SystemAPI.Query<DynamicBuffer<StatefulCollisionEvent>>()
                     .WithEntityAccess())
        {
            if (!SystemAPI.HasComponent<PhysicsVelocity>(entity) ||
                !SystemAPI.HasComponent<CollisionDamage>(entity))
                continue;

            var movementDir = SystemAPI.GetComponent<PhysicsVelocity>(entity);
            var damageSettings = SystemAPI.GetComponent<CollisionDamage>(entity);

            for (int i = 0; i < collisionBuffer.Length; i++)
            {
                var collision = collisionBuffer[i];

                if (collision.State != StatefulEventState.Enter)
                    continue;


                float damageAmount = CalculateCollisionDamage(
                    movementDir.Linear,
                    collision.Normal,
                    damageSettings,
                    collision);

                if (damageAmount > 0)
                {
                    Debug.Log($"Head-on collision! Damage: {damageAmount}");
                }
            }
        }
    }

    private float CalculateCollisionDamage(
        float3 movementDirection,
        float3 collisionNormal,
        CollisionDamage damageSettings,
        StatefulCollisionEvent collision)
    {
        float3 normalizedMovement = math.normalize(movementDirection);
        float3 normalizedCollisionNormal = math.normalize(collisionNormal);

        float dotProduct = math.dot(normalizedMovement, normalizedCollisionNormal);

        if (dotProduct < damageSettings.MinDotProduct)
        {
            float headOnFactor = math.abs(dotProduct);
            float damage = damageSettings.BaseDamage;

            damage *= (1.0f + (damageSettings.HeadOnMultiplier * headOnFactor));

            if (collision.TryGetDetails(out var details))
            {
                float impulseFactor = math.min(details.EstimatedImpulse * 0.1f, 2.0f);
                damage *= impulseFactor;
            }

            return damage;
        }

        return 0;
    }
}


public class CollisionDamageAuthoring : MonoBehaviour
{
    public float baseDamage = 10f;
    public float headOnMultiplier = 2f;
    [Range(-1f, 0f)] public float minDotProductForHeadOn = -0.8f;
    public float maxHealth = 100f;

    class Baker : Baker<CollisionDamageAuthoring>
    {
        public override void Bake(CollisionDamageAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new CollisionDamage
            {
                BaseDamage = authoring.baseDamage,
                HeadOnMultiplier = authoring.headOnMultiplier,
                MinDotProduct = authoring.minDotProductForHeadOn
            });
        }
    }
}