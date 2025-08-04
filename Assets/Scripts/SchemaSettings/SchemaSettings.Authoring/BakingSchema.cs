using Unity.Collections;
using Unity.Entities;

namespace SchemaSettings.SchemaSettings.Authoring
{
    public abstract class BakingSchema<T> : BaseSchema<T> where T : struct
    {
        public static BlobAssetReference<BlobArray<T>> CreateBlobAssetRef(BakingSchema<T>[] datas)
        {
            using var builder = new BlobBuilder(Allocator.Temp);
            ref var blobArray = ref builder.ConstructRoot<BlobArray<T>>();
            var arrayBuilder = builder.Allocate(ref blobArray, datas.Length);
            for (int i = 0; i < datas.Length; i++) arrayBuilder[i] = datas[i].ToData();
            var blobAssetRef = builder.CreateBlobAssetReference<BlobArray<T>>(Allocator.Persistent);
            return blobAssetRef;
        }
    }
}