using Dalamud.Game.ClientState.Keys;

namespace Dalamud.Game.Keybind
{
    /// <summary>
    /// Event arguments for keybind events.
    /// </summary>
    public class KeybindEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the ID of the keybind that was added or removed.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Gets the keybind information for the keybind that was added or removed.
        /// </summary>
        public required IReadOnlyKeybindInfo KeybindInfo { get; init; }
    }

    /// <summary>
    /// Event arguments for when a keybind conflict occurs.
    /// </summary>
    public class KeybindConflictEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the virtual key of the conflicting keybind.
        /// </summary>
        public required VirtualKey ConflictingKey { get; init; }

        /// <summary>
        /// Gets the modifier keys of the conflicting keybind.
        /// </summary>
        public required ModifierFlags ConflictingModifiers { get; init; }

        /// <summary>
        /// Gets the ID of the keybind attempting to register/update.
        /// </summary>
        public required string AttemptedId { get; init; }

        /// <summary>
        /// Gets the <see cref="IReadOnlyKeybindInfo"/> of the keybind attempting to register/update.
        /// </summary>
        public required IReadOnlyKeybindInfo AttemptedInfo { get; init; }

        /// <summary>
        /// Gets the plugin name of the keybind attempting to register/update.
        /// </summary>
        public required string AttemptedPlugin { get; init; }

        /// <summary>
        /// Gets the ID of the existing keybind that holds this key/modifier combination.
        /// </summary>
        public required string ExistingId { get; init; }

        /// <summary>
        /// Gets the <see cref="IReadOnlyKeybindInfo"/> of the existing keybind that holds this key/modifier combination.
        /// </summary>
        public required IReadOnlyKeybindInfo ExistingInfo { get; init; }

        /// <summary>
        /// Gets the plugin name of the existing keybind that holds this key/modifier combination.
        /// </summary>
        public required string ExistingPlugin { get; init; }
    }

    /// <summary>
    /// Event arguments for when a keybind is overridden by another plugin.
    /// </summary>
    public class KeybindOverriddenEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the ID of the keybind that was overridden.
        /// </summary>
        public required string OverriddenId { get; init; }

        /// <summary>
        /// Gets the <see cref="IReadOnlyKeybindInfo"/> of the keybind that was overridden.
        /// </summary>
        public required IReadOnlyKeybindInfo OverriddenInfo { get; init; }

        /// <summary>
        /// Gets the plugin name of the keybind that was overridden.
        /// </summary>
        public required string OverriddenPlugin { get; init; }

        /// <summary>
        /// Gets the virtual key of the keybind that was overridden.
        /// </summary>
        public required VirtualKey LostKey { get; init; }

        /// <summary>
        /// Gets the modifier keys of the keybind that was overridden.
        /// </summary>
        public required ModifierFlags LostModifiers { get; init; }

        /// <summary>
        /// Gets the ID of the keybind that is overriding the existing keybind.
        /// </summary>
        public required string OverridingId { get; init; }

        /// <summary>
        /// Gets the <see cref="IReadOnlyKeybindInfo"/> of the keybind that is overriding the existing keybind.
        /// </summary>
        public required string OverridingPlugin { get; init; }
    }

    /// <summary>
    /// Event arguments for when a keybind is updated.
    /// </summary>
    public class KeybindUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the ID of the keybind that was updated.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Gets the <see cref="IReadOnlyKeybindInfo"/> of the keybind that was updated.
        /// </summary>
        public required IReadOnlyKeybindInfo Info { get; init; }

        /// <summary>
        /// Gets the old virtual key of the keybind before the update.
        /// </summary>
        public required VirtualKey OldKey { get; init; }

        /// <summary>
        /// Gets the old modifier keys of the keybind before the update.
        /// </summary>
        public required ModifierFlags OldModifiers { get; init; }

        /// <summary>
        /// Gets the new virtual key of the keybind after the update.
        /// </summary>
        public required VirtualKey NewKey { get; init; }

        /// <summary>
        /// Gets the new modifier keys of the keybind after the update.
        /// </summary>
        public required ModifierFlags NewModifiers { get; init; }
    }
}
