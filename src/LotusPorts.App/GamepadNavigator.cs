using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Windows.Gaming.Input;

namespace LotusPorts.App;

/// <summary>What a controller button does in Lotus Ports.</summary>
public enum PadAction
{
    Up,
    Down,
    Left,
    Right,
    Activate,
    Back,
    PreviousSection,
    NextSection,
    Search,
    ToggleTvMode,
}

/// <summary>
/// Full navigation by controller (master prompt, phase 7): d-pad and left stick move the focus, A activates, B goes back,
/// LB/RB switch sections, Y searches, View toggles TV mode. The controller is read only while a controller is connected
/// and the window is active, so nothing runs in the background while you play.
/// </summary>
public sealed class GamepadNavigator
{
    private static readonly TimeSpan FirstRepeat = TimeSpan.FromMilliseconds(380);
    private static readonly TimeSpan NextRepeat = TimeSpan.FromMilliseconds(120);

    private readonly DispatcherQueueTimer timer;
    private readonly Func<XamlRoot?> root;
    private GamepadButtons previous;
    private PadAction? held;
    private DateTime heldSince, lastRepeat;
    private bool windowActive = true;

    /// <summary>Raised for actions the page cannot do by focus alone (back, sections, search, TV mode).</summary>
    public event Action<PadAction>? Command;

    public GamepadNavigator(DispatcherQueue dispatcher, Func<XamlRoot?> root)
    {
        this.root = root;
        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(16);
        timer.Tick += (_, _) => Poll();
        Gamepad.GamepadAdded += (_, _) => dispatcher.TryEnqueue(Update);
        Gamepad.GamepadRemoved += (_, _) => dispatcher.TryEnqueue(Update);
        Update();
    }

    public bool HasController => Gamepad.Gamepads.Count > 0;

    /// <summary>Polling stops while the window is in the background (a game is open) and starts again when it returns.</summary>
    public void SetWindowActive(bool active)
    {
        windowActive = active;
        Update();
    }

    private void Update()
    {
        var run = windowActive && HasController;
        if (run && !timer.IsRunning)
        {
            previous = Gamepad.Gamepads.FirstOrDefault()?.GetCurrentReading().Buttons ?? GamepadButtons.None; // no action for buttons already down
            timer.Start();
        }
        else if (!run && timer.IsRunning) timer.Stop();
    }

    private void Poll()
    {
        if (Gamepad.Gamepads.FirstOrDefault() is not { } pad) { Update(); return; }
        var reading = pad.GetCurrentReading();
        var pressed = reading.Buttons & ~previous;
        previous = reading.Buttons;

        if (pressed.HasFlag(GamepadButtons.A)) Run(PadAction.Activate);
        if (pressed.HasFlag(GamepadButtons.B)) Run(PadAction.Back);
        if (pressed.HasFlag(GamepadButtons.LeftShoulder)) Run(PadAction.PreviousSection);
        if (pressed.HasFlag(GamepadButtons.RightShoulder)) Run(PadAction.NextSection);
        if (pressed.HasFlag(GamepadButtons.Y)) Run(PadAction.Search);
        if (pressed.HasFlag(GamepadButtons.View)) Run(PadAction.ToggleTvMode);

        // Direction: d-pad or left stick, with key-like repeat while held.
        PadAction? direction =
            reading.Buttons.HasFlag(GamepadButtons.DPadUp) || reading.LeftThumbstickY > 0.6 ? PadAction.Up
            : reading.Buttons.HasFlag(GamepadButtons.DPadDown) || reading.LeftThumbstickY < -0.6 ? PadAction.Down
            : reading.Buttons.HasFlag(GamepadButtons.DPadLeft) || reading.LeftThumbstickX < -0.6 ? PadAction.Left
            : reading.Buttons.HasFlag(GamepadButtons.DPadRight) || reading.LeftThumbstickX > 0.6 ? PadAction.Right
            : null;
        var now = DateTime.UtcNow;
        if (direction is null) { held = null; return; }
        if (direction != held)
        {
            held = direction;
            heldSince = lastRepeat = now;
            Run(direction.Value);
        }
        else if (now - heldSince > FirstRepeat && now - lastRepeat > NextRepeat)
        {
            lastRepeat = now;
            Run(direction.Value);
        }
    }

    public void Run(PadAction action)
    {
        var xamlRoot = root();
        switch (action)
        {
            case PadAction.Up or PadAction.Down or PadAction.Left or PadAction.Right:
                if (xamlRoot is null) return;
                var options = new FindNextElementOptions { SearchRoot = xamlRoot.Content };
                var direction = action switch
                {
                    PadAction.Up => FocusNavigationDirection.Up,
                    PadAction.Down => FocusNavigationDirection.Down,
                    PadAction.Left => FocusNavigationDirection.Left,
                    _ => FocusNavigationDirection.Right,
                };
                // First press with nothing focused: start on the first focusable element.
                if (FocusManager.GetFocusedElement(xamlRoot) is null)
                {
                    if (FocusManager.FindFirstFocusableElement(xamlRoot.Content) is Control first) first.Focus(FocusState.Keyboard);
                    return;
                }
                FocusManager.TryMoveFocus(direction, options);
                break;
            case PadAction.Activate:
                if (xamlRoot is not null) Activate(FocusManager.GetFocusedElement(xamlRoot));
                break;
            default:
                Command?.Invoke(action);
                break;
        }
    }

    /// <summary>What Enter would do on the focused element: press a button, flip a switch, open a combo box, pick an item.</summary>
    private static void Activate(object? focused)
    {
        switch (focused)
        {
            case ToggleSwitch toggle:
                toggle.IsOn = !toggle.IsOn;
                break;
            case ComboBox combo:
                combo.IsDropDownOpen = !combo.IsDropDownOpen;
                break;
            case UIElement element when element is ButtonBase or SelectorItem:
                var peer = FrameworkElementAutomationPeer.FromElement(element) ?? FrameworkElementAutomationPeer.CreatePeerForElement(element);
                if (peer is null) break;
                if (peer.GetPattern(PatternInterface.Invoke) is IInvokeProvider invoke) invoke.Invoke();
                else if (peer.GetPattern(PatternInterface.Toggle) is IToggleProvider toggleable) toggleable.Toggle();
                else if (peer.GetPattern(PatternInterface.SelectionItem) is ISelectionItemProvider item) item.Select();
                break;
        }
    }
}
