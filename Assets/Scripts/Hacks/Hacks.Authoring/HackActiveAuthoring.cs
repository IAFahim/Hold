using Hacks.Hacks.Data;
using Unity.Entities;
using UnityEngine;

namespace Hacks.Hacks.Authoring
{
    public class HackActiveAuthoring : MonoBehaviour
    {
        public bool Enable;

        public class HackActiveBaker : Baker<HackActiveAuthoring>
        {
            public override void Bake(HackActiveAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new HackActive { Enable = authoring.Enable });
            }
        }
    }
}