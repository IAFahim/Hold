using Animations.Animation.Data;
using Animations.Animation.Data.enums;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Animations.Animation.Authoring
{
    public class AnimationStateAuthoring : MonoBehaviour
    {
        public EAnimationState animation;
        public half speed = new(1);

        public class CharacterAnimationStateBaker : Baker<AnimationStateAuthoring>
        {
            public override void Bake(AnimationStateAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new AnimationStateComponent
                {
                    Animation = authoring.animation,
                    Speed = authoring.speed
                });
            }
        }
    }
}