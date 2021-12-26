using Avalonia;
using Avalonia.Controls;

namespace PatchGenerator.AttachedProperties
{
    /// <summary>
    /// Just a random boolean value for you to attach and use to any control.
    /// </summary>
    public class RandomBoolAttProp : AvaloniaObject
    {
        public static readonly AttachedProperty<bool> RandomBoolProperty =
            AvaloniaProperty.RegisterAttached<RandomBoolAttProp, Control, bool>("RandomBool");

        public static bool GetRandomBool(Control control)
        {
            return control.GetValue(RandomBoolProperty);
        }

        public static void SetRandomBool(Control control, bool value)
        {
            control.SetValue(RandomBoolProperty, value);
        }
    }
}
