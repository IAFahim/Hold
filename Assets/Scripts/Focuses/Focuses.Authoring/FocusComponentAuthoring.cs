using Focuses.Focuses.Data;
using Unity.Entities;
using UnityEngine;

namespace Focuses.Focuses.Authoring
{
    public class FocusComponentAuthoring : MonoBehaviour
    {
        public sbyte priority = 0;

        public class FocusComponentBaker : Baker<FocusComponentAuthoring>
        {
            public override void Bake(FocusComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new FocusPriorityComponent { Priority = authoring.priority });
            }
        }
    }
}