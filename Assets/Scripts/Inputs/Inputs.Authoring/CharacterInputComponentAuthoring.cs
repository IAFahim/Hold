using Inputs.Inputs.Data;
using Unity.Entities;
using UnityEngine;

namespace Inputs.Inputs.Authoring
{
    public class CharacterInputComponentAuthoring : MonoBehaviour
    {
        [SerializeField] private byte input = CharacterInputComponent.MiddleLane;

        public class CharacterInputComponentBaker : Baker<CharacterInputComponentAuthoring>
        {
            public override void Bake(CharacterInputComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CharacterInputComponent
                {
                    Value = authoring.input
                });
            }
        }
    }
} 