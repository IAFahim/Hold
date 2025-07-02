using Unity.Entities;
using Unity.Mathematics;

namespace Move.Move.Data
{
    public struct MoveStartComponent : IComponentData, IEnableableComponent
    {
        public float3 Value;
    }
}
