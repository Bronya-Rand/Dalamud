using System;
using System.Collections.Generic;
using System.Text;

using Dalamud.Configuration.Internal;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Keybind;

namespace Dalamud.Interface.Internal
{
    [ServiceManager.EarlyLoadedService]

    internal class DalamudKeybinds : IServiceType
    {
        [ServiceManager.ServiceConstructor]
        private DalamudKeybinds(KeybindManager keybindManager)
        {
            keybindManager.AddHandler("XLPlugins", new KeybindInfo(this.OnOpenInstallerCommand)
            {
                Key = VirtualKey.F1,
                Modifiers = ModifierFlags.None,
                HelpMessage = "Opens the Plugin Installer window.",
                DisplayName = "Plugin Installer",
                DefaultKey = VirtualKey.F1,
                DefaultModifiers = ModifierFlags.None,
            });
        }

        private void OnOpenInstallerCommand(string id, VirtualKey key, ModifierFlags modifiers)
        {
            var configuration = Service<DalamudConfiguration>.Get();
            Service<DalamudInterface>.Get().TogglePluginInstallerWindowTo(configuration.PluginInstallerOpen);
        }
    }
}
