// <copyright file="GameViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using BovineLabs.Anchor;
using BovineLabs.Anchor.Contracts;
using Unity.Entities;
using Unity.Properties;
using UnityEngine;

namespace BovineLabs.Sample.UI.ViewModels.Game
{
    public partial class GameViewModel : SystemObservableObject<GameViewModel.Data>
    {
        
        [CreateProperty(ReadOnly = true)]
        public int Gold => this.Value.Gold;

        public partial struct Data : IComponentData
        {
            [SystemProperty] [SerializeField] private int gold;
        }
    }
}