using System;
using System.Collections.Generic;
using System.Text;

using Dalamud.Game.ClientState.Keys;

namespace Dalamud.Game.Keybind
{
    /// <summary>
    /// Represents the data for a saved keybind.
    /// </summary>
    internal class SavedKeybindData
    {
        /// <summary>
        /// Gets or sets the virtual key of the saved keybind.
        /// </summary>
        public VirtualKey Key { get; set; }

        /// <summary>
        /// Gets or sets the modifier keys of the saved keybind.
        /// </summary>
        public ModifierFlags Modifiers { get; set; }
    }
}
