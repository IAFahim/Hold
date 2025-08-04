using System;
using BovineLabs.Core.Settings;
using UnityEngine;

namespace Maps.Maps.Data
{
    [SettingsGroup("Mission")]
    public class MissionSettings : ScriptableObject, ISettings
    {
        [SerializeField] public MissionSchema[] schemas = Array.Empty<MissionSchema>();
    }
}