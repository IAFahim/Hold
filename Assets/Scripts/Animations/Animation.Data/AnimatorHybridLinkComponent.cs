using Unity.Entities;
using UnityEngine;

namespace Animations.Animation.Data
{
    public struct AnimatorHybridLinkComponent : ICleanupComponentData
    {
        public UnityObjectRef<Animator> Ref;
    }
}