using System;
using BovineLabs.Core.Settings;
using Missions.Missions.Authoring.Schemas;
using UnityEngine;

namespace Missions.Missions.Authoring.Settings
{
    public class GoalSettings : ScriptableObject, ISettings
    {
        public GoalSchema[] schemas = Array.Empty<GoalSchema>();
    }
}