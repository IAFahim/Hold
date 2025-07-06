using Animations.Animation.Data;
using Unity.Entities;
using UnityEngine;

namespace Animations.Animation.Authoring
{
    public class AnimatorAssetIndexDisposeComponentAuthoring : MonoBehaviour
    {
        public byte index;

        public class AnimatorAssetIndexDisposeComponentBaker : Baker<AnimatorAssetIndexDisposeComponentAuthoring>
        {
            public override void Bake(AnimatorAssetIndexDisposeComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new AnimatorAssetIndexDisposeComponent { Index = authoring.index });
            }
        }
    }
}