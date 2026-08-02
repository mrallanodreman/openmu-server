// <copyright file="SummonCompanionChatCommandPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.Bots;
using MUnique.OpenMU.PlugIns;

/// <summary>Chat command: /companion CharacterName and /companion off.</summary>
[Guid("D6C41A3E-8D6C-4CF9-B4C0-0C5D9A8B10E2")]
[PlugIn]
[Display(Name = "Companion", Description = "Summons a second character from the same account as a party companion.")]
[ChatCommandHelp("/companion", CharacterStatus.Normal)]
public sealed class SummonCompanionChatCommandPlugIn : IChatCommandPlugIn
{
    public string Key => "/companion";

    public CharacterStatus MinCharacterStatusRequirement => CharacterStatus.Normal;

    public async ValueTask HandleCommandAsync(Player player, string command)
    {
        var argument = command["/companion".Length..].Trim();
        var manager = player.GameContext.CompanionManager;
        var success = string.Equals(argument, "off", StringComparison.OrdinalIgnoreCase)
            ? await manager.DismissAsync(player).ConfigureAwait(false)
            : !string.IsNullOrWhiteSpace(argument)
                && await manager.SummonAsync(player, argument).ConfigureAwait(false);

        await player.ShowBlueMessageAsync(success
            ? string.Equals(argument, "off", StringComparison.OrdinalIgnoreCase) ? "Companion dismissed." : $"Companion {argument} summoned."
            : "Companion command failed.").ConfigureAwait(false);
    }
}
