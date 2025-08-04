using System;
using BovineLabs.Core.Settings;
using Goals.Goals.Authoring.Schema;
using UnityEngine;

namespace Goals.Goals.Authoring.Settings
{
    
    [SettingsGroup("Goal")]
    public class GoalFloatSettings : ScriptableObject, ISettings
    {
        [SerializeField] public GoalFloatSchema[] schemas = Array.Empty<GoalFloatSchema>();
    }
}