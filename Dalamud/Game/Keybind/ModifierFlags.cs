using System.Collections.Generic;

namespace Dalamud.Game.Keybind;

/// <summary>
/// Represents the modifier keys that can be pressed in combination with a keybind.
/// </summary>
[Flags]
public enum ModifierFlags : ushort
{
    /// <summary>
    /// No modifier keys are pressed.
    /// </summary>
    None = 0,

    /// <summary>
    /// The Control key is pressed.
    /// </summary>
    Control = 1 << 0,

    /// <summary>
    /// The Shift key is pressed.
    /// </summary>
    Shift = 1 << 1,

    /// <summary>
    /// The Alt key is pressed.
    /// </summary>
    Alt = 1 << 2,

    // Directional modifiers are not supported by the game, but they are supported by Dalamud.

    /// <summary>
    /// The left Control key is pressed.
    /// </summary>
    LeftControl = 1 << 3,

    /// <summary>
    /// The right Control key is pressed.
    /// </summary>
    RightControl = 1 << 4,

    /// <summary>
    /// The left Shift key is pressed.
    /// </summary>
    LeftShift = 1 << 5,

    /// <summary>
    /// The right Shift key is pressed.
    /// </summary>
    RightShift = 1 << 6,

    /// <summary>
    /// The left Alt key is pressed.
    /// </summary>
    LeftAlt = 1 << 7,

    /// <summary>
    /// The right Alt key is pressed.
    /// </summary>
    RightAlt = 1 << 8,
}


