using System;
using BovineLabs.Core.Settings;
using UnityEngine;

namespace Goals.Goals.Authoring.Settings
{
    public class GoalSettings<T>: ScriptableObject, ISettings
    {
        [SerializeField] public T[] schemas = Array.Empty<T>();
    }
}
