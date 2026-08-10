# C2 45 D0 - AddTransformedCharactersToScopeGlobal (by server)

## Is sent when

The player wears a monster transformation ring in a global coordinate world.

## Causes the following actions on the client side

The character appears as monster, defined by the Skin property, using ushort coordinates.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0x45  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0xD0  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | CharacterCount |
| 6 | CharacterDataGlobal.Length * CharacterCount | Array of CharacterDataGlobal |  | Characters |

### CharacterDataGlobal Structure

Contains the data of an transformed character in a global coordinate world.

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 2 | ShortBigEndian |  | Id |
| 2 | 2 | ShortBigEndian |  | CurrentPositionX |
| 4 | 2 | ShortBigEndian |  | CurrentPositionY |
| 6 | 2 | ShortBigEndian |  | Skin |
| 8 | 10 | String |  | Name |
| 18 | 2 | ShortBigEndian |  | TargetPositionX |
| 20 | 2 | ShortBigEndian |  | TargetPositionY |
| 22 | 4 bit | Byte |  | Rotation |
| 22 << 0 | 4 bit | CharacterHeroState |  | HeroState |
| 23 | 18 | Binary |  | Appearance |
| 41 | 1 | Byte |  | EffectCount; Defines the number of effects which would be sent after this field. |
| 42 | EffectId.Length * EffectCount | Array of EffectId |  | Effects |

### CharacterHeroState Enum

Defines the hero state of a character.

| Value | Name | Description |
|-------|------|-------------|
| 0 | New | The character is new and has the highest state. |
| 1 | Hero | The character is a hero. |
| 2 | LightHero | The character is a hero, but the state is almost gone. |
| 3 | Normal | The character is in a neutral state. |
| 4 | PlayerKillWarning | The character killed another character, and has a kill warning. |
| 5 | PlayerKiller1stStage | The character killed two characters, and has some restrictions. |
| 6 | PlayerKiller2ndStage | The character killed more than two characters, and has hard restrictions. |

### EffectId Structure

Contains the id of a magic effect.

Length: 1 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Id |