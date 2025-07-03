using Moves.Move.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Moves.Move.Authoring
{
    public class MoveStartComponentAuthoring : MonoBehaviour
    {
        public float3 value;

        public class MoveStartComponentBaker : Baker<MoveStartComponentAuthoring>
        {
            public override void Bake(MoveStartComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new MoveStartComponent { Value = authoring.value });
            }
        }
    }
}