// <copyright file="GameView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using _src.Scripts.UiServices.UXMLs.Service;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace BovineLabs.Sample.UI.Views.Game
{
    using BovineLabs.Sample.UI.ViewModels.Game;

    public class GameView : GameBaseView<GameViewModel>
    {
        public GameView(IUxmlService uxmlService)
            : base(new GameViewModel())
        {
            /*this.Add(abilityToolbarView);*/
            var visualTreeAsset = uxmlService.GetAsset("game");
            var root = visualTreeAsset.Instantiate().contentContainer[0];
            Add(root);
            var text = root.Q<Text>("text_gold");
            Debug.Log(text.text);
        }
    }
}