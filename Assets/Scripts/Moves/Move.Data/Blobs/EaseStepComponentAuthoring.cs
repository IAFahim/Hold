using Unity.Entities;
using UnityEngine;

namespace Moves.Move.Data.Blobs
{
    public class EaseStepComponentAuthoring : MonoBehaviour
    {
        public byte start;

        class Baker : Baker<EaseStepComponentAuthoring>
        {
            public override void Bake(EaseStepComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new EaseStepPlanComponent { Current = authoring.start });
            }
        }
    }
}