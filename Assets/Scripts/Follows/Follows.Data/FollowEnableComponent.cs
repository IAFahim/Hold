using Unity.Entities;

namespace Follows.Follows.Data
{
    public struct FollowEnableComponent : IComponentData, IEnableableComponent
    {
        public bool Reached;
        public float StoppingDistanceSq;
    }
}