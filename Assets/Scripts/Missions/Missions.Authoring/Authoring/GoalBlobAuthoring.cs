using Missions.Missions.Authoring.BlobComponents;
using Unity.Entities;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    public class GoalBlobAuthoring : MonoBehaviour
    {
        public class GoalBlobBaker : Baker<GoalBlobAuthoring>
        {
            public override void Bake(GoalBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<GoalBlob>(entity);
            }
        }
    }
}