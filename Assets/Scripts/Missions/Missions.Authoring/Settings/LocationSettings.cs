using System;
using BovineLabs.Core.Settings;
using Missions.Missions.Authoring.Schemas;
using UnityEngine;

namespace Missions.Missions.Authoring.Settings
{
    public class LocationSettings : ScriptableObject, ISettings
    {
        public LocationSchema[] schemas = Array.Empty<LocationSchema>();
    }
}