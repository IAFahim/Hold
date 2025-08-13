// <copyright file="GameViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using BovineLabs.Anchor;
using BovineLabs.Anchor.Contracts;
using Unity.Properties;

namespace BovineLabs.Sample.UI.ViewModels.Game
{
    public partial class GameViewModel : SystemObservableObject<GameViewModel.Data>
    {
        
        [CreateProperty(ReadOnly = true)]
        public int Gold => this.Value.Gold;

        public partial struct Data
        {
            [SystemProperty] public int gold;
        }
    }
}