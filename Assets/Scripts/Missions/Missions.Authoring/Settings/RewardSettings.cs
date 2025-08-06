using System;
using BovineLabs.Core.Settings;
using Missions.Missions.Authoring.Schemas;
using UnityEngine;

namespace Missions.Missions.Authoring.Settings
{
    public class RewardSettings : ScriptableObject, ISettings
    {
        public RewardSchema[] schemas = Array.Empty<RewardSchema>();
    }
}
