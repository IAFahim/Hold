using BovineLabs.Core.ObjectManagement;
using System;
using Missions.Missions.Authoring.Scriptable;
using Missions.Missions.Authoring.Settings;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [AutoRef(
        nameof(NameSettings), nameof(NameSettings.schemas),
        FieldName, TypeString + "/" + FieldName
    )]
    public class NameSchema : BakingSchema<Name>
    {
        private const string FieldName = nameof(NameSchema);
        private const string TypeString = "Name";

        [FormerlySerializedAs("fixed32")] public FixedString32Bytes fixed32String;

        public override Name ToData()
        {
            return new Name
            {
                id = (ushort)ID,
                fixedString32 = fixed32String,
            };
        }

    }
}