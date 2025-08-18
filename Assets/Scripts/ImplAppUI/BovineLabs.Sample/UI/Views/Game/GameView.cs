// <copyright file="GameView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using _src.Scripts.UiServices.UXMLs.Service;
using Unity.AppUI.UI;
using Unity.Collections;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace BovineLabs.Sample.UI.Views.Game
{
    using BovineLabs.Sample.UI.ViewModels.Game;

    public class GameView : GameBaseView<GameViewModel>
    {
        public GameView(GameViewModel viewModel, IUxmlService uxmlService) : base(viewModel)
        {
            var visualTreeAsset = uxmlService.GetAsset("game");
            var root = visualTreeAsset.Instantiate().contentContainer[0];
            Add(root);
            // SetTime(root, "text_gold");
        }

        
    }
}