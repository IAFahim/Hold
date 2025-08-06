using System;
using BovineLabs.Core.Settings;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    [SettingsGroup("Mission")]
    public class MissionSettings : ScriptableObject, ISettings
    {
        [SerializeField] public MissionSchema[] schemas = Array.Empty<MissionSchema>();
    }
}