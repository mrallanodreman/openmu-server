// <copyright file="TeleportTargetHandlerGlobalPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions;
using MUnique.OpenMU.Network.Packets.ClientToServer;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handler for teleport target packets (teleport ally skill) in a global coordinate world.
/// </summary>
[PlugIn]
[MinimumClient(106, 3, ClientLanguage.Invariant)]
internal class TeleportTargetHandlerGlobalPlugIn : IPacketHandlerPlugIn
{
    private readonly WizardTeleportAction _teleportAction = new();

    /// <inheritdoc />
    public byte Key => TeleportTargetGlobal.Code;

    /// <inheritdoc />
    public bool IsEncryptionExpected => true;

    /// <inheritdoc />
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        TeleportTargetGlobal message = packet;

        await this._teleportAction.TryTeleportTargetWithSkillAsync(
            player,
            message.TargetId,
            new Point(message.TeleportTargetX, message.TeleportTargetY)).ConfigureAwait(false);
    }
}
