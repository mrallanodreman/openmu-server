// <copyright file="CompanionManager.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Bots;

using System.Collections.Concurrent;
using System.Threading;
using MUnique.OpenMU.GameLogic.Views.Companion;

/// <summary>
/// Owns same-account companion bots. Companions are server-side players, so the
/// existing AI, MuHelper, party, follow, combat and pickup code is reused.
/// </summary>
public sealed class CompanionManager
{
    private static int _nextCompanionId;

    private readonly ConcurrentDictionary<string, BotPlayer> _companions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets the active companion for an account, if any.</summary>
    public bool TryGet(string loginName, out BotPlayer? companion)
        => this._companions.TryGetValue(loginName, out companion);

    /// <summary>Gets the active companion owned by the leader, if any.</summary>
    public bool TryGetForLeader(Player leader, out BotPlayer? companion)
    {
        var loginName = leader.Account?.LoginName;
        if (string.IsNullOrWhiteSpace(loginName))
        {
            companion = null;
            return false;
        }

        return this._companions.TryGetValue(loginName, out companion);
    }

    /// <summary>Summons another character of the same account and adds it to the leader's party.</summary>
    public async ValueTask<bool> SummonAsync(Player leader, string characterName)
    {
        var loginName = leader.Account?.LoginName;
        if (string.IsNullOrWhiteSpace(loginName)
            || leader.SelectedCharacter is null
            || leader.Party is { PartyList.Count: >= 5 }
            || string.Equals(leader.Name, characterName, StringComparison.OrdinalIgnoreCase))
        {
            leader.Logger.LogWarning(
                "Companion summon rejected for {Leader}: account={Account}, selected={Selected}, target={Target}, partySize={PartySize}.",
                leader.Name,
                loginName ?? "<none>",
                leader.SelectedCharacter?.Name ?? "<none>",
                characterName,
                leader.Party?.PartyList.Count ?? 0);
            return false;
        }

        if (this._companions.ContainsKey(loginName))
        {
            leader.Logger.LogWarning("Companion summon rejected for {Leader}: an active companion already exists for account {Account}.", leader.Name, loginName);
            return false;
        }

            var bot = new BotPlayer(leader.GameContext)
            {
                MuHelperSettings = new BotMuHelperSettings(),
                CompanionId = unchecked((uint)Interlocked.Increment(ref _nextCompanionId)),
            };

        try
        {
            var account = await bot.PersistenceContext.GetAccountByLoginNameAsync(loginName).ConfigureAwait(false);
            var character = account?.Characters.FirstOrDefault(c =>
                string.Equals(c.Name, characterName, StringComparison.OrdinalIgnoreCase));
            if (account is null || character is null)
            {
                leader.Logger.LogWarning(
                    "Companion summon rejected for {Leader}: character {Target} was not found in account {Account}.",
                    leader.Name,
                    characterName,
                    loginName);
                await bot.DisposeAsync().ConfigureAwait(false);
                return false;
            }

            if (!this._companions.TryAdd(loginName, bot))
            {
                await bot.DisposeAsync().ConfigureAwait(false);
                return false;
            }

            if (!await bot.InitializeAsync(loginName, character.Name).ConfigureAwait(false))
            {
                leader.Logger.LogWarning("Companion initialization failed for {Leader}: account={Account}, character={Target}.", leader.Name, loginName, character.Name);
                this._companions.TryRemove(loginName, out _);
                await bot.DisposeAsync().ConfigureAwait(false);
                return false;
            }

            var party = leader.Party ?? leader.GameContext.PartyManager.CreateParty();
            if (leader.Party is null && !await party.AddAsync(leader).ConfigureAwait(false))
            {
                leader.Logger.LogWarning("Companion summon failed while creating a party for {Leader}.", leader.Name);
                await this.DismissAsync(leader).ConfigureAwait(false);
                return false;
            }

            if (!await party.AddAsync(bot).ConfigureAwait(false))
            {
                leader.Logger.LogWarning("Companion summon failed while adding {Target} to {Leader}'s party.", character.Name, leader.Name);
                await this.DismissAsync(leader).ConfigureAwait(false);
                return false;
            }

            // The bot initialized on its saved home map; teleport it onto the leader's map right
            // now (so it is visible) and then re-place it exactly 2 tiles ahead of the leader.
            // - WarpToAsync brings the bot onto the leader's map (works for offline bots: the
            //   server-side AddPlayer broadcast is what the client actually renders, so no client
            //   F3 handshake is needed to make the bot appear).
            // - RespawnAtAsync on the SAME map then moves the bot to leader.Position+(2,0) and
            //   re-adds it to the map's observer scope, placing it a hard 2 tiles from the leader.
            if (leader.CurrentMap is { } leaderMap
                && leaderMap.SafeZoneSpawnGate is { } leaderSpawnGate)
            {
                await bot.WarpToAsync(leaderSpawnGate).ConfigureAwait(false);

                // Leader-relative spawn point, clamped to the walkable terrain of the leader's map.
                var leaderPos = leader.Position;
                var spawnX = (ushort)Math.Clamp(leaderPos.X + 2, 0, leaderMap.Terrain.Size - 1);
                var spawnY = leaderPos.Y;
                if (!leaderMap.Terrain.WalkMap[spawnX, spawnY])
                {
                    // X is blocked (cliff/wall): step in Y instead, keeping the 2-tile offset.
                    spawnX = leaderPos.X;
                    spawnY = (ushort)Math.Clamp(leaderPos.Y + 2, 0, leaderMap.Terrain.Size - 1);
                }

                var companionGate = new ExitGate
                {
                    Map = leaderMap.Definition,
                    X1 = spawnX,
                    X2 = spawnX,
                    Y1 = spawnY,
                    Y2 = spawnY,
                    Direction = leader.Rotation,
                    IsSpawnGate = false,
                };

                // Same map as the leader by now -> isRespawnOnSameMap=true path re-adds the bot
                // to the leader's map observer set without a client map-load ack.
                await bot.RespawnAtAsync(companionGate).ConfigureAwait(false);
            }

            await leader.InvokeViewPlugInAsync<ICompanionStatePlugIn>(
                view => view.SendCompanionStateAsync(bot)).ConfigureAwait(false);

            bot.Logger.LogInformation("Companion '{Companion}' summoned by '{Leader}'.", bot.Name, leader.Name);
            return true;
        }
        catch
        {
            this._companions.TryRemove(loginName, out _);
            await bot.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>Dismisses the same-account companion.</summary>
    public async ValueTask<bool> DismissAsync(Player leader)
    {
        var loginName = leader.Account?.LoginName;
        if (loginName is null || !this._companions.TryRemove(loginName, out var bot))
        {
            return false;
        }

        if (bot.Party is { } party)
        {
            await party.KickMySelfAsync(bot).ConfigureAwait(false);
        }

        await bot.StopAsync().ConfigureAwait(false);
        await bot.GameContext.RemovePlayerAsync(bot).ConfigureAwait(false);
        await bot.DisposeAsync().ConfigureAwait(false);
        return true;
    }
}
