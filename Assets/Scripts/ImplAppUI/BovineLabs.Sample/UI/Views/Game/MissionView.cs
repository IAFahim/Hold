using _src.Scripts.UiServices.UXMLs.Service;
using BovineLabs.Sample.UI.ViewModels.Game;

namespace BovineLabs.Sample.UI.Views.Game
{
    public class MissionView : MissionBaseView<MissionViewModel>
    {
        public MissionView(MissionViewModel viewModel, IUxmlService uxmlService) : base(viewModel)
        {
            var visualTreeAsset = uxmlService.GetAsset("mission");
            MainGameObjectCamera.Instance.orthographic = false;
        }
    }
}