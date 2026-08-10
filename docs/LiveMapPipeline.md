# Live map pipeline: mu-map-editor -> LinuxMu + OpenMU

This pipeline applies an existing 512x512 world without reinitializing OpenMU.

## Export

In the Electron map editor:

1. Load the existing `Data` root and open `WorldN`.
2. Edit the terrain and objects.
3. Set **OpenMU map number** (`World1` normally maps to OpenMU map `0`).
4. Click **Export LinuxMu + OpenMU 512 Bundle...**.

The exported layout is:

```text
bundle/
├── Data/WorldN/
│   ├── EncTerrainN.att
│   ├── EncTerrainN.map
│   ├── EncTerrainN.obj
│   ├── EncTerrainN_SERVER.att
│   ├── TerrainHeight.OZB
│   └── TerrainLight.OZJ
├── OpenMU/WorldN/TerrainData.att
└── map-bundle.json
```

The bundle contains the generated core files. Existing `Tile*` textures and
`ObjectN` models remain dependencies of the loaded client Data root.

## Apply to OpenMU

```bash
/run/media/pctorre/HddCompiler/MU/projects/OpenMU/tools/apply-map-bundle.sh /absolute/path/to/bundle
```

The command validates the profile and ATT dimensions, checks that gates and
persisted characters fit, creates a full PostgreSQL backup, updates exactly one
`GameMapDefinition.TerrainData`, verifies its MD5, and restarts
`openmu-startup` so `GameMapTerrain` is reconstructed.

Overlay `bundle/Data/WorldN` into the LinuxMu `Data` root, then test login,
collision, safe zone, movement through 255->256, teleport, and respawn.

## Deliberate exclusions

- OpenMU does not read client `.map`, `.obj`, `.ozb`, textures, or models.
- Terrain cannot currently be hot-reloaded inside an instantiated game map.
- Spawn/gate import is separate. The editor's legacy spawn JSON does not match
  OpenMU `MapSpawnExport` and could delete valid spawns.
- Do not run `-reinit` or recreate the Docker container for a terrain-only update.
