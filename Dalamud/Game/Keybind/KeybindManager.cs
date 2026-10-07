using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Hooking.WndProcHook;
using Dalamud.Logging.Internal;
using Dalamud.Plugin.Services;
using Dalamud.Utility;

using TerraFX.Interop.Windows;

namespace Dalamud.Game.Keybind
{
    /// <summary>
    /// Represents a keybind and its associated information.
    /// </summary>
    /// <param name="KeybindId">The ID of the keybind.</param>
    /// <param name="Info">The keybind information.</param>
    internal record struct KeybindInfoKey(string KeybindId, IReadOnlyKeybindInfo Info);

    /// <summary>
    /// Represents a combination of a virtual key and modifier flags.
    /// </summary>
    /// <param name="Key">The virtual key.</param>
    /// <param name="Modifiers">The modifier flags.</param>
    internal record struct KeybindCombo(VirtualKey Key, ModifierFlags Modifiers);

    /// <summary>
    /// A service for managing keybinds.
    /// </summary>
    [ServiceManager.EarlyLoadedService]
    internal class KeybindManager : IInternalDisposableService, IKeybindManager
    {
        private static readonly ModuleLog Log = ModuleLog.Create<KeybindManager>();

        private readonly ConcurrentDictionary<string, KeybindInfo> keybindMap = new();
        private readonly ConcurrentDictionary<KeybindInfoKey, string> keybindAssemblyNameMap = new();
        private readonly ConcurrentDictionary<KeybindCombo, string> keybindsByCombo = new();

        private readonly WndProcHookManager wndProcHookManager;
        private readonly KeyState keyState;

        [ServiceManager.ServiceConstructor]
        private KeybindManager(WndProcHookManager wndProcHookManager, KeyState keyState)
        {
            this.wndProcHookManager = wndProcHookManager;
            this.keyState = keyState;
            this.wndProcHookManager.PreWndProc += this.OnPreWndProc;
        }

        /// <inheritdoc />
        public event EventHandler<KeybindEventArgs>? KeybindAdded;

        /// <inheritdoc />
        public event EventHandler<KeybindEventArgs>? KeybindRemoved;

        /// <inheritdoc />
        public event EventHandler<KeybindUpdatedEventArgs>? KeybindUpdated;

        /// <inheritdoc />
        public event EventHandler<KeybindConflictEventArgs>? KeybindConflict;

        /// <inheritdoc />
        public event EventHandler<KeybindOverriddenEventArgs>? KeybindOverridden;

        /// <inheritdoc />
        public ReadOnlyDictionary<string, IReadOnlyKeybindInfo> Keybinds =>
            new(this.keybindMap.ToDictionary(k => k.Key, v => (IReadOnlyKeybindInfo)v.Value));

        /// <summary>
        /// Adds a keybind handler to the manager.
        /// </summary>
        /// <param name="id">The ID of the keybind.</param>
        /// <param name="info">The keybind information.</param>
        /// <param name="pluginAssemblyName">The name of the plugin assembly.</param>
        /// <returns>true if the keybind was added successfully, false otherwise.</returns>
        public bool AddHandler(string id, KeybindInfo info, string pluginAssemblyName)
        {
            if (info.DisplayName.IsNullOrEmpty())
                info.DisplayName = id;

            // Store the default key and modifiers if none is set
            if (info.DefaultKey == VirtualKey.NO_KEY && info.DefaultModifiers == ModifierFlags.None)
            {
                info.DefaultKey = info.Key;
                info.DefaultModifiers = info.Modifiers;
            }

            id = FormatKeybindId(pluginAssemblyName, id);

            var combo = new KeybindCombo(info.Key, info.Modifiers);

            if (info.Key != VirtualKey.NO_KEY && this.keybindsByCombo.TryGetValue(combo, out var existingId))
            {
                var existingInfo = this.keybindMap[existingId];
                var existingPlugin = this.keybindAssemblyNameMap[new KeybindInfoKey(existingId, existingInfo)];

                Log.Warning($"Keybind {info.Key} with modifiers {info.Modifiers} is already registered by '{existingId}' from assembly '{existingPlugin}'.");

                this.KeybindConflict?.InvokeSafely(this, new KeybindConflictEventArgs
                {
                    ConflictingKey = info.Key,
                    ConflictingModifiers = info.Modifiers,
                    AttemptedId = id,
                    AttemptedInfo = info,
                    AttemptedPlugin = pluginAssemblyName,
                    ExistingId = existingId,
                    ExistingInfo = existingInfo,
                    ExistingPlugin = existingPlugin,
                });

                return false;
            }

            if (!this.keybindsByCombo.TryAdd(combo, id))
            {
                Log.Error($"Keybind {info.Key} with modifiers {info.Modifiers} is already registered by '{this.keybindsByCombo[combo]}'.");
                return false;
            }

            if (!this.keybindMap.TryAdd(id, info))
            {
                // Roll back the combo registration
                this.keybindsByCombo.TryRemove(combo, out _);
                Log.Error($"Keybind with ID '{id}' is already registered.");
                return false;
            }

            if (!this.keybindAssemblyNameMap.TryAdd(new KeybindInfoKey(id, info), pluginAssemblyName))
            {
                this.keybindMap.TryRemove(id, out _);
                this.keybindsByCombo.TryRemove(combo, out _);
                Log.Error($"Keybind with ID '{id}' is already registered for assembly '{pluginAssemblyName}'.");
                return false;
            }

            this.KeybindAdded?.InvokeSafely(this, new KeybindEventArgs
            {
                Id = id,
                KeybindInfo = info,
            });
            return true;
        }

        /// <inheritdoc/>
        public bool AddHandler(string id, KeybindInfo info) =>
            this.AddHandler(id, info, "Dalamud");

        /// <inheritdoc />
        public bool UpdateBinding(string id, VirtualKey newKey, ModifierFlags newModifiers)
        {
            var targetId = this.keybindMap.ContainsKey(id)
                ? id
                : FormatKeybindId("Dalamud", id);

            if (!this.keybindMap.TryGetValue(targetId, out var info))
            {
                Log.Error($"Keybind with ID '{targetId}' is not registered.");
                return false;
            }

            var oldCombo = new KeybindCombo(info.Key, info.Modifiers);
            var newCombo = new KeybindCombo(newKey, newModifiers);

            if (oldCombo.Equals(newCombo))
                return true; // No change needed

            if (!this.keybindsByCombo.TryAdd(newCombo, targetId))
            {
                Log.Error($"Keybind {newKey} with modifiers {newModifiers} is already registered by '{this.keybindsByCombo[newCombo]}'.");
                return false;
            }

            this.keybindsByCombo.TryRemove(oldCombo, out _);
            info.Key = newKey;
            info.Modifiers = newModifiers;

            this.KeybindUpdated?.InvokeSafely(this, new KeybindUpdatedEventArgs
            {
                Id = targetId,
                Info = info,
                OldKey = oldCombo.Key,
                OldModifiers = oldCombo.Modifiers,
                NewKey = newKey,
                NewModifiers = newModifiers,
            });
            return true;
        }

        /// <inheritdoc/>
        public bool RemoveHandler(string id)
        {
            var targetId = this.keybindMap.ContainsKey(id)
                ? id
                : FormatKeybindId("Dalamud", id);

            if (!this.keybindMap.TryRemove(targetId, out var info))
                return false;

            this.keybindAssemblyNameMap.TryRemove(new KeybindInfoKey(targetId, info), out _);
            this.keybindsByCombo.TryRemove(new KeybindCombo(info.Key, info.Modifiers), out _);

            this.KeybindRemoved?.InvokeSafely(this, new KeybindEventArgs { Id = targetId, KeybindInfo = info });
            return true;
        }

