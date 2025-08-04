// <copyright file="GameViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using BovineLabs.Anchor;
using Unity.Properties;

namespace BovineLabs.Sample.UI.ViewModels.Game
{
    public class GameViewModel : SystemObservableObject<GameViewModel.Data>
    {
        public partial struct Data
        {
            [CreateProperty(ReadOnly = true)] public float Gold;
        }
    }
}