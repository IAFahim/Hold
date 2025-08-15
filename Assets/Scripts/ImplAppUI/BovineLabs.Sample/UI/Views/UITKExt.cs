using System.Runtime.CompilerServices;
using Unity.AppUI.UI;
using Unity.Properties;
using UnityEngine.UIElements;

namespace BovineLabs.Sample.UI.Views
{
    public static class UITKExt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Text SetText(
            this VisualElement root,
            string className,
            object viewModel,
            string propertyPath,
            BindingMode mode = BindingMode.ToTarget
        )
        {
            var text = root.Q<Text>(className);
            text.SetBinding(nameof(Text.text), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSource = viewModel,
                dataSourcePath = new PropertyPath(propertyPath)
            });
            return text;
        }
    }
}