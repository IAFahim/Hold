// using Unity.Collections;
// using Unity.Entities;
//
// namespace SchemaSettings.SchemaSettings.Authoring
// {
//     public abstract class BakingSchema<T> : BaseSchema<T> where T : struct
//     {
//         public abstract T ToData();
//         
//         public static BlobAssetReference<BlobArray<T>> ToBlobAssetRef(BakingSchema<T>[] datas)
//         {
//             var builder = new BlobBuilder(Allocator.Temp);
//             var blobAssetRef = ToBlobAssetRef(ref builder, datas);
//             builder.Dispose();
//             return blobAssetRef;
//         }
//         
//         public static BlobAssetReference<BlobArray<T>> ToBlobAssetRef(ref BlobBuilder builder, BakingSchema<T>[] datas)
//         {
//             ref var blobArray = ref builder.ConstructRoot<BlobArray<T>>();
//             var arrayBuilder = builder.Allocate(ref blobArray, datas.Length);
//             for (int i = 0; i < datas.Length; i++) arrayBuilder[i] = datas[i].ToData();
//             var blobAssetRef = builder.CreateBlobAssetReference<BlobArray<T>>(Allocator.Persistent);
//             return blobAssetRef;
//         }
//     }
// }