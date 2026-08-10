# C2 1F D0 - AddSummonedMonstersToScopeGlobal (by server)

## Is sent when

One or more summoned monsters got into the observed scope of the player in a global coordinate world.

## Causes the following actions on the client side

The client adds the monsters to the shown map using ushort coordinates.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0x1F  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0xD0  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | MonsterCount |
| 6 | SummonedMonsterDataGlobal.Length * MonsterCount | Array of SummonedMonsterDataGlobal |  | SummonedMonsters |

### SummonedMonsterDataGlobal Structure

Contains the data of a summoned monster in a global coordinate world.

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 2 | ShortBigEndian |  | Id |
| 2 | 2 | ShortBigEndian |  | TypeNumber |
| 4 | 2 | ShortBigEndian |  | CurrentPositionX |
| 6 | 2 | ShortBigEndian |  | CurrentPositionY |
| 8 | 2 | ShortBigEndian |  | TargetPositionX |
| 10 | 2 | ShortBigEndian |  | TargetPositionY |
| 12 | 4 bit | Byte |  | Rotation |
| 13 | 10 | String |  | OwnerCharacterName |
| 23 | 1 | Byte |  | EffectCount; Defines the number of effects which would be sent after this field. This is currently not supported. |
| 24 | EffectId.Length * EffectCount | Array of EffectId |  | Effects |

### EffectId Structure

Contains the id of a magic effect.

Length: 1 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Id |