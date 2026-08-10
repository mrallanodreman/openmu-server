# C2 20 D0 - ItemsDroppedGlobal (by server)

## Is sent when

The items dropped on the ground in a global coordinate world.

## Causes the following actions on the client side

The client adds the items to the ground using ushort coordinates.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0x20  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0xD0  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | ItemCount |
| 6 | DroppedItemGlobal.Length * ItemCount | Array of DroppedItemGlobal |  | Items |

### DroppedItemGlobal Structure

Contains the data about a dropped item in a global coordinate world.

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 2 | ShortBigEndian |  | Id |
| 0 << 7 | 1 bit | Boolean |  | IsFreshDrop; If this flag is set, the item is added to the map with an animation and sound. Otherwise it's just added like it was already on the ground before. |
| 2 | 2 | ShortBigEndian |  | PositionX |
| 4 | 2 | ShortBigEndian |  | PositionY |
| 6 |  | Binary |  | ItemData |