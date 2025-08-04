using System;
using BovineLabs.Core.Settings;
using Goals.Goals.Authoring.Schema;
using UnityEngine;

namespace Goals.Goals.Authoring.Settings
{
    
    [SettingsGroup("Goal")]
    public class GoalSettings<T>: ScriptableObject, ISettings
    {
        [SerializeField] public T[] schemas = Array.Empty<T>();
    }
}