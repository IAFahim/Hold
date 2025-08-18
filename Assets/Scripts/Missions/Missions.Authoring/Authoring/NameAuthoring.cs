using System;
using Missions.Missions.Authoring.BlobComponents;
using Missions.Missions.Authoring.Schemas;
using Unity.Entities;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    public class NameAuthoring : MonoBehaviour
    {
        public NameSchema[] nameSchemas = Array.Empty<NameSchema>();

        public class NameBaker : Baker<NameAuthoring>
        {
            public override void Bake(NameAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new NameBlob
                {
                    BlobAssetRef = NameSchema.ToAssetRef(authoring.nameSchemas)
                });
            }
        }
    }
}