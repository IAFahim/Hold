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
            
            
#if UNITY_STANDALONE
            var quitButton = new ActionButton
            {
                label = QuitText,
            };

            quitButton.AddToClassList(ButtonClassName);
            quitButton.clickable.clickedWithEventInfo += Quit;
            left.Add(quitButton);
#endif
        }

        public override void OnEnter(NavController controller, NavDestination destination, Argument[] args)
        {
            base.OnEnter(controller, destination, args);
        }

#if UNITY_STANDALONE
        private static void Quit(EventBase evt)
        {
            if (evt.target is ExVisualElement btn)
            {
                var dialog = new AlertDialog
                {
                    description = QuitDescriptionText,
                    variant = AlertSemantic.Destructive,
                    title = QuitTitleText,
                };

                dialog.SetPrimaryAction(99, QuitText, () =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    UnityEngine.Application.Quit();
#endif
                });

                dialog.SetCancelAction(1, QuitCancelText);

                var modal = Modal.Build(btn, dialog);
                modal.Show();
            }
        }

#endif
        private void Play()
        {
            this.Navigate(Actions.home_to_play);
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