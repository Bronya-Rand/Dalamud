using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

using Dalamud.Game.ClientState.Keys;
using Dalamud.IoC;
using Dalamud.IoC.Internal;
using Dalamud.Logging.Internal;

using Dalamud.Plugin.Internal.Types;

using Dalamud.Plugin.Services;

namespace Dalamud.Game.Keybind;

/// <summary>
/// Plugin-scoped version of the <see cref="KeybindManager"/> service.
/// </summary>
[PluginInterface]
[ServiceManager.ScopedService]
[ResolveVia<IKeybindManager>]
internal class KeybindManagerPluginScoped : IInternalDisposableService, IKeybindManager
{
    private static readonly ModuleLog Log = ModuleLog.Create<KeybindManager>();

    [ServiceManager.ServiceDependency]
    private readonly KeybindManager keybindManagerService = Service<KeybindManager>.Get();

    private readonly List<string> pluginRegisteredKeybinds = [];
    private readonly LocalPlugin pluginInfo;
    private readonly Lock keybindListLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="KeybindManagerPluginScoped"/> class.
    /// </summary>
    /// <param name="pluginInfo">Info for the plugin that requests this service.</param>
    public KeybindManagerPluginScoped(LocalPlugin pluginInfo)
    {
        this.pluginInfo = pluginInfo;

        this.keybindManagerService.KeybindAdded += this.OnRootKeybindAdded;
        this.keybindManagerService.KeybindUpdated += this.OnRootKeybindUpdated;
        this.keybindManagerService.KeybindRemoved += this.OnRootKeybindRemoved;

        this.keybindManagerService.KeybindConflict += this.OnRootKeybindConflict;
        this.keybindManagerService.KeybindOverridden += this.OnRootKeybindOverriden;
    }

    /// <inheritdoc />
    public event EventHandler<KeybindEventArgs>? KeybindAdded;

    /// <inheritdoc />
    public event EventHandler<KeybindUpdatedEventArgs>? KeybindUpdated;

    /// <inheritdoc />
    public event EventHandler<KeybindEventArgs>? KeybindRemoved;

    /// <inheritdoc />
    public event EventHandler<KeybindConflictEventArgs>? KeybindConflict;

    /// <inheritdoc />
    public event EventHandler<KeybindOverriddenEventArgs>? KeybindOverridden;

    /// <inheritdoc/>
    public ReadOnlyDictionary<string, IReadOnlyKeybindInfo> Keybinds =>
        this.keybindManagerService.Keybinds;

    /// <inheritdoc/>
    void IInternalDisposableService.DisposeService()
    {
        using var scope = this.keybindListLock.EnterScope();

        this.keybindManagerService.KeybindAdded -= this.OnRootKeybindAdded;
        this.keybindManagerService.KeybindUpdated -= this.OnRootKeybindUpdated;
        this.keybindManagerService.KeybindRemoved -= this.OnRootKeybindRemoved;
        this.keybindManagerService.KeybindConflict -= this.OnRootKeybindConflict;
        this.keybindManagerService.KeybindOverridden -= this.OnRootKeybindOverriden;

        foreach (var keybind in this.pluginRegisteredKeybinds)
            this.keybindManagerService.RemoveHandler(keybind);

        this.pluginRegisteredKeybinds.Clear();
    }

    /// <inheritdoc/>
    public bool ProcessKeybind(VirtualKey key, ModifierFlags modifiers) =>
        this.keybindManagerService.ProcessKeybind(key, modifiers);

    /// <inheritdoc/>
    public void DispatchKeybind(string id, VirtualKey key, ModifierFlags modifiers) =>
        this.keybindManagerService.DispatchKeybind(id, key, modifiers);

