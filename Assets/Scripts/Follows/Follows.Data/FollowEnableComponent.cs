using Unity.Entities;

namespace Follows.Follows.Data
{
    public struct FollowEnableComponent : IComponentData, IEnableableComponent
    {
        public float StoppingDistanceSq;
    }
}