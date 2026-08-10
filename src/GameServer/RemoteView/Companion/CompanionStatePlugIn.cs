namespace MUnique.OpenMU.GameServer.RemoteView.Companion;

using MUnique.OpenMU.GameLogic.Bots;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Views.Companion;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.Network.Packets.ServerToClient;
using MUnique.OpenMU.PlugIns;

/// <summary>Sends the authoritative companion snapshot to its owning remote player.</summary>
[PlugIn]
public sealed class CompanionStatePlugIn : ICompanionStatePlugIn
{
    private readonly RemotePlayer _player;

    public CompanionStatePlugIn(RemotePlayer player) => this._player = player;

    public async ValueTask SendCompanionStateAsync(BotPlayer companion)
    {
        var character = companion.SelectedCharacter;
        var attributes = companion.Attributes;
        var connection = this._player.Connection;
        if (connection is null || character is null || attributes is null)
        {
            return;
        }

        var helperConfiguration = new byte[257];
        if (character.MuHelperConfiguration is { } savedConfiguration)
        {
            savedConfiguration.AsSpan(0, Math.Min(savedConfiguration.Length, helperConfiguration.Length))
                .CopyTo(helperConfiguration);
        }

        await connection.SendCompanionStateAsync(
            companion.CompanionId,
            companion.CompanionRevision,
            character.Name,
            (byte)(character.CharacterClass?.Number ?? 0),
            (ushort)attributes[Stats.Level],
            0,
            (uint)Math.Max(0, character.LevelUpPoints),
            (uint)attributes[Stats.BaseStrength],
            (uint)attributes[Stats.BaseAgility],
            (uint)attributes[Stats.BaseVitality],
            (uint)attributes[Stats.BaseEnergy],
            (byte)(companion.IsAlive ? 1 : 0),
            (uint)Math.Max(0, attributes[Stats.CurrentHealth]),
            (uint)Math.Max(0, attributes[Stats.MaximumHealth]),
            (uint)Math.Max(0, attributes[Stats.CurrentMana]),
            (uint)Math.Max(0, attributes[Stats.MaximumMana]),
            (uint)Math.Max(0, attributes[Stats.CurrentShield]),
            (uint)Math.Max(0, attributes[Stats.MaximumShield]),
            (ushort)helperConfiguration.Length,
            helperConfiguration,
            new byte[27],
            new byte[27])
            .ConfigureAwait(false);

        var items = (companion.Inventory?.Items ?? character.Inventory?.Items ?? Enumerable.Empty<MUnique.OpenMU.DataModel.Entities.Item>())
            .Where(item => item.Definition is not null)
            .OrderBy(item => item.ItemSlot)
            .ToList();

        int WriteInventory()
        {
            var serializer = this._player.ItemSerializer;
            var itemLength = StoredItemRef.GetRequiredSize(serializer.NeededSpace);
            var size = CompanionInventoryRef.GetRequiredSize(items.Count, itemLength);
            var span = connection.Output.GetSpan(size)[..size];
            var packet = new CompanionInventoryRef(span)
            {
                CompanionId = companion.CompanionId,
                Revision = companion.CompanionRevision,
                ItemCount = 0,
            };

            var offset = CompanionInventoryRef.GetRequiredSize(0, 0);
            foreach (var item in items)
            {
                var stored = new StoredItemRef(span[offset..])
                {
                    ItemSlot = item.ItemSlot,
                };
                var serializedSize = serializer.SerializeItem(stored.ItemData, item);
                offset += StoredItemRef.GetRequiredSize(serializedSize);
                packet.ItemCount++;
            }

            span[..offset].SetPacketSize();
            return offset;
        }

        await connection.SendAsync(WriteInventory).ConfigureAwait(false);
    }
}