    /// <inheritdoc/>
    public bool AddHandler(string id, KeybindInfo info)
    {
        using var scope = this.keybindListLock.EnterScope();
        var normalizedId = this.NormalizeId(id);

        // Set Id to Display Name if Empty
        if (string.IsNullOrEmpty(info.DisplayName))
            info.DisplayName = id;

        if (!this.pluginRegisteredKeybinds.Contains(normalizedId))
        {
            if (this.keybindManagerService.AddHandler(normalizedId, info, this.pluginInfo.InternalName))
            {
                this.pluginRegisteredKeybinds.Add(normalizedId);
                return true;
            }
        }
        else
        {
            Log.Error($"Keybind with ID '{normalizedId}' is already registered for plugin '{this.pluginInfo.InternalName}'.");
        }

        return false;
    }

    /// <inheritdoc />
    public bool UpdateBinding(string id, VirtualKey newKey, ModifierFlags newModifiers)
    {
        var targetId = this.keybindManagerService.Keybinds.ContainsKey(id)
            ? id
            : this.NormalizeId(id);

        return this.keybindManagerService.UpdateBinding(targetId, newKey, newModifiers);
    }

    /// <inheritdoc/>
    public bool RemoveHandler(string id)
    {
        using var scope = this.keybindListLock.EnterScope();
        var targetId = this.pluginRegisteredKeybinds.Contains(id)
            ? id
            : this.NormalizeId(id);

        if (this.pluginRegisteredKeybinds.Contains(targetId))
        {
            if (this.keybindManagerService.RemoveHandler(targetId))
            {
                this.pluginRegisteredKeybinds.Remove(targetId);
                return true;
            }

            Log.Error($"Failed to remove keybind with ID '{targetId}' for plugin '{this.pluginInfo.InternalName}'.");
        }
        else
        {
            Log.Error($"Keybind with ID '{targetId}' was not found for plugin '{this.pluginInfo.InternalName}'.");
        }

        return false;
    }

    /// <inheritdoc/>
    public bool OverrideBinding(string id, KeybindInfo info)
    {
        using var scope = this.keybindListLock.EnterScope();
        var normalizedId = this.NormalizeId(id);

        if (string.IsNullOrEmpty(info.DisplayName))
            info.DisplayName = id;

        if (!this.pluginRegisteredKeybinds.Contains(normalizedId))
        {
            if (this.keybindManagerService.OverrideBinding(normalizedId, info, this.pluginInfo.InternalName))
            {
                this.pluginRegisteredKeybinds.Add(normalizedId);
                return true;
            }

            Log.Error($"Failed to override keybind with ID '{normalizedId}' for plugin '{this.pluginInfo.InternalName}'.");
        }
        else
        {
            Log.Error($"Keybind with ID '{normalizedId}' is already registered for plugin '{this.pluginInfo.InternalName}'.");
        }

        return false;
    }

    /// <inheritdoc/>
    public string GetHandlerAssemblyName(string id, IReadOnlyKeybindInfo info) =>
        this.keybindManagerService.GetHandlerAssemblyName(id, info);

    /// <inheritdoc/>
    public IReadOnlyList<(string Id, IReadOnlyKeybindInfo Info)> GetHandlersByAssemblyName(string assemblyName)
        => this.keybindManagerService.GetHandlersByAssemblyName(assemblyName);

    private void OnRootKeybindAdded(object? sender, KeybindEventArgs e) =>
        this.KeybindAdded?.Invoke(sender, e);

    private void OnRootKeybindUpdated(object? sender, KeybindUpdatedEventArgs e) =>
        this.KeybindUpdated?.Invoke(sender, e);

    private void OnRootKeybindRemoved(object? sender, KeybindEventArgs e) =>
        this.KeybindRemoved?.Invoke(sender, e);

    private void OnRootKeybindConflict(object? sender, KeybindConflictEventArgs e)
    {
        if (e.AttemptedPlugin == this.pluginInfo.InternalName)
            this.KeybindConflict?.Invoke(sender, e);
    }

    private void OnRootKeybindOverriden(object? sender, KeybindOverriddenEventArgs e)
    {
        if (e.OverriddenPlugin == this.pluginInfo.InternalName)
            this.KeybindOverridden?.Invoke(sender, e);
    }

    private string NormalizeId(string id)
    {
        var prefix = $"{this.pluginInfo.InternalName}.";
        return id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? id : $"{prefix}{id}";
    }
}
