using System;
using Missions.Missions.Authoring.BlobComponents;
using Missions.Missions.Authoring.Schemas;
using Unity.Entities;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    public class MissionAuthoring : MonoBehaviour
    {
        public MissionSchema[] missions = Array.Empty<MissionSchema>();

        public class MissionBaker : Baker<MissionAuthoring>
        {
            public override void Bake(MissionAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new MissionBlob
                {
                    BlobAssetRef = MissionSchema.ToAssetRef(authoring.missions)
                });
            }
        }
    }
}