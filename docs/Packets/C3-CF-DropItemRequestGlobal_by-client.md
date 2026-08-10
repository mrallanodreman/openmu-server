# C3 CF - DropItemRequestGlobal (by client)

## Is sent when

A player requests to drop an item of his inventory on the ground in a global coordinate world.

## Causes the following actions on the server side

When the specified coordinates are valid, and the item is allowed to be dropped, it will be dropped on the ground and the surrounding players are notified using ushort coordinates.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC3  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   8   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xCF  | Packet header - packet type identifier |
| 3 | 2 | ShortBigEndian |  | TargetX |
| 5 | 2 | ShortBigEndian |  | TargetY |
| 7 | 1 | Byte |  | ItemSlot |