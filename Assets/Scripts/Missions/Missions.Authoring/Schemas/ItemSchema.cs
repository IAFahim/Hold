using System;
using BovineLabs.Core.ObjectManagement;
using Missions.Missions.Authoring.Scriptable;
using Missions.Missions.Authoring.Settings;
using UnityEngine;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/Items/Create ItemSchema", fileName = nameof(ItemSchema))]
    [
        AutoRef(
            nameof(ItemSettings), nameof(ItemSettings.schemas),
            nameof(ItemSchema), "Items/" + nameof(ItemSchema)
        )
    ]
    public class ItemSchema : BakingSchema<Item>
    {
        [Tooltip("Display name reference")] public NameSchema name;
        [Min(0)] public float weightKg;
        public bool isFragile;
        public bool isHeavy;
        public bool isLightweight;
        public bool banned;

        public override Item ToData()
        {
            return new Item
            {
                id = (ushort)ID,
                nameId = (ushort)(name ? name.ID : 0),
                weightKg = weightKg,
                flags = (byte)((isFragile ? 1 : 0) | (isHeavy ? 1 << 1 : 0) | (isLightweight ? 1 << 2 : 0) | (banned ? 1 << 3 : 0))
            };
        }
    }

    [Serializable]
    public struct Item
    {
        public ushort id;
        public ushort nameId;
        public float weightKg;
        public byte flags;
    }
}