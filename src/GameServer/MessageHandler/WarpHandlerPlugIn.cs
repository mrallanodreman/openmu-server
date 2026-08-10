// <copyright file="WarpHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions;
using MUnique.OpenMU.GameLogic.Properties;
using MUnique.OpenMU.Network.Packets.ClientToServer;
using MUnique.OpenMU.PlugIns;
using PlugInResources = MUnique.OpenMU.GameServer.Properties.PlugInResources;

/// <summary>
/// Handler for warp request packets.
/// This one is called when a player uses the warp list.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.WarpHandlerPlugIn_Name), Description = nameof(PlugInResources.WarpHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("3d261a26-4357-4367-b999-703ea936f4e9")]
internal class WarpHandlerPlugIn : IPacketHandlerPlugIn
{
    private readonly WarpAction _warpAction = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => WarpCommandRequest.Code;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        WarpCommandRequest request = packet;
        ushort warpInfoIndex = request.WarpInfoIndex;
        player.Logger.LogWarning(
            "WARP DEBUG request character={Character} index={Index} map={Map} position=({X},{Y}) warpCount={WarpCount}",
            player.Name,
            warpInfoIndex,
            player.CurrentMap?.Definition.Number,
            player.Position.X,
            player.Position.Y,
            player.GameContext.Configuration.WarpList?.Count ?? 0);

        var warpList = player.GameContext.Configuration.WarpList;
        var warpInfo = warpList?.FirstOrDefault(info => info.Index == warpInfoIndex);

        // A stale persisted entry can map Arena (1) to Lorencia. Restore the
        // standard Arena target while keeping its configured requirements.
        if (warpInfo is { Index: 1, Gate.Map.Number: 0 }
            && await player.GameContext.GetMapAsync(6).ConfigureAwait(false) is { SafeZoneSpawnGate: { } arenaGate })
        {
            player.Logger.LogWarning("WARP DEBUG corrected stale index 1 target to Arena gate=({X1},{Y1})-({X2},{Y2})", arenaGate.X1, arenaGate.Y1, arenaGate.X2, arenaGate.Y2);
            warpInfo = new WarpInfo
            {
                Index = warpInfo.Index,
                Costs = warpInfo.Costs,
                LevelRequirement = warpInfo.LevelRequirement,
                Gate = arenaGate,
            };
        }

        // Persisted configurations may miss the standard Lorencia aliases 0/2.
        // Resolve both through the initialized map instead of returning UnknownWarpIndex.
        if (warpInfo is null && warpInfoIndex is 0 or 2)
        {
            var localMap = await player.GameContext.GetMapAsync(0).ConfigureAwait(false);
            var localGate = localMap?.SafeZoneSpawnGate
                            ?? localMap?.Definition.ExitGates?.FirstOrDefault(gate => !gate.IsSpawnGate)
                            ?? localMap?.Definition.ExitGates?.FirstOrDefault();
            if (localGate is not null)
            {
                player.Logger.LogWarning("WARP DEBUG resolved fallback index {Index} to Lorencia gate=({X1},{Y1})-({X2},{Y2})", warpInfoIndex, localGate.X1, localGate.Y1, localGate.X2, localGate.Y2);
                await this._warpAction.WarpToAsync(player, new WarpInfo
                {
                    Index = warpInfoIndex,
                    Costs = 2000,
                    LevelRequirement = 10,
                    Gate = localGate,
                }).ConfigureAwait(false);
                return;
            }

            player.Logger.LogError("WARP DEBUG fallback index {Index} could not resolve a Lorencia gate", warpInfoIndex);
        }

        if (warpInfo != null)
        {
            player.Logger.LogWarning("WARP DEBUG resolved index {Index} to gate={Gate}", warpInfoIndex, warpInfo.Gate);
            await this._warpAction.WarpToAsync(player, warpInfo).ConfigureAwait(false);
        }
        else
        {
            player.Logger.LogError("WARP DEBUG unknown index {Index}; configured indexes={Indexes}", warpInfoIndex, string.Join(",", warpList?.Select(info => info.Index) ?? []));
            await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.UnknownWarpIndex)).ConfigureAwait(false);
        }
    }
}
