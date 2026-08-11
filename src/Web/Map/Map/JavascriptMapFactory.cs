// <copyright file="JavascriptMapFactory.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.Map.Map;

using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

/// <summary>
/// Class which manages the creation of the map which is implemented in javascript.
/// </summary>
public sealed class JavascriptMapFactory : IMapFactory
{
    /// <summary>
    /// The side length assumed when a map has no usable terrain data. It's the size every
    /// map had before per-map sizes existed, so it keeps such maps rendering as before.
    /// </summary>
    private const int DefaultTerrainSize = 256;

    /// <summary>
    /// Number of leading bytes of <see cref="IGameMapInfo.TerrainData"/> which are not tiles,
    /// matching what <see cref="GameMapTerrain"/> skips when it reads the same payload.
    /// </summary>
    private const int TerrainDataHeaderLength = 3;

    private readonly IJSRuntime _jsRuntime;
    private readonly ILoggerFactory _loggerFactory;

    private int _mapCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="JavascriptMapFactory"/> class.
    /// </summary>
    /// <param name="jsRuntime">The js runtime.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public JavascriptMapFactory(IJSRuntime jsRuntime, ILoggerFactory loggerFactory)
    {
        this._jsRuntime = jsRuntime;
        this._loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public async ValueTask<IMapController> CreateMapAsync(IObservableGameServer gameServer, Guid mapId)
    {
        MapController? mapController = null;
        try
        {
            var appId = this.GenerateMapAppIdentifier(gameServer.Id, mapId);
            var mapSize = GetTerrainSize(gameServer, mapId);
            await this._jsRuntime.InvokeVoidAsync("CreateMap", gameServer.Id, mapId, this.GetMapContainerIdentifier(gameServer.Id, mapId), appId, mapSize).ConfigureAwait(false);
            mapController = new MapController(this._jsRuntime, this._loggerFactory, appId, gameServer, mapId);
            await gameServer.RegisterMapObserverAsync(mapId, mapController).ConfigureAwait(false);
        }
        catch
        {
            if (mapController != null)
            {
                await mapController.DisposeAsync().ConfigureAwait(false);
            }

            throw;
        }

        return mapController;
    }

    /// <inheritdoc />
    public string GetMapContainerIdentifier(int serverId, Guid mapId) => $"map_{serverId}_{mapId:N}";

    /// <summary>
    /// Determines the side length of a map, so that the viewer can size itself to the actual
    /// map instead of assuming the historical 256. The terrain payload is square after its
    /// header, which is where the size comes from — the same way <see cref="GameMapTerrain"/>
    /// derives it.
    /// </summary>
    private static int GetTerrainSize(IObservableGameServer gameServer, Guid mapId)
    {
        if (gameServer.Maps.FirstOrDefault(m => m.Id == mapId)?.TerrainData is not { } terrainData
            || terrainData.Length <= TerrainDataHeaderLength)
        {
            return DefaultTerrainSize;
        }

        var size = (int)Math.Sqrt(terrainData.Length - TerrainDataHeaderLength);
        return size * size == terrainData.Length - TerrainDataHeaderLength ? size : DefaultTerrainSize;
    }

    private string GenerateMapAppIdentifier(int serverId, Guid mapId) => $"map_{serverId}_{mapId:N}_app{this._mapCount++}";
}