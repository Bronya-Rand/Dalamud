using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;

namespace Dalamud.Game.Keybind;

/// <summary>
/// Represents a keybind that can be registered with <see cref="IKeybindManager"/>.
/// </summary>
public interface IReadOnlyKeybindInfo
{
    /// <summary>
    /// The function to be executed when the keybind is dispatched.
    /// </summary>
    /// <param name="id">The ID of the keybind that was dispatched.</param>
    /// <param name="key">The virtual key that was pressed.</param>
    /// <param name="modifiers">The modifier keys that were pressed.</param>
    delegate void HandlerDelegate(string id, VirtualKey key, ModifierFlags modifiers);

    /// <summary>
    /// Gets a <see cref="HandlerDelegate"/> which will be called when the keybind is dispatched.
    /// </summary>
    HandlerDelegate Handler { get; }

    /// <summary>
    /// Gets the virtual key that will trigger this keybind.
    /// </summary>
    VirtualKey Key { get; }

    /// <summary>
    /// Gets the modifier keys that will trigger this keybind.
    /// </summary>
    ModifierFlags Modifiers { get; }

    /// <summary>
    /// Gets the help message for this keybind.
    /// </summary>
    string HelpMessage { get; }

    /// <summary>
    /// Gets a value indicating whether if this keybind should be shown in the help output.
    /// </summary>
    bool ShowInHelp { get; }

    /// <summary>
    /// Gets the display order of this keybind. Defaults to alphabetical ordering.
    /// </summary>
    int DisplayOrder { get; }

    /// <summary>
    /// Gets a value indicating whether this keybind should suppress game input when triggered.
    /// </summary>
    bool SuppressGameInput { get; }

    /// <summary>
    /// Gets the display name for this keybind.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the default virtual key assigned by the plugin for this keybind.
    /// </summary>
    VirtualKey DefaultKey { get; }

    /// <summary>
    /// Gets the default modifier keys assigned by the plugin for this keybind.
    /// </summary>
    ModifierFlags DefaultModifiers { get; }
}

/// <summary>
/// Represents a keybind that can be registered with <see cref="IKeybindManager"/>.
/// </summary>
public sealed class KeybindInfo : IReadOnlyKeybindInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeybindInfo"/> class.
    /// </summary>
    /// <param name="handler">The method to call when the keybind is triggered.</param>
    public KeybindInfo(IReadOnlyKeybindInfo.HandlerDelegate handler)
    {
        this.Handler = handler;
    }

    /// <inheritdoc/>
    public IReadOnlyKeybindInfo.HandlerDelegate Handler { get; }

    /// <inheritdoc/>
    public VirtualKey Key { get; internal set; } = VirtualKey.NO_KEY;

    /// <inheritdoc/>
    public ModifierFlags Modifiers { get; internal set; } = ModifierFlags.None;

    /// <inheritdoc/>
    public string HelpMessage { get; set; } = string.Empty;

    /// <inheritdoc/>
    public bool ShowInHelp { get; set; } = true;

    /// <inheritdoc/>
    public int DisplayOrder { get; set; } = -1;

    /// <inheritdoc/>
    public bool SuppressGameInput { get; set; } = true;

    /// <inheritdoc/>
    public string DisplayName { get; set; } = string.Empty;

    /// <inheritdoc/>
    public VirtualKey DefaultKey { get; internal set; } = VirtualKey.NO_KEY;

    /// <inheritdoc/>
    public ModifierFlags DefaultModifiers { get; internal set; } = ModifierFlags.None;
}