        /// <summary>
        /// Overrides an existing keybind with a new one, notifying the previous owner of the override.
        /// </summary>
        /// <param name="id">The ID of the keybind to override.</param>
        /// <param name="info">The new keybind information.</param>
        /// <param name="pluginAssemblyName">The name of the plugin that is overriding the keybind.</param>
        /// <returns>true if the keybind was successfully overridden; otherwise, false.</returns>
        public bool OverrideBinding(string id, KeybindInfo info, string pluginAssemblyName)
        {
            if (info.DisplayName.IsNullOrEmpty())
                info.DisplayName = id;

            id = FormatKeybindId(pluginAssemblyName, id);

            var combo = new KeybindCombo(info.Key, info.Modifiers);

            // Check if the keybind is already registered
            if (info.Key != VirtualKey.NO_KEY && this.keybindsByCombo.TryGetValue(combo, out var existingId))
            {
                if (this.keybindMap.TryGetValue(existingId, out var existingInfo))
                {
                    var existingPlugin = this.keybindAssemblyNameMap[new KeybindInfoKey(existingId, existingInfo)];

                    // Clear the existing keybind
                    existingInfo.Key = VirtualKey.NO_KEY;
                    existingInfo.Modifiers = ModifierFlags.None;

                    this.keybindsByCombo.TryRemove(combo, out _);

                    // Notify old keybind owner about the override
                    this.KeybindOverridden?.InvokeSafely(this, new KeybindOverriddenEventArgs
                    {
                        OverriddenId = existingId,
                        OverriddenInfo = existingInfo,
                        OverriddenPlugin = existingPlugin,
                        LostKey = combo.Key,
                        LostModifiers = combo.Modifiers,
                        OverridingId = id,
                        OverridingPlugin = pluginAssemblyName,
                    });
                }
            }

            // Register the new keybind
            return this.AddHandler(id, info, pluginAssemblyName);
        }

        /// <inheritdoc/>
        public bool OverrideBinding(string id, KeybindInfo info) =>
            this.OverrideBinding(id, info, "Dalamud");

        /// <inheritdoc/>
        public bool ProcessKeybind(VirtualKey key, ModifierFlags modifiers)
        {
            var matched = false;

            foreach (var (id, info) in this.keybindMap)
            {
                if (info.Key != key)
                    continue;

                if (!MatchesModifiers(info.Modifiers, modifiers))
                    continue;

                this.DispatchKeybind(id, key, modifiers);

                if (info.SuppressGameInput)
                    matched = true;
            }

            return matched;
        }

        /// <inheritdoc/>
        public void DispatchKeybind(string id, VirtualKey key, ModifierFlags modifiers)
        {
            if (!this.keybindMap.TryGetValue(id, out var info))
                return;

            try
            {
                info.Handler(id, key, modifiers);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error while dispatching keybind '{id}'");
            }
        }

        /// <inheritdoc/>
        public void DisposeService()
        {
            this.wndProcHookManager.PreWndProc -= this.OnPreWndProc;
            this.keybindMap.Clear();
            this.keybindAssemblyNameMap.Clear();
            this.keybindsByCombo.Clear();
        }

        /// <inheritdoc/>
        public string GetHandlerAssemblyName(string id, IReadOnlyKeybindInfo info)
        {
           if (this.keybindAssemblyNameMap.TryGetValue(new KeybindInfoKey(id, info), out var assemblyName))
                return assemblyName;

           return string.Empty;
        }

        /// <inheritdoc/>
        public IReadOnlyList<(string Id, IReadOnlyKeybindInfo Info)> GetHandlersByAssemblyName(string assemblyName)
        {
            return this.keybindAssemblyNameMap
                .Where(kvp => kvp.Value == assemblyName)
                .Select(kvp => (kvp.Key.KeybindId, kvp.Key.Info))
                .ToList();
        }

