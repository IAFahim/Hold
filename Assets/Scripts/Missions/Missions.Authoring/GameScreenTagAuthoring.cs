using Missions.Missions.Data;
using Unity.Entities;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    public class GameScreenTagAuthoring : MonoBehaviour
    {
        public class GameScreenTagBaker : Baker<GameScreenTagAuthoring>
        {
            public override void Bake(GameScreenTagAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<GameScreenTag>(entity);
            }
        }
    }
}