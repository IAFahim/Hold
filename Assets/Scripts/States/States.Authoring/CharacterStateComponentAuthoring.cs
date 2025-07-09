using Animations.Animation.Data.enums;
using States.States.Data;
using States.States.Data.enums;
using Unity.Entities;
using UnityEngine;

namespace States.States.Authoring
{
    public class CharacterStateComponentAuthoring : MonoBehaviour
    {
        public ECharacterState previous;
        public ECharacterState current;

        public class CharacterStateComponentBaker : Baker<CharacterStateComponentAuthoring>
        {
            public override void Bake(CharacterStateComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CharacterStateComponent
                    {
                        Previous = authoring.previous,
                        Current = authoring.current,
                    }
                );
            }
        }
    }
}