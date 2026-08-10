// <copyright file="AreaSkillAttackHandlerGlobalPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.Skills;
using MUnique.OpenMU.Network.Packets.ClientToServer;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handler for area skill attack packets in a global coordinate world.
/// </summary>
[PlugIn]
[Guid("2a71cdb6-5d39-4e8f-b1c2-d47e8f0a3f11")]
[MinimumClient(106, 3, ClientLanguage.Invariant)]
internal class AreaSkillAttackHandlerGlobalPlugIn : IPacketHandlerPlugIn
{
    private const int PollutionSkillId = 225;

    private readonly AreaSkillAttackAction _attackAction = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => true;

    /// <inheritdoc/>
    public byte Key => AreaSkillGlobal.Code;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        AreaSkillGlobal message = packet;
        if (player.SkillList is null || !player.SkillList.ContainsSkill(message.SkillId))
        {
            return;
        }

        if (player.SkillList.GetSkill(message.SkillId) is { Skill.SkillType: SkillType.AreaSkillExplicitHits })
        {
            // we don't need to return if it fails - it doesn't cause any damage, and the player
            // still "pays" the mana and ag.
            player.SkillHitValidator.TryRegisterAnimation(message.SkillId, message.AnimationCounter);
        }

        await this._attackAction.AttackAsync(player, message.ExtraTargetId, message.SkillId, new Point(message.TargetX, message.TargetY), message.Rotation).ConfigureAwait(false);

        if (message.SkillId == PollutionSkillId)
        {
            var point = new Point(message.TargetX, message.TargetY);
            var extraTargetId = message.ExtraTargetId;
            var rotation = message.Rotation;

            _ = Task.Run(async () =>
            {
                try
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        await Task.Delay(1000).ConfigureAwait(false);
                        await this._attackAction.AttackAsync(player, extraTargetId, PollutionSkillId, point, rotation).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    player.Logger.LogError(ex, "Error during pollution skill execution.");
                }
            });
        }
    }
}
