using Moves.Move.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Moves.Move.Authoring
{
    public class MoveVectorAuthoring : MonoBehaviour
    {
        public float2 value;

        public class MoveVectorBaker : Baker<MoveVectorAuthoring>
        {
            public override void Bake(MoveVectorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new MoveVector
                {
                    Value = authoring.value
                });
            }
        }
    }
}