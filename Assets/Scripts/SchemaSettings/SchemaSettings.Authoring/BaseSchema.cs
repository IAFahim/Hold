using System.Runtime.CompilerServices;
using BovineLabs.Core.ObjectManagement;
using Unity.Entities;
using UnityEngine;

namespace SchemaSettings.SchemaSettings.Authoring
{
    public abstract class BaseSchema<T> : ScriptableObject, IUID where T : struct
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public static void CreateBlobArray<TV>(ref BlobBuilder builder, ref BlobArray<ushort> blobArray, TV[] schemas)
            where TV : IUID
        {
            var array = builder.Allocate(ref blobArray, schemas.Length);
            for (int i = 0; i < schemas.Length; i++)
                array[i] = (ushort)schemas[i].ID;
        }
    }
}