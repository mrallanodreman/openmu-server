// <copyright file="CompanionManager.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Bots;

using System.Collections.Concurrent;

/// <summary>
/// Owns same-account companion bots. Companions are server-side players, so the
/// existing AI, MuHelper, party, follow, combat and pickup code is reused.
/// </summary>
public sealed class CompanionManager
{
    private readonly ConcurrentDictionary<string, BotPlayer> _companions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets the active companion for an account, if any.</summary>
    public bool TryGet(string loginName, out BotPlayer? companion)
        => this._companions.TryGetValue(loginName, out companion);

    /// <summary>Summons another character of the same account and adds it to the leader's party.</summary>
    public async ValueTask<bool> SummonAsync(Player leader, string characterName)
    {
        var loginName = leader.Account?.LoginName;
        if (string.IsNullOrWhiteSpace(loginName)
            || leader.SelectedCharacter is null
            || leader.Party is { PartyList.Count: >= 5 }
            || string.Equals(leader.Name, characterName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (this._companions.ContainsKey(loginName))
        {
            return false;
        }

        var bot = new BotPlayer(leader.GameContext)
        {
            MuHelperSettings = new BotMuHelperSettings(),
        };

        try
        {
            var account = await bot.PersistenceContext.GetAccountByLoginNameAsync(loginName).ConfigureAwait(false);
            var character = account?.Characters.FirstOrDefault(c =>
                string.Equals(c.Name, characterName, StringComparison.OrdinalIgnoreCase));
            if (account is null || character is null)
            {
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
                this._companions.TryRemove(loginName, out _);
                await bot.DisposeAsync().ConfigureAwait(false);
                return false;
            }

            var party = leader.Party ?? leader.GameContext.PartyManager.CreateParty();
            if (leader.Party is null && !await party.AddAsync(leader).ConfigureAwait(false))
            {
                await this.DismissAsync(leader).ConfigureAwait(false);
                return false;
            }

            if (!await party.AddAsync(bot).ConfigureAwait(false))
            {
                await this.DismissAsync(leader).ConfigureAwait(false);
                return false;
            }

            // The bot initialized on its saved home map; respawn it 2 tiles in front of the leader
            // (on the leader's current map, at a walkable safezone point) so it renders next to the
            // leader right away - not at the map's far spawn gate and never on another map.
            if (leader.CurrentMap is { } leaderMap
                && leaderMap.Definition.GetSafezoneGate(leaderMap.Terrain) is { } safeZoneGate
                && leader.SafeZoneSpawnGate is not null)
            {
                var leaderPos = leader.Position;
                // 2 tiles ahead of the leader along the X axis; clamp to the 0..255 walk grid.
                var spawnX = (byte)Math.Clamp(leaderPos.X + 2, 0, 255);
                var spawnY = leaderPos.Y;
                if (!leaderMap.Terrain.WalkMap[spawnX, spawnY] || !leaderMap.Terrain.SafezoneMap[spawnX, spawnY])
                {
                    spawnX = leaderPos.X;
                    spawnY = (byte)Math.Clamp(leaderPos.Y + 2, 0, 255);
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

                await bot.RespawnAtAsync(companionGate).ConfigureAwait(false);
                await bot.ClientReadyAfterMapChangeAsync().ConfigureAwait(false);
            }

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
