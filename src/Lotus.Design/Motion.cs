using System.Numerics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace Lotus.Design;

/// <summary>
/// Small, GPU-driven motion shared by the Lotus apps. Uses WinUI's built-in Scale/Translation transitions,
/// so nothing runs between interactions, and everything is off when Windows "Animation effects" is off.
/// XAML: &lt;Button design:Motion.Lift="True" /&gt;
/// </summary>
public static class Motion
{
    private static readonly Lazy<bool> enabled = new(() =>
    {
        try { return new Windows.UI.ViewManagement.UISettings().AnimationsEnabled; }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or InvalidOperationException) { return true; }
    });

    /// <summary>False when the player asked Windows to reduce animations.</summary>
    public static bool Enabled => enabled.Value;

    public static readonly DependencyProperty LiftProperty =
        DependencyProperty.RegisterAttached("Lift", typeof(bool), typeof(Motion), new PropertyMetadata(false, OnLiftChanged));

    public static bool GetLift(DependencyObject element) => (bool)element.GetValue(LiftProperty);

    public static void SetLift(DependencyObject element, bool value) => element.SetValue(LiftProperty, value);

    private static void OnLiftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement element && e.NewValue is true) Attach(element);
    }

    /// <summary>Hover and keyboard focus lift the element a little; pressing sinks it.</summary>
    public static void Attach(FrameworkElement element, float lift = 5f, float grow = 1.03f)
    {
        if (!Enabled) return;
        element.ScaleTransition = new Vector3Transition { Duration = TimeSpan.FromMilliseconds(160) };
        element.TranslationTransition = new Vector3Transition { Duration = TimeSpan.FromMilliseconds(160) };
        element.SizeChanged += (_, a) => element.CenterPoint = new Vector3((float)a.NewSize.Width / 2, (float)a.NewSize.Height / 2, 0);

        var hovered = false;
        var focused = false;
        void Apply(float scale, float up)
        {
            element.Scale = new Vector3(scale, scale, 1);
            element.Translation = new Vector3(0, -up, 0);
        }
        void Rest() => Apply(hovered || focused ? grow : 1f, hovered || focused ? lift : 0f);

        element.PointerEntered += (_, _) => { hovered = true; Rest(); };
        element.PointerExited += (_, _) => { hovered = false; Rest(); };
        element.PointerCanceled += (_, _) => { hovered = false; Rest(); };
        element.PointerCaptureLost += (_, _) => Rest();
        element.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler((_, _) => Apply(0.98f, 0f)), handledEventsToo: true);
        element.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler((_, _) => Rest()), handledEventsToo: true);
        element.GotFocus += (_, _) => { focused = element.FocusState == FocusState.Keyboard; Rest(); };
        element.LostFocus += (_, _) => { focused = false; Rest(); };
    }
}
