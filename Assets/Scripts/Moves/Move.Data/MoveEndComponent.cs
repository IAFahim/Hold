using Unity.Entities;
using Unity.Mathematics;

namespace Moves.Move.Data
{
    public struct MoveEndComponent : IComponentData
    {
        public float3 Value;
    }
}