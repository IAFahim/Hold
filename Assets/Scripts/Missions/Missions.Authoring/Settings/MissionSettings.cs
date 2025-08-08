using System;
using BovineLabs.Core.Settings;
using Missions.Missions.Authoring.Schemas;
using UnityEngine;

namespace Missions.Missions.Authoring.Settings
{
    [SettingsGroup("Mission")]
    public class MissionSettings : ScriptableObject, ISettings
    {
        [SerializeField] public MissionSchema[] schemas = Array.Empty<MissionSchema>();
    }
}