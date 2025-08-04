using System;
using BovineLabs.Core.Settings;
using Goals.Goals.Authoring.Schema;
using UnityEngine;

namespace Goals.Goals.Authoring.Settings
{
    
    [SettingsGroup("Goal")]
    public class GoalTimeSettings : ScriptableObject, ISettings
    {
        [SerializeField] public GoalTimeSchema[] schemas = Array.Empty<GoalTimeSchema>();
    }
}