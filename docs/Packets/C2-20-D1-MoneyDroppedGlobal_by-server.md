# C2 20 D1 - MoneyDroppedGlobal (by server)

## Is sent when

Money dropped on the ground in a global coordinate world.

## Causes the following actions on the client side

The client adds the money to the ground using ushort coordinates.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |   25   | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0x20  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0xD1  | Packet header - sub packet type identifier |
| 5 | 1 | Byte | 1 | ItemCount |
| 6 | 2 | ShortBigEndian |  | Id |
| 6 << 7 | 1 bit | Boolean |  | IsFreshDrop; If this flag is set, the money is added to the map with an animation and sound. Otherwise it's just added like it was already on the ground before. |
| 8 | 2 | ShortBigEndian |  | PositionX |
| 10 | 2 | ShortBigEndian |  | PositionY |
| 12 | 1 | Byte | 15 | MoneyNumber |
| 13 | 4 | IntegerLittleEndian |  | Amount |
| 17 | 1 | Byte | 14 | MoneyGroup |