using System.Runtime.CompilerServices;
using Unity.AppUI.UI;
using Unity.Collections;
using Unity.Properties;
using UnityEngine.UIElements;

namespace BovineLabs.Sample.UI.Views
{
    public static class UIToolKitExt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Text BindText(
            this VisualElement root,
            in FixedString32Bytes propertyPath,
            object viewModel,
            in FixedString32Bytes className,
            BindingMode mode = BindingMode.ToTarget
        )
        {
            var text = root.Q<Text>(className.ToString());
            text.SetBinding(nameof(Text.text), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSource = viewModel,
                dataSourcePath = new PropertyPath(propertyPath.ToString())
            });
            return text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Text BindText(
            this VisualElement root,
            in string propertyPath,
            object viewModel,
            in string className,
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
        //
    }
}