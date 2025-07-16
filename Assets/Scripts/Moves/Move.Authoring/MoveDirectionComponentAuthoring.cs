using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Moves.Move.Data
{
    public class MoveDirectionComponentAuthoring : MonoBehaviour
    {
        public float2 groundDirection;

        public class MoveDirectionComponentBaker : Baker<MoveDirectionComponentAuthoring>
        {
            public override void Bake(MoveDirectionComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new MoveDirectionComponent { GroundDirection = authoring.groundDirection });
            }
        }
    }
}