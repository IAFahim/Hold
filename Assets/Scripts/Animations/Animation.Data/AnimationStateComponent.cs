using Animations.Animation.Data.enums;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Animations.Animation.Data
{
    [BurstCompile]
    public struct AnimationStateComponent : IComponentData
    {
        public EAnimationState Animation;
        public half Speed;
    }
}