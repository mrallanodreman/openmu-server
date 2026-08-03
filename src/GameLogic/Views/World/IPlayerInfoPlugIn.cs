// <copyright file="IPlayerInfoPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.World;

/// <summary>
/// Interface of a view whose implementation shows the inspect/target info
/// (appearance, equipped items, ...) of another player to the observing player.
/// </summary>
public interface IPlayerInfoPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the player info of <paramref name="target"/> to the observing player.
    /// </summary>
    /// <param name="target">The player whose info should be shown.</param>
    ValueTask ShowPlayerInfoAsync(Player target);
}
