// <copyright file="GameView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using _src.Scripts.UiServices.UXMLs.Service;
using Unity.Properties;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using BovineLabs.Sample.UI.ViewModels.Game;
using Unity.AppUI.Navigation;

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
            MainGameObjectCamera.Instance.orthographic = false;
        }

        public override void OnEnter(NavController controller, NavDestination destination, Argument[] args)
        {
            base.OnEnter(controller, destination, args);
            UIToolkitJoystick.Instance.enabled = true;
        }

        public override void OnExit(NavController controller, NavDestination destination, Argument[] args)
        {
            base.OnExit(controller, destination, args);
            UIToolkitJoystick.Instance.enabled = false;
        }


        private void Bind(VisualElement root)
        {
            var goldContainer = root.Q("gold_container");
            goldContainer.BindText(nameof(GameViewModel.StarCount), ViewModel, "text");
        }
        
        
    }
}