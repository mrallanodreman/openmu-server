// <copyright file="MapDragState.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.Shared.Components.MapEditor;

using MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// Stores the state required for dragging and resizing map objects.
/// </summary>
internal struct MapDragState
{
    /// <summary>Gets or sets the starting X coordinate of the mouse drag.</summary>
    public ushort StartX;

    /// <summary>Gets or sets the starting Y coordinate of the mouse drag.</summary>
    public ushort StartY;

    /// <summary>Gets or sets the original X1 corner of the dragged object.</summary>
    public ushort OrigX1;

    /// <summary>Gets or sets the original Y1 corner of the dragged object.</summary>
    public ushort OrigY1;

    /// <summary>Gets or sets the original X2 corner of the dragged object.</summary>
    public ushort OrigX2;

    /// <summary>Gets or sets the original Y2 corner of the dragged object.</summary>
    public ushort OrigY2;

    private const int MapSize = 256;

    /// <summary>Captures the current bounds of the given map area as drag origin.</summary>
    /// <param name="area">The map area to capture bounds from.</param>
    public void Capture(IMapArea area)
    {
        this.OrigX1 = area.X1;
        this.OrigY1 = area.Y1;
        this.OrigX2 = area.X2;
        this.OrigY2 = area.Y2;
    }

    /// <summary>
    /// Computes new dragged bounds and returns whether the position changed.
    /// </summary>
    /// <param name="x">Current mouse X in map coordinates.</param>
    /// <param name="y">Current mouse Y in map coordinates.</param>
    /// <param name="newX1">Computed new X1 value.</param>
    /// <param name="newY1">Computed new Y1 value.</param>
    /// <param name="newX2">Computed new X2 value.</param>
    /// <param name="newY2">Computed new Y2 value.</param>
    /// <returns>True if the computed bounds differ from the originals.</returns>
    public bool ApplyDrag(ushort x, ushort y, out ushort newX1, out ushort newY1, out ushort newX2, out ushort newY2)
    {
        int dx = x - this.StartX;
        int dy = y - this.StartY;
        int width = this.OrigX2 - this.OrigX1;
        int height = this.OrigY2 - this.OrigY1;

        newX1 = (ushort)Math.Clamp(this.OrigX1 + dx, 0, MapSize - 1 - width);
        newY1 = (ushort)Math.Clamp(this.OrigY1 + dy, 0, MapSize - 1 - height);
        newX2 = (ushort)(newX1 + width);
        newY2 = (ushort)(newY1 + height);

        return this.OrigX1 != newX1 || this.OrigY1 != newY1 || this.OrigX2 != newX2 || this.OrigY2 != newY2;
    }
}
