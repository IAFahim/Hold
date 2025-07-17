using Animations.Animation.Data;
using BovineLabs.Core.Input;
using BovineLabs.Core.Iterators;
using BovineLabs.Essence.Data;
using Unity.Transforms;

namespace States.States.Data
{
    public interface IPlatformerCharacterState
    {
        public void OnStateExit(
            ref LocalTransform localTransform,
            ref CharacterStateComponent characterState,
            in InputComponent inputComponent,
            ref DynamicHashMap<StatKey, StatValue> stats,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic
        );

        public void OnStateEnter(
            ref LocalTransform localTransform,
            ref CharacterStateComponent characterState,
            in InputComponent inputComponent,
            ref DynamicHashMap<StatKey, StatValue> stats,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic
        );

        public void Transition(
            ref LocalTransform localTransform,
            ref CharacterStateComponent characterStateComponent,
            in AnimationStateComponent animation,
            in InputComponent inputComponent,
            ref DynamicHashMap<StatKey, StatValue> stats,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic
        );

        public void AnimationStateChange();
    }
}