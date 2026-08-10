# C4 F3 60 - CompanionState (by server)

## Is sent when

While an owned temporary companion is active in the party and after a state mutation.

## Causes the following actions on the client side

The client replaces the selected companion snapshot.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC4  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |   386   | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xF3  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x60  | Packet header - sub packet type identifier |
| 5 | 4 | IntegerLittleEndian |  | CompanionId |
| 9 | 4 | IntegerLittleEndian |  | Revision |
| 13 | 10 | String |  | Name |
| 23 | 1 | Byte |  | Class |
| 24 | 2 | ShortLittleEndian |  | Level |
| 26 | 2 | ShortLittleEndian |  | MasterLevel |
| 28 | 4 | IntegerLittleEndian |  | AvailableStatPoints |
| 32 | 4 | IntegerLittleEndian |  | Strength |
| 36 | 4 | IntegerLittleEndian |  | Agility |
| 40 | 4 | IntegerLittleEndian |  | Vitality |
| 44 | 4 | IntegerLittleEndian |  | Energy |
| 48 | 1 | Byte |  | HelperState |
| 49 | 4 | IntegerLittleEndian |  | CurrentHealth |
| 53 | 4 | IntegerLittleEndian |  | MaximumHealth |
| 57 | 4 | IntegerLittleEndian |  | CurrentMana |
| 61 | 4 | IntegerLittleEndian |  | MaximumMana |
| 65 | 4 | IntegerLittleEndian |  | CurrentShield |
| 69 | 4 | IntegerLittleEndian |  | MaximumShield |
| 73 | 2 | ShortLittleEndian |  | HelperConfigurationLength |
| 75 | 257 | Binary |  | HelperConfiguration |
| 332 | 27 | Binary |  | Equipment |
| 359 | 27 | Binary |  | Inventory |