using Times.Time.Data;
using Unity.Entities;
using UnityEngine;

namespace Times.Time.Authoring
{
    public class AliveTimeComponentAuthoring : MonoBehaviour
    {
        public float value;

        public class AliveTimeComponentBaker : Baker<AliveTimeComponentAuthoring>
        {
            public override void Bake(AliveTimeComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new AliveTimeComponent { Value = authoring.value });
            }
        }
    }
}