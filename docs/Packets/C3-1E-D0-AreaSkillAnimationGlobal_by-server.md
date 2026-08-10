# C3 1E D0 - AreaSkillAnimationGlobal (by server)

## Is sent when

An object performs a skill which has effect on an area in a global coordinate world.

## Causes the following actions on the client side

The animation is shown on the user interface using ushort coordinates.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC3  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   14   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0x1E  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xD0  | Packet header - sub packet type identifier |
| 4 | 2 | ShortBigEndian |  | SkillId |
| 6 | 2 | ShortBigEndian |  | PlayerId |
| 8 | 2 | ShortBigEndian |  | PointX |
| 10 | 2 | ShortBigEndian |  | PointY |
| 12 | 1 | Byte |  | Rotation |