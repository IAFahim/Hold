// <copyright file="HomeView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using _src.Scripts.UiServices.UXMLs.Service;
using BovineLabs.Core;
using Unity.AppUI.Navigation.Generated;

namespace BovineLabs.Sample.UI.Views.Menu
{
    using BovineLabs.Anchor;
    using BovineLabs.Sample.UI.ViewModels.Menu;
    using Unity.AppUI.Navigation;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    public class HomeView : MenuBaseView<HomeViewModel>
    {
        public const string UssHomeClassName = "bl-home-view";


        public HomeView(IUxmlService uxmlService, HomeViewModel viewModel)
            : base(viewModel)
        {
            var root = uxmlService.GetAsset("mission").Instantiate().contentContainer[0];
            Add(root);
            root.Q<UnityEngine.UIElements.Button>("play-button").clicked += Play;
        }

        public override void OnEnter(NavController controller, NavDestination destination, Argument[] args)
        {
            base.OnEnter(controller, destination, args);
        }

        private void Play()
        {
            this.Navigate(Actions.home_to_play);
            BovineLabsBootstrap.Instance.CreateGameWorld();
        }

        private void Load()
        {
        }

        private void Options()
        {
            this.Navigate(Actions.go_to_options);
        }
    }
}