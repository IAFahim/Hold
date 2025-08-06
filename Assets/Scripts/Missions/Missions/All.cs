using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Data
{
    public interface IHasID
    {
        int ID { get; set; }
    }

    [Serializable]
    public struct Mission : IHasID, IEquatable<ushort>
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

        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public struct Station : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public ushort nameId;
        public float3 position;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(Station other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public struct Name : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public FixedString32Bytes name;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(Name other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public struct RangeFloat : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public CheckType checkType;
        public float lower;
        public float upper;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(RangeFloat other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public struct RangeInt : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public CheckType checkType;
        public int lower;
        public int upper;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(RangeInt other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public struct Time : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public CrossLinkType crossLinkType;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(Time other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }

    [Serializable]
    public enum CrossLinkType : byte
    {
        Mission,
        Goal,
        Reward
    }

    public enum TargetType : byte
    {
        Intrinsic,
        Stat,
        Inventory,
        Time
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

    public abstract class BaseSchema : ScriptableObject, IHasID
    {
        [SerializeField] private ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }
    }

    public abstract class BakingSchema<T> : BaseSchema where T : struct, IHasID
    {
        public abstract T ToData();

        public static void ToBlobArray<TV>(ref BlobBuilder builder, ref BlobArray<ushort> blobArray, TV[] schemas)
            where TV : IHasID, IEquatable<ushort>
        {
            var array = builder.Allocate(ref blobArray, schemas.Length);
            for (int i = 0; i < schemas.Length; i++)
                array[i] = (ushort)schemas[i].ID;
        }

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
            for (int i = 0; i < datas.Length; i++)
                arrayBuilder[i] = datas[i].ToData();
            return builder.CreateBlobAssetReference<BlobArray<T>>(Allocator.Persistent);
        }
    }

    // ====================== BAKING SCHEMAS ======================
    public class MissionSchema : BakingSchema<Mission>
    {
        public NameSchema nameSchema;
        public StationSchema stationSchema;
        public GoalSchema[] goals;

        public override Mission ToData()
        {
            return new Mission
            {
                id = (ushort)ID,
                station = (ushort)stationSchema.ID,
                name = (ushort)nameSchema.ID
            };
        }
    }

    public class StationSchema : BakingSchema<Station>
    {
        public NameSchema nameSchema;
        public float3 position;

        public override Station ToData()
        {
            return new Station
            {
                id = (ushort)ID,
                nameId = (ushort)nameSchema.ID,
                position = position
            };
        }
    }

    public class NameSchema : BakingSchema<Name>
    {
        public FixedString32Bytes string32;

        public override Name ToData()
        {
            return new Name
            {
                id = (ushort)ID,
                name = string32
            };
        }
    }

    public class GoalSchema : BakingSchema<Goal>
    {
        public TargetType targetType;
        public BaseSchema rangeSchema;

        public override Goal ToData()
        {
            return new Goal
            {
                id = (ushort)ID,
                rangeType = rangeSchema.ToNumType(),
                targetType = targetType,
                rangeId = (ushort)rangeSchema.ID
            };
        }
    }

    public class RewardSchema : BakingSchema<Reward>
    {
        public BaseSchema crossLink;
        public DataContainerSchema dataContainer;

        public override Reward ToData()
        {
            return new Reward
            {
                id = (ushort)ID,
                crossLinkType = crossLink.ToCrossLinkType(),
                crossLinkID = (ushort)crossLink.ID,
                dataContainerID = (ushort)dataContainer.ID
            };
        }
    }

    public class DataContainerSchema : BakingSchema<DataContainer>
    {
        public TargetType targetType;
        public float valueFloat;
        public int valueInt;

        public override DataContainer ToData()
        {
            return new DataContainer
            {
                id = (ushort)ID,
                targetType = targetType,
                valueFloat = valueFloat,
                valueInt = valueInt
            };
        }
    }

    public class RangeFloatSchema : BakingSchema<RangeFloat>
    {
        public CheckType checkType;
        public float lower;
        public float upper;

        public override RangeFloat ToData()
        {
            return new RangeFloat
            {
                id = (ushort)ID,
                checkType = checkType,
                lower = lower,
                upper = upper
            };
        }
    }

    public class RangeIntSchema : BakingSchema<RangeInt>
    {
        public CheckType checkType;
        public int lower;
        public int upper;

        public override RangeInt ToData()
        {
            return new RangeInt
            {
                id = (ushort)ID,
                checkType = checkType,
                lower = lower,
                upper = upper
            };
        }
    }

    public class TimeSchema : BakingSchema<Time>
    {
        public BaseSchema crossLinkType;

        public override Time ToData()
        {
            return new Time
            {
                id = (ushort)ID,
                crossLinkType = crossLinkType.ToCrossLinkType()
            };
        }
    }

    internal static class BaseSchemaExt
    {
        public static NumType ToNumType(this BaseSchema baseSchema)
        {
            return baseSchema switch
            {
                RangeFloatSchema => NumType.Float,
                RangeIntSchema => NumType.Int,
                _ => throw new ArgumentOutOfRangeException(nameof(baseSchema), baseSchema, null)
            };
        }

        public static CrossLinkType ToCrossLinkType(this BaseSchema baseSchema)
        {
            return baseSchema switch
            {
                MissionSchema => CrossLinkType.Mission,
                GoalSchema => CrossLinkType.Goal,
                RewardSchema => CrossLinkType.Reward,
                _ => throw new ArgumentOutOfRangeException(nameof(baseSchema), baseSchema, null)
            };
        }
    }
}