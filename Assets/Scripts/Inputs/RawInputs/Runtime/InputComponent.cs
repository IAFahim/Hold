#if !BL_DISABLE_INPUT
using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Core.Input
{
    public partial struct InputComponent : IComponentData
    {
        [InputActionDelta] public float2 SwipeDelta;
    }
}
#endif