using System;
using BovineLabs.Core.Settings;
using Data;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    public class NameSettings : ScriptableObject, ISettings
    {
        public NameSchema[] schemas = Array.Empty<NameSchema>();
    }
}