// <copyright file="PlayerInfoPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.World;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The default implementation of the <see cref="IPlayerInfoPlugIn"/> which forwards
/// the target's in-scope appearance (name, hero state, equipped items, position, effects)
/// to the observing player's game client by re-using the existing
/// <see cref="INewPlayersInScopePlugIn"/>. This guarantees that party members - e.g. the
/// party leader viewing a companion - always have a fresh, complete representation of the
/// target available to render a target-info / inspect panel.
/// </summary>
/// <remarks>
/// No additional server-to-client packet is introduced: the companion's appearance is
/// already broadcast when it enters the observer scope. This plug-in acts as a proactive
/// refresh so the data is re-sent on demand (e.g. when a member joins the party).
/// </remarks>
[PlugIn]
[Display(Name = "Player info view", Description = "Broadcasts a player's target info (appearance, gear, ...) to the observing player.")]
[Guid("E8F9A8B4-7C32-4F10-9D0A-6C91A522A9F5")]
[MinimumClient(5, 0, ClientLanguage.Invariant)]
public class PlayerInfoPlugIn : IPlayerInfoPlugIn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerInfoPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public PlayerInfoPlugIn(RemotePlayer player) => this.Player = player;

    /// <summary>
    /// Gets the player of this view.
    /// </summary>
    protected RemotePlayer Player { get; }

    /// <inheritdoc/>
    public async ValueTask ShowPlayerInfoAsync(Player target)
    {
        // Re-use the already-implemented, protocol-correct appearance broadcast instead of
        // duplicating the AddCharactersToScope packet logic.
        await this.Player.InvokeViewPlugInAsync<INewPlayersInScopePlugIn>(
            p => p.NewPlayersInScopeAsync(target.GetAsEnumerable(), true))
            .ConfigureAwait(false);
    }
}
