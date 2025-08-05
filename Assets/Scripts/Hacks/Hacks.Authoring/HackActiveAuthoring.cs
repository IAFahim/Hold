using Hacks.Hacks.Data;
using Unity.Entities;
using UnityEngine;

namespace Hacks.Hacks.Authoring
{
    public class HackActiveAuthoring : MonoBehaviour
    {
        public bool enable;

        public class HackActiveBaker : Baker<HackActiveAuthoring>
        {
            public override void Bake(HackActiveAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new HackActive { Enable = authoring.enable });
            }
        }
    }
}