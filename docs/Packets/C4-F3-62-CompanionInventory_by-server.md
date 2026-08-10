# C4 F3 62 - CompanionInventory (by server)

## Is sent when

After CompanionState or after a companion inventory mutation.

## Causes the following actions on the client side

The client replaces the companion inventory snapshot.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC4  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xF3  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x62  | Packet header - sub packet type identifier |
| 5 | 4 | IntegerLittleEndian |  | CompanionId |
| 9 | 4 | IntegerLittleEndian |  | Revision |
| 13 | 1 | Byte |  | ItemCount |
| 14 | StoredItem.Length * ItemCount | Array of StoredItem |  | Items |

### StoredItem Structure

The structure for a stored item, e.g. in the inventory or vault.

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | ItemSlot |
| 1 |  | Binary |  | ItemData |