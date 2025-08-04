
using System;
using BovineLabs.Core.Settings;
using UnityEngine;

namespace Rewards.Rewards.Authoring.Settings
{
    public class RewardSettings<T>: ScriptableObject, ISettings
    {
        [SerializeField] public T[] schemas = Array.Empty<T>();
    }
}
