using Moves.Move.Data.Blobs;
using Unity.Entities;
using UnityEngine;

namespace Moves.Move.Authoring
{
    public class EaseStateComponentAuthoring : MonoBehaviour
    {
        [Range(byte.MinValue, byte.MaxValue)] public byte start;
        public float elapsedTime;

        class Baker : Baker<EaseStateComponentAuthoring>
        {
            public override void Bake(EaseStateComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new EaseStateComponent
                {
                    Current = authoring.start,
                    ElapsedTime = authoring.elapsedTime
                });
            }
        }
    }
}