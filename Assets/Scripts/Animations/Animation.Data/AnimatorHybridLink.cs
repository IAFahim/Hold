using Unity.Entities;
using UnityEngine;

namespace Animations.Animation.Data
{
    public struct AnimatorHybridLink : ICleanupComponentData
    {
        public UnityObjectRef<Animator> Ref;
    }
}