using Animations.Animation.Data;
using BovineLabs.Core.Iterators;
using BovineLabs.Stats.Data;
using Moves.Move.Data;

namespace States.States.Data
{
    public interface IPlatformerCharacterState
    {
        void OnStateExit(
            in CharacterStateAnimation next,
            ref MoveVector moveVector,
            ref DynamicHashMap<StatKey, StatValue> stats,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic
        );
        
        void OnStateEnter(
            CharacterStateAnimation previous,
            ref MoveVector moveVector,
            ref DynamicHashMap<StatKey, StatValue> stats,
            ref DynamicHashMap<IntrinsicKey, int> intrinsic
        );

        void AnimationStateChange()
        {
            
        }
    }
}