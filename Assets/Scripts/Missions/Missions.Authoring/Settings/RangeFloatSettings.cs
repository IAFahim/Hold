using System;
using BovineLabs.Core.Settings;
using Missions.Missions.Authoring.Schemas;
using UnityEngine;

namespace Missions.Missions.Authoring.Settings
{
    public class RangeFloatSettings : ScriptableObject, ISettings
    {
        public RangeFloatSchema[] schemas = Array.Empty<RangeFloatSchema>();
    }
}
