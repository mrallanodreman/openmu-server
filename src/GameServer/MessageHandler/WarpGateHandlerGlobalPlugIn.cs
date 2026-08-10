// <copyright file="WarpGateHandlerGlobalPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler;

using Microsoft.Extensions.Logging;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions;
using MUnique.OpenMU.Network.Packets.ClientToServer;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handler for warp gate packets in a global coordinate world.
/// This one is called when a player has entered a gate area, and sends a gate enter request.
/// </summary>
[PlugIn]
[MinimumClient(106, 3, ClientLanguage.Invariant)]
internal class WarpGateHandlerGlobalPlugIn : IPacketHandlerPlugIn
{
    private readonly WarpGateAction _warpAction = new();

    private readonly WizardTeleportAction _teleportAction = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => EnterGateRequestGlobal.Code;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < EnterGateRequestGlobal.Length)
        {
            return;
        }

        EnterGateRequestGlobal request = packet;
        var gateNumber = request.GateNumber;

        if (gateNumber == 0)
        {
            await this._teleportAction.TryTeleportWithSkillAsync(player, new Point(request.TeleportTargetX, request.TeleportTargetY)).ConfigureAwait(false);
            return;
        }

        var gate = player.SelectedCharacter?.CurrentMap?.EnterGates.FirstOrDefault(g => g.Number == gateNumber);
        if (gate is null)
        {
            player.Logger.LogWarning("Gate {0} not found in current map {1}", gateNumber, player.SelectedCharacter?.CurrentMap);
            return;
        }

        await this._warpAction.EnterGateAsync(player, gate).ConfigureAwait(false);
    }
}
