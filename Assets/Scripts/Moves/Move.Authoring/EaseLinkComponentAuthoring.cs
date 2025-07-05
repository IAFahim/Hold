using Moves.Move.Data.Blobs;
using Unity.Entities;
using UnityEngine;

namespace Moves.Move.Authoring
{
    public class EaseLinkComponentAuthoring : MonoBehaviour
    {
        [Range(byte.MinValue, byte.MaxValue)] public byte start;
        public float elapsedTime;

        class Baker : Baker<EaseLinkComponentAuthoring>
        {
            public override void Bake(EaseLinkComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new EaseLinkComponent
                {
                    Current = authoring.start,
                    ElapsedTime = authoring.elapsedTime
                });
            }
        }
    }
}