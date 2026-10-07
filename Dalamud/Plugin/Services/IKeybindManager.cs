using System.Collections.Generic;
using System.Collections.ObjectModel;

using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Keybind;

namespace Dalamud.Plugin.Services;

/// <summary>
/// A service for managing keybinds.
/// </summary>
public interface IKeybindManager : IDalamudService
{
    /// <summary>
    /// Event that is dispatched when a keybind is added.
    /// </summary>
    event EventHandler<KeybindEventArgs>? KeybindAdded;

    /// <summary>
    /// Event that is dispatched when a keybind is updated.
    /// </summary>
    event EventHandler<KeybindUpdatedEventArgs>? KeybindUpdated;

    /// <summary>
    /// Event that is dispatched when a keybind is removed.
    /// </summary>
    event EventHandler<KeybindEventArgs>? KeybindRemoved;

    /// <summary>
    /// Event that is dispatched when a keybind conflict occurs.
    /// </summary>
    event EventHandler<KeybindConflictEventArgs>? KeybindConflict;

    /// <summary>
    /// Event that is dispatched when a keybind is overridden by another plugin.
    /// </summary>
    event EventHandler<KeybindOverriddenEventArgs>? KeybindOverridden;

    /// <summary>
    /// Gets a read-only list of all registered keybinds.
    /// </summary>
    ReadOnlyDictionary<string, IReadOnlyKeybindInfo> Keybinds { get; }

    /// <summary>
    /// Processes a keybind event.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <param name="modifiers">The modifier keys that were pressed.</param>
    /// <returns>True if the keybind was found and dispatched.</returns>
    bool ProcessKeybind(VirtualKey key, ModifierFlags modifiers);

    /// <summary>
    /// Dispatches the handling of a keybind event.
    /// </summary>
    /// <param name="id">The ID of the keybind to dispatch.</param>
    /// <param name="key">The key that was pressed.</param>
    /// <param name="modifiers">The modifier keys that were pressed.</param>
    void DispatchKeybind(string id, VirtualKey key, ModifierFlags modifiers);

    /// <summary>
    /// Add a keybind handler, which you can use to add your own keybinds to the game.
    /// </summary>
    /// <param name="id">The ID of the keybind to register.</param>
    /// <param name="info">A <see cref="KeybindInfo"/> object describing the keybind.</param>
    /// <returns>True if the keybind was successfully added.</returns>
    bool AddHandler(string id, KeybindInfo info);

    /// <summary>
    /// Update an existing keybind handler with a new key and modifiers.
    /// </summary>
    /// <param name="id">The ID of the keybind to update.</param>
    /// <param name="newKey">The new key to bind.</param>
    /// <param name="newModifiers">The new modifier keys to bind.</param>
    /// <returns>True if the keybind was successfully updated.</returns>
    bool UpdateBinding(string id, VirtualKey newKey, ModifierFlags newModifiers);

    /// <summary>
    /// Remove a keybind from the keybind handlers.
    /// </summary>
    /// <param name="id">The ID of the keybind to remove.</param>
    /// <returns>True if the keybind was successfully removed.</returns>
    bool RemoveHandler(string id);

    /// <summary>
    /// Override an existing keybind with a new key and modifiers.
    /// </summary>
    /// <param name="id">The ID of the keybind.</param>
    /// <param name="info">The keybind information.</param>
    /// <returns>True if the keybind was successfully overridden.</returns>
    bool OverrideBinding(string id, KeybindInfo info);

    /// <summary>
    /// Get the assembly name of the plugin that registered a given keybind.
    /// </summary>
    /// <param name="id">The ID of the keybind.</param>
    /// <param name="info">The keybind information.</param>
    /// <returns>The assembly name of the plugin that registered the keybind or null if not found.</returns>
    string GetHandlerAssemblyName(string id, IReadOnlyKeybindInfo info);

    /// <summary>
    /// Get all keybind handlers registered by a given assembly name.
    /// </summary>
    /// <param name="assemblyName">The name of the plugin.</param>
    /// <returns>A list of keybind handlers registered by the specified assembly.</returns>
    IReadOnlyList<(string Id, IReadOnlyKeybindInfo Info)> GetHandlersByAssemblyName(string assemblyName);
}
