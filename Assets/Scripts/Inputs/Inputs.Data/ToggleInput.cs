using Unity.Burst;
using Unity.Entities;

namespace Inputs.Inputs.Data
{
    [BurstCompile]
    public struct ToggleInput : IComponentData
    {
        public EToggleInput Input;
    }
}