# C1 2F D0 - MoneyDroppedExtendedGlobal (by server)

## Is sent when

Money dropped on the ground in a global coordinate world.

## Causes the following actions on the client side

The client adds the money to the ground using ushort coordinates.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   14   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0x2F  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xD0  | Packet header - sub packet type identifier |
| 3 | 1 | Boolean |  | IsFreshDrop; If this flag is set, the money is added to the map with an animation and sound. Otherwise, it's just added like it was already on the ground before. |
| 4 | 2 | ShortLittleEndian |  | Id |
| 6 | 2 | ShortBigEndian |  | PositionX |
| 8 | 2 | ShortBigEndian |  | PositionY |
| 10 | 4 | IntegerLittleEndian |  | Amount |