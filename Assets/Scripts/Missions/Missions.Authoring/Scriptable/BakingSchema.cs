using System;
using Unity.Collections;
using Unity.Entities;

namespace Missions.Missions.Authoring.Scriptable
{
    public abstract class BakingSchema<T> : BaseSchema where T : struct
    {
        public abstract T ToData();

        public static BlobAssetReference<BlobArray<T>> ToAssetRef(BakingSchema<T>[] datas)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            var blobAssetRef = ToAssetRef(ref builder, datas);
            builder.Dispose();
            return blobAssetRef;
        }

        public static BlobAssetReference<BlobArray<T>> ToAssetRef(
            ref BlobBuilder builder,
            BakingSchema<T>[] datas
        )
        {
            ref var blobArray = ref builder.ConstructRoot<BlobArray<T>>();
            var arrayBuilder = builder.Allocate(ref blobArray, datas.Length);
            for (int i = 0; i < datas.Length; i++) arrayBuilder[i] = datas[i].ToData();
            return builder.CreateBlobAssetReference<BlobArray<T>>(Allocator.Persistent);
        }

        public static void ToUshortArray<TV>(ref BlobBuilder builder, ref BlobArray<ushort> blobArray, TV[] schemas)
            where TV : IHasID, IEquatable<ushort>
        {
            var array = builder.Allocate(ref blobArray, schemas.Length);
            for (int i = 0; i < schemas.Length; i++) array[i] = (ushort)schemas[i].ID;
        }
    }
}