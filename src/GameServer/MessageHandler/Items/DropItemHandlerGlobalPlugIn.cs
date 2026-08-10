// <copyright file="DropItemHandlerGlobalPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Items;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.Network.Packets.ClientToServer;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handler for drop item packets in a global coordinate world.
/// </summary>
[PlugIn]
[Guid("1e3d5b0f-8c4e-4c7a-bc21-9a5f1c9e7b0d")]
internal class DropItemHandlerGlobalPlugIn : IPacketHandlerPlugIn
{
    private readonly DropItemAction _dropAction = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => DropItemRequestGlobal.Code;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        DropItemRequestGlobal message = packet;
        await this._dropAction.DropItemAsync(player, message.ItemSlot, new Point(message.TargetX, message.TargetY)).ConfigureAwait(false);
    }
}