        /// <summary>
        /// Formats a keybind ID by prepending the assembly name if it is not already present.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to prepend.</param>
        /// <param name="id">The keybind ID to format.</param>
        /// <returns>The formatted keybind ID.</returns>
        internal static string FormatKeybindId(string assemblyName, string id)
        {
            var prefix = $"{assemblyName}.";
            return id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? id
                : $"{prefix}{id}";
        }

        private static bool MatchesCategory(
            ModifierFlags expected, ModifierFlags actual,
            ModifierFlags generic, ModifierFlags left, ModifierFlags right)
        {
            if (expected.HasFlag(left))
                return actual.HasFlag(left);
            if (expected.HasFlag(right))
                return actual.HasFlag(right);
            if (expected.HasFlag(generic))
                return actual.HasFlag(generic);
            return !actual.HasFlag(generic);
        }

        private static bool MatchesModifiers(ModifierFlags expected, ModifierFlags actual) =>
            MatchesCategory(expected, actual, ModifierFlags.Control, ModifierFlags.LeftControl, ModifierFlags.RightControl)
            && MatchesCategory(expected, actual, ModifierFlags.Shift, ModifierFlags.LeftShift, ModifierFlags.RightShift)
            && MatchesCategory(expected, actual, ModifierFlags.Alt, ModifierFlags.LeftAlt, ModifierFlags.RightAlt);

        private static bool IsStandaloneModifier(VirtualKey key) =>
            key is VirtualKey.CONTROL or VirtualKey.LCONTROL or VirtualKey.RCONTROL
                or VirtualKey.SHIFT or VirtualKey.LSHIFT or VirtualKey.RSHIFT
                or VirtualKey.MENU or VirtualKey.LMENU or VirtualKey.RMENU;

        private void OnPreWndProc(WndProcEventArgs args)
        {
            switch (args.Message)
            {
                case WM.WM_KEYDOWN:
                case WM.WM_SYSKEYDOWN:
                {
                    // Ignore repeated keystrokes when holding down a key
                    var isRepeat = (args.LParam & (1 << 30)) != 0;
                    if (isRepeat)
                        break;

                    var vk = (VirtualKey)(int)args.WParam;

                    // Ignore standalone modifiers as primary key
                    if (IsStandaloneModifier(vk))
                        break;

                    // Don't trigger keybinds if in ImGui text field
                    if (ImGui.GetIO().WantCaptureKeyboard)
                        break;

                    var modifiers = this.GetModifierFlags();

                    // If matching keybind requested to suppress game input, suppress it
                    if (this.ProcessKeybind(vk, modifiers))
                        args.SuppressWithValue(0);

                    break;
                }
            }
        }

        private ModifierFlags GetModifierFlags()
        {
            var flags = ModifierFlags.None;

            // Control
            var isLCtrl = this.keyState[VirtualKey.LCONTROL];
            var isRCtrl = this.keyState[VirtualKey.RCONTROL];
            if (isLCtrl) flags |= ModifierFlags.LeftControl;
            if (isRCtrl) flags |= ModifierFlags.RightControl;
            if (isLCtrl || isRCtrl || this.keyState[VirtualKey.CONTROL]) flags |= ModifierFlags.Control;

            // Shift
            var isLShift = this.keyState[VirtualKey.LSHIFT];
            var isRShift = this.keyState[VirtualKey.RSHIFT];
            if (isLShift) flags |= ModifierFlags.LeftShift;
            if (isRShift) flags |= ModifierFlags.RightShift;
            if (isLShift || isRShift || this.keyState[VirtualKey.SHIFT]) flags |= ModifierFlags.Shift;

            // Alt
            var isLAlt = this.keyState[VirtualKey.LMENU];
            var isRAlt = this.keyState[VirtualKey.RMENU];
            if (isLAlt) flags |= ModifierFlags.LeftAlt;
            if (isRAlt) flags |= ModifierFlags.RightAlt;
            if (isLAlt || isRAlt || this.keyState[VirtualKey.MENU]) flags |= ModifierFlags.Alt;

            return flags;
        }
    }
}
