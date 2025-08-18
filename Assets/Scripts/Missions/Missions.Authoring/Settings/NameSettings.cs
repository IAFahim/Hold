using BovineLabs.Core.Authoring.Settings;
using Missions.Missions.Authoring.BlobComponents;
using Missions.Missions.Authoring.Schemas;
using Unity.Entities;

namespace Missions.Missions.Authoring.Settings
{
    public class NameSettings : SettingsSchema<NameSchema>
    {
        public override void Bake(Baker<SettingsAuthoring> baker)
        {
            var blobAssetReference = NameSchema.ToAssetRef(schemas);
            var entity = baker.GetEntity(TransformUsageFlags.None);
            baker.AddComponent<NameBlob>(entity, new NameBlob()
            {
                BlobAssetRef = blobAssetReference
            });
        }
    }
}