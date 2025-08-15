// <copyright file="GameView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using _src.Scripts.UiServices.UXMLs.Service;
using Unity.AppUI.UI;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace BovineLabs.Sample.UI.Views.Game
{
    using BovineLabs.Sample.UI.ViewModels.Game;

    public class GameView : GameBaseView<GameViewModel>
    {
        public GameView(GameViewModel viewModel,IUxmlService uxmlService) : base(viewModel)
        {
            /*this.Add(abilityToolbarView);*/
            var visualTreeAsset = uxmlService.GetAsset("game");
            var root = visualTreeAsset.Instantiate().contentContainer[0];
            Add(root);
            SetTime(root);
        }

        private void SetTime(VisualElement root)
        {
            var timeText = root.Q<Text>("text_gold");
            timeText.dataSource = ViewModel;
            timeText.SetBinding(nameof(Text.text), new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(GameViewModel.StarCount))
            });
        }
    }
}