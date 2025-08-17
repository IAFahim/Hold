// In your UI assembly (e.g., YourProject.UI)

using System;
using BovineLabs.Anchor;
using BovineLabs.Anchor.Contracts;
using BovineLabs.Core.Utility;
using Unity.Entities;
using Unity.Properties;
using UnityEngine;

namespace BovineLabs.Sample.UI.ViewModels.Game
{
    public class MissionViewModel: SystemObservableObject<MissionViewModel.Data>
    {
        
        public struct Data : IComponentData
        {
            public ButtonEvent StationNext;
            public ButtonEvent StationLast;
            public ButtonEvent MapNext;
            public ButtonEvent MapLast;
        }
    }
}