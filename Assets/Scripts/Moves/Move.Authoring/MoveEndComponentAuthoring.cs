using Moves.Move.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Moves.Move.Authoring
{
    public class MoveEndComponentAuthoring : MonoBehaviour
    {
        public float3 value;

        public class MoveEndComponentBaker : Baker<MoveEndComponentAuthoring>
        {
            public override void Bake(MoveEndComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new MoveEndComponent { Value = authoring.value });
            }
        }
    }
}