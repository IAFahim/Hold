using Unity.Entities;

namespace Hacks.Hacks.Data
{
    public struct HackReady : IComponentData, IEnableableComponent
    {
        public float ActiveDuration;
    }
}