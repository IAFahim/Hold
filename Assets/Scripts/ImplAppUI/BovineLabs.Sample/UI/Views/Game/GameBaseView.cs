// <copyright file="GameBaseView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using _src.Scripts.UiServices.UXMLs.Service;
using UnityEngine;

namespace BovineLabs.Sample.UI.Views.Game
{
    public abstract class GameBaseView<T> : BaseScreen<T>
    {
        public const string GameClassName = "bl-game-view";
        protected GameBaseView(T viewModel) : base(viewModel) => AddToClassList(GameClassName);
    }
}