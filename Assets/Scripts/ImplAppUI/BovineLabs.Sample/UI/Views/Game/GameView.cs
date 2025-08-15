// <copyright file="GameView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using _src.Scripts.UiServices.UXMLs.Service;
using Unity.Properties;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using BovineLabs.Sample.UI.ViewModels.Game;

namespace BovineLabs.Sample.UI.Views.Game
{
    public class GameView : GameBaseView<GameViewModel>
    {
        public GameView(GameViewModel viewModel, IUxmlService uxmlService) : base(viewModel)
        {
            var visualTreeAsset = uxmlService.GetAsset("game");
            var root = visualTreeAsset.Instantiate().contentContainer[0];
            Add(root);
            Bind(root);
        }

        private void Bind(VisualElement root)
        {
            var goldContainer = root.Q("gold_container");
            goldContainer.BindText(nameof(GameViewModel.StarCount), ViewModel, "text");
        }
    }
}