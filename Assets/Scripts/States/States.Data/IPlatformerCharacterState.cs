using BovineLabs.Core.Iterators;
using BovineLabs.Stats.Data;

namespace States.States.Data
{
    public interface IPlatformerCharacterState
    {
        void OnStateEnter(
            CharacterAnimationState previousState,
            ref DynamicHashMap<StatKey, StatValue> stats,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic
        );

        void OnStateExit(
            CharacterAnimationState nextState,
            ref DynamicHashMap<StatKey, StatValue> stats,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic
        );

        void OnStateVariableUpdate(
            ref DynamicHashMap<StatKey, StatValue> stats,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic
        );
    }
}