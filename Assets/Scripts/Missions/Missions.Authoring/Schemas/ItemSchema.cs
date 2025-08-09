using System;
using BovineLabs.Core.ObjectManagement;
using Missions.Missions.Authoring.Scriptable;
using Missions.Missions.Authoring.Settings;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Search;

namespace Missions.Missions.Authoring.Schemas
{
    [Flags]
    public enum ItemFlags : byte
    {
        None = 0,
        Fragile = 1 << 0,
        Heavy = 1 << 1,
        Lightweight = 1 << 2,
        Banned = 1 << 3,
    }

    [CreateAssetMenu(menuName = "Hold/Items/Create ItemSchema", fileName = nameof(ItemSchema))]
    [
        AutoRef(
            nameof(ItemSettings), nameof(ItemSettings.schemas),
            nameof(ItemSchema), "Items/" + nameof(ItemSchema)
        )
    ]
    public class ItemSchema : BakingSchema<Item>
    {
        [Tooltip("Display name reference")] [SearchContext("item")]
        public NameSchema nameSchema;

        [Min(0)] public float weightKg;

        [Tooltip("Bitmask flags describing item properties")]
        public ItemFlags flags;

        public override Item ToData()
        {
            return new Item
            {
                id = (ushort)ID,
                nameId = (ushort)(nameSchema ? nameSchema.ID : 0),
                weightKg = weightKg,
                flags = flags
            };
        }
    }

    [Serializable]
    public struct Item
    {
        public ushort id;
        public ushort nameId;
        public float weightKg;
        public ItemFlags flags;
    }
}