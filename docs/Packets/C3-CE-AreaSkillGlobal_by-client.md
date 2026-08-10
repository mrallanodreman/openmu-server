# C3 CE - AreaSkillGlobal (by client)

## Is sent when

A player is performing a skill which affects an area of the map in a global coordinate world.

## Causes the following actions on the server side

It's forwarded to all surrounding players, so that the animation is visible. In the original server implementation, no damage is done yet for attack skills - there are separate hit packets.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC3  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   15   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xCE  | Packet header - packet type identifier |
| 3 | 2 | ShortBigEndian |  | SkillId |
| 5 | 2 | ShortBigEndian |  | TargetX |
| 7 | 2 | ShortBigEndian |  | TargetY |
| 9 | 1 | Byte |  | Rotation |
| 12 | 2 | ShortBigEndian |  | ExtraTargetId |
| 14 | 1 | Byte |  | AnimationCounter; Animation counter which acts as a reference to the previously sent Area Skill Animation packet. |