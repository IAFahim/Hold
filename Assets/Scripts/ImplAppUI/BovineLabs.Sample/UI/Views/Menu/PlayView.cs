// <copyright file="PlayView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using BovineLabs.Core;
using Unity.AppUI.Navigation.Generated;

namespace BovineLabs.Sample.UI.Views.Menu
{
    using BovineLabs.Anchor;
    using BovineLabs.Sample.UI.ViewModels.Menu;

    public class PlayView : MenuBaseView<HomeViewModel>
    {
        public const string UssClassName = "bl-play-view";

        private const string PrivateText = "@UI:privateGame";
        private const string PrivateSubText = "@UI:privateGameSub";
        private const string HostText = "@UI:hostGame";
        private const string HostSubText = "@UI:hostGameSub";
        private const string JoinText = "@UI:joinGame";
        private const string JoinSubText = "@UI:joinGameSub";

        public PlayView(HomeViewModel viewModel)
            : base(viewModel)
        {
            
        }

        private void JoinGame()
        {
            this.ViewModel.Value.Join.TryProduce();
            this.Navigate(Actions.go_to_game);
            BovineLabsBootstrap.Instance.CreateGameWorld();
        }
    }
}
