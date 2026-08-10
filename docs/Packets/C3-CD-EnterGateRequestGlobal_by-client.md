# C3 CD - EnterGateRequestGlobal (by client)

## Is sent when

In a global-world: when the player enters an area on the game map which is configured as gate at the client data files, or, in the special case of wizards, for the teleport skill (GateNumber is 0 and the target coordinates are specified).

## Causes the following actions on the server side

If the player is allowed to enter the "gate", it's moved to the corresponding exit gate area.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC3  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   10   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xCD  | Packet header - packet type identifier |
| 4 | 2 | ShortLittleEndian |  | GateNumber |
| 6 | 2 | ShortBigEndian |  | TeleportTargetX |
| 8 | 2 | ShortBigEndian |  | TeleportTargetY |