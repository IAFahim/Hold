namespace BovineLabs.Sample.UI.Views.Game
{
    public abstract class MissionBaseView<T> : BaseScreen<T>
    {
        public const string GameClassName = "bl-game-map-view";
        protected MissionBaseView(T viewModel) : base(viewModel) => AddToClassList(GameClassName);
    }
}