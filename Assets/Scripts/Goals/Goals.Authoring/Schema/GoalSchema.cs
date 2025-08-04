using BovineLabs.Core.ObjectManagement;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Goals.Goals.Authoring.Schema
{
    public abstract class GoalSchema<T> : ScriptableObject, IUID where T : struct
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public abstract T ToGoal();

        public static BlobAssetReference<BlobArray<T>> CreateBlobAssetRef(GoalSchema<T>[] datas)
        {
            using var builder = new BlobBuilder(Allocator.Temp);
            ref var blobArray = ref builder.ConstructRoot<BlobArray<T>>();
            var arrayBuilder = builder.Allocate(ref blobArray, datas.Length);
            for (int i = 0; i < datas.Length; i++) arrayBuilder[i] = datas[i].ToGoal();
            var blobAssetRef = builder.CreateBlobAssetReference<BlobArray<T>>(Allocator.Persistent);
            return blobAssetRef;
        }
    }
}