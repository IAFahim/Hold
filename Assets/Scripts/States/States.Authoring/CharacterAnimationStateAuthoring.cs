using States.States.Data;
using States.States.Data.enums;
using Unity.Entities;
using UnityEngine;

namespace States.States.Authoring
{
    public class CharacterAnimationStateAuthoring : MonoBehaviour
    {
        public ECharacterState previous;
        public ECharacterState current;

        public class CharacterAnimationStateBaker : Baker<CharacterAnimationStateAuthoring>
        {
            public override void Bake(CharacterAnimationStateAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CharacterStateAnimation
                    {
                        Previous = authoring.previous, Current = authoring.current
                    }
                );

            }
        }
    }
}