using Moves.Move.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Moves.Move.Authoring
{
    public class GroundMoveDirectionComponentAuthoring : MonoBehaviour
    {
        public float2 value;

        public class GroundMoveDirectionComponentBaker : Baker<GroundMoveDirectionComponentAuthoring>
        {
            public override void Bake(GroundMoveDirectionComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GroundMoveDirectionComponent { Value = authoring.value });
            }
        }
    }
}