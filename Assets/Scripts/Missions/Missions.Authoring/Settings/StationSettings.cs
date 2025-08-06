using System;
using BovineLabs.Core.Settings;
using Missions.Missions.Authoring.Schemas;
using UnityEngine;

namespace Missions.Missions.Authoring.Settings
{
    public class StationSettings : ScriptableObject, ISettings
    {
        public StationSchema[] schemas = Array.Empty<StationSchema>();
    }
}