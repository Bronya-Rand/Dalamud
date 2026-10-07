using System.Collections.Generic;

using Dalamud.Game.ClientState.Keys;

namespace Dalamud.Game.Keybind
{
    /// <summary>
    /// Provides extension methods for working with keybinds and modifier flags.
    /// </summary>
    public static class KeybindExtensions
    {
        /// <summary>
        /// Gets a user-friendly display string for the given modifier flags.
        /// </summary>
        /// <param name="modifiers">The modifier flags to convert.</param>
        /// <returns>A formatted modifier string or empty if no modifiers are set.</returns>
        public static string ToDisplayString(this ModifierFlags modifiers)
        {
            if (modifiers == ModifierFlags.None)
                return string.Empty;

            var parts = new List<string>();

            // Control
            if (modifiers.HasFlag(ModifierFlags.LeftControl)) parts.Add("LCtrl");
            else if (modifiers.HasFlag(ModifierFlags.RightControl)) parts.Add("RCtrl");
            else if (modifiers.HasFlag(ModifierFlags.Control)) parts.Add("Ctrl");

            // Shift
            if (modifiers.HasFlag(ModifierFlags.LeftShift)) parts.Add("LShift");
            else if (modifiers.HasFlag(ModifierFlags.RightShift)) parts.Add("RShift");
            else if (modifiers.HasFlag(ModifierFlags.Shift)) parts.Add("Shift");

            // Alt
            if (modifiers.HasFlag(ModifierFlags.LeftAlt)) parts.Add("LAlt");
            else if (modifiers.HasFlag(ModifierFlags.RightAlt)) parts.Add("RAlt");
            else if (modifiers.HasFlag(ModifierFlags.Alt)) parts.Add("Alt");

            return string.Join(" + ", parts);
        }

        /// <summary>
        /// Gets a user-friendly display string for the given keybind info.
        /// </summary>
        /// <param name="keybind">The keybind info to format.</param>
        /// <returns>A formatted string representing the keybind.</returns>
        public static string ToDisplayString(this IReadOnlyKeybindInfo keybind) =>
            Format(keybind.Key, keybind.Modifiers);

        /// <summary>
        /// Formats a key and its modifiers into a user-friendly string.
        /// </summary>
        /// <param name="key">The key to format.</param>
        /// <param name="modifiers">The modifiers to format.</param>
        /// <returns>A formatted string representing the key and its modifiers.</returns>
        public static string Format(VirtualKey key, ModifierFlags modifiers)
        {
            if (key == VirtualKey.NO_KEY)
                return "None";

            var modStr = modifiers.ToDisplayString();
            var keyStr = key.ToString();

            return string.IsNullOrEmpty(modStr) ? keyStr : $"{modStr} + {keyStr}";
        }
    }
}
