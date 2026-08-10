# C3 CC - TeleportTargetGlobal (by client)

## Is sent when

A wizard uses the 'Teleport Ally' skill to teleport a party member of his view range to a nearby coordinate in a global coordinate world.

## Causes the following actions on the server side

If the target player is in the same party and in the range, it will teleported to the specified coordinates.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC3  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   9   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xCC  | Packet header - packet type identifier |
| 3 | 2 | ShortLittleEndian |  | TargetId |
| 5 | 2 | ShortBigEndian |  | TeleportTargetX |
| 7 | 2 | ShortBigEndian |  | TeleportTargetY |