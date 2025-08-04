using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class PlatformerCharacterAnimationAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlatformerCharacterAnimationAuthoring>
    {
        public override void Bake(PlatformerCharacterAnimationAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlatformerCharacterAnimation());
        }
    }
}