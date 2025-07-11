using Inputs.Inputs.Data;
using Unity.Entities;
using UnityEngine;

namespace Inputs.Inputs.Authoring
{
    public class CharacterInputComponentAuthoring : MonoBehaviour
    {
        public ECharacterInput CharacterInputComponent;

        public class CharacterInputComponentBaker : Baker<CharacterInputComponentAuthoring>
        {
            public override void Bake(CharacterInputComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CharacterInputComponent { Value = authoring.CharacterInputComponent });
            }
        }
    }
}