using System;
using BovineLabs.Core.ObjectManagement;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Data
{
    public interface ID : IEquatable<ushort>
    {
        public int ID { get; set; }
    }

    [Serializable]
    public struct Mission : ID
    {
        public ushort id;

        public ushort station;
        public ushort name;

        public BlobArray<ushort> Goals;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(ref Mission other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }

    public class StationSchema : BakingSchema<Station>
    {
        public NameSchema nameSchema;
        public float3 position;
        public override Station ToData()
        {
            return new Station
            {
                id = id,
                nameId = nameSchema.id
            };
        }
    }

    [Serializable]
    public struct Station : ID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public ushort nameId;
        public float3 position;

        public bool Equals(Station other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public static implicit operator ushort(Station obj) => obj.id;
        public bool Equals(ushort other) => id == other;
    }

    public class NameSchema : BakingSchema<Name>
    {
        public FixedString32Bytes str;

        public override Name ToData()
        {
            return new Name
            {
                id = id,
                name = str,
            };
        }
    }

    [Serializable]
    public struct Name : ID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public FixedString32Bytes name;

        public bool Equals(Name other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public static implicit operator ushort(Name obj) => obj.id;
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public struct Goal : ID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public NumType numType;
        public DataType dataType;
        public ushort rangeId;

        public bool Equals(Goal other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public static implicit operator ushort(Goal obj) => obj.id;
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public struct Reward : ID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public CrossLinkType crossLinkType;
        public DataType dataType;
        public NumType numType;

        public bool Equals(Reward other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public static implicit operator ushort(Reward obj) => obj.id;
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public struct Data : ID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public DataType dataType;
        public ushort numId;

        public bool Equals(Data other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public static implicit operator ushort(Data obj) => obj.id;
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public struct NumFloat : ID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public float value;

        public bool Equals(NumFloat other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public static implicit operator ushort(NumFloat obj) => obj.id;
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public struct NumInt : ID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public int value;

        public bool Equals(NumInt other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public static implicit operator ushort(NumInt obj) => obj.id;
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public struct RangeFloat : ID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public CheckType checkType;
        public float lower;
        public float upper;

        public bool Equals(RangeFloat other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public static implicit operator ushort(RangeFloat obj) => obj.id;
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public struct RangeInt : ID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public CheckType checkType;
        public int lower;
        public int upper;

        public bool Equals(RangeInt other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public static implicit operator ushort(RangeInt obj) => obj.id;
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public struct Time : ID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public CrossLinkType crossLinkType;

        public bool Equals(Time other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public static implicit operator ushort(Time obj) => obj.id;
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public enum CrossLinkType : byte
    {
        Mission,
        Goal, // Fixed typo from "Gaol"
        Reward
    }

    public enum DataType : byte
    {
        Intrinsic,
        Stat,
        Inventory,
        Time
    }

    public enum NumType : byte
    {
        Float,
        Int,
    }

    public enum CheckType : byte
    {
        GreaterOrEqual = 0,
        GreaterThan = 1,
        LessOrEqual = 2,
        LessThan = 3,
        Equals = 4,
        NotEqual = 5,
        Between = 6,
        NotBetween = 7
    }

    public abstract class BaseSchema : ScriptableObject, IUID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public static void ToBlobArray<TV>(ref BlobBuilder builder, ref BlobArray<ushort> blobArray, TV[] schemas)
            where TV : IUID
        {
            var array = builder.Allocate(ref blobArray, schemas.Length);
            for (int i = 0; i < schemas.Length; i++) array[i] = (ushort)schemas[i].ID;
        }
    }

    public abstract class BakingSchema<T> : BaseSchema where T : struct
    {
        public abstract T ToData();

        public static BlobAssetReference<BlobArray<T>> ToBlobAssetRef(BakingSchema<T>[] datas)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            var blobAssetRef = ToBlobAssetRef(ref builder, datas);
            builder.Dispose();
            return blobAssetRef;
        }

        public static BlobAssetReference<BlobArray<T>> ToBlobAssetRef(
            ref BlobBuilder builder,
            BakingSchema<T>[] datas
        )
        {
            ref var blobArray = ref builder.ConstructRoot<BlobArray<T>>();
            var arrayBuilder = builder.Allocate(ref blobArray, datas.Length);
            for (int i = 0; i < datas.Length; i++) arrayBuilder[i] = datas[i].ToData();
            var blobAssetRef = builder.CreateBlobAssetReference<BlobArray<T>>(Allocator.Persistent);
            return blobAssetRef;
        }
    }
}