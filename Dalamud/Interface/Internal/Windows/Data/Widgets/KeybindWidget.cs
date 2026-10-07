using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.Command;
using Dalamud.Game.Keybind;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;

namespace Dalamud.Interface.Internal.Windows.Data.Widgets
{
    /// <summary>
    /// Widget for displaying keybind info.
    /// </summary>
    internal class KeybindWidget : IDataWindowWidget
    {
        private const ImGuiTableFlags TableFlags = ImGuiTableFlags.ScrollY | ImGuiTableFlags.Borders |
                                           ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.Sortable |
                                           ImGuiTableFlags.SortTristate;

        /// <inheritdoc/>
        public string[]? CommandShortcuts { get; init; } = ["keybinds"];

        /// <inheritdoc/>
        public string DisplayName { get; init; } = "Keybinds";

        /// <inheritdoc/>
        public bool Ready { get; set; }

        /// <inheritdoc/>
        public void Load()
        {
            this.Ready = true;
        }

        /// <inheritdoc/>
        public void Draw()
        {
            var keybindManager = Service<KeybindManager>.Get();

            using var table = ImRaii.Table("KeybindsTable", 4, TableFlags);
            if (table)
            {
                ImGui.TableSetupScrollFreeze(0, 1);

                ImGui.TableSetupColumn("Keybind"u8);
                ImGui.TableSetupColumn("Plugin"u8);
                ImGui.TableSetupColumn("HelpMessage"u8, ImGuiTableColumnFlags.NoSort);
                ImGui.TableSetupColumn("In Help?"u8, ImGuiTableColumnFlags.NoSort);
                ImGui.TableHeadersRow();

                var sortSpecs = ImGui.TableGetSortSpecs();
                var keybinds = keybindManager.Keybinds.ToArray();

                if (sortSpecs.SpecsCount != 0)
                {
                    keybinds = sortSpecs.Specs.ColumnIndex switch
                    {
                        0 => sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending
                                 ? keybinds.OrderBy(kv => kv.Key).ToArray()
                                 : keybinds.OrderByDescending(kv => kv.Key).ToArray(),
                        1 => sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending
                                 ? keybinds.OrderBy(kv => keybindManager.GetHandlerAssemblyName(kv.Key, kv.Value)).ToArray()
                                 : keybinds.OrderByDescending(kv => keybindManager.GetHandlerAssemblyName(kv.Key, kv.Value)).ToArray(),
                        _ => keybinds,
                    };
                }

                foreach (var keybind in keybinds)
                {
                    var keyInfo = keybind.Value;

                    ImGui.TableNextRow();

                    // Keybind
                    ImGui.TableNextColumn();
                    ImGui.Text(keyInfo.ToDisplayString());

                    // Plugin Name
                    ImGui.TableNextColumn();
                    ImGui.Text(keybindManager.GetHandlerAssemblyName(keybind.Key, keyInfo));

                    // Help Message
                    ImGui.TableNextColumn();
                    ImGui.Text(keyInfo.HelpMessage);

                    // Show in Help
                    ImGui.TableNextColumn();
                    ImGui.Text(keyInfo.ShowInHelp ? "Yes" : "No");
                }
            }
        }
    }
}
