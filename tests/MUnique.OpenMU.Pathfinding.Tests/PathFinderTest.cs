// <copyright file="PathFinderTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Pathfinding.Tests;

using MUnique.OpenMU.Pathfinding;

/// <summary>
/// Tests for the pathfinder.
/// </summary>
[TestFixture]
public class PathFinderTest
{
    private IPathFinder _pathFinder = null!;
    private byte[,] _grid = null!;

    /// <summary>
    /// Sets up the path finder with a basic, unrestricted grid.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this._grid = new byte[0x100, 0x100];
        for (int x = 100; x < 200; x++)
        {
            for (int y = 100; y < 200; y++)
            {
                this._grid[x, y] = 10;
            }
        }

        // Safezone:
        for (int x = 50; x < 100; x++)
        {
            for (int y = 50; y < 100; y++)
            {
                this._grid[x, y] = 0b1000_0001;
            }
        }

        this._pathFinder = new PathFinder(new ScopedGridNetwork());
    }

    /// <summary>
    /// Tests the straight path.
    /// </summary>
    [Test]
    public void TestStraightPath()
    {
        var start = new Point(110, 100);
        var end = new Point(115, 100);
        var result = this._pathFinder.FindPath(start, end, this._grid, false);
        Assert.That(result, Is.Not.Null);
        var lastNode = result!.LastOrDefault();
        Assert.That(lastNode, Is.Not.Null);
        Assert.That(lastNode.X, Is.EqualTo(end.X));
        Assert.That(lastNode.Y, Is.EqualTo(end.Y));
    }

    /// <summary>
    /// Tests the straight path.
    /// </summary>
    [Test]
    public void TestStraightPath_InSafezone()
    {
        var start = new Point(51, 60);
        var end = new Point(60, 60);
        var result = this._pathFinder.FindPath(start, end, this._grid, true);
        Assert.That(result, Is.Not.Null);
        var lastNode = result!.LastOrDefault();
        Assert.That(lastNode, Is.Not.Null);
        Assert.That(lastNode.X, Is.EqualTo(end.X));
        Assert.That(lastNode.Y, Is.EqualTo(end.Y));
    }

    /// <summary>
    /// Tests the straight path.
    /// </summary>
    [Test]
    public void TestStraightPath_InSafezone_ButNotIncluded()
    {
        var start = new Point(51, 60);
        var end = new Point(60, 60);
        var result = this._pathFinder.FindPath(start, end, this._grid, false);
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests the diagonal path.
    /// </summary>
    [Test]
    public void TestDiagonalPath()
    {
        var start = new Point(100, 100);
        var end = new Point(110, 110);
        var result = this._pathFinder.FindPath(start, end, this._grid, false);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Count, Is.EqualTo(10));
        for (int i = 1; i <= 10; i++)
        {
            Assert.That(result[i - 1].X, Is.EqualTo(start.X + i));
            Assert.That(result[i - 1].Y, Is.EqualTo(start.Y + i));
        }

        Assert.That(result.Last().X, Is.EqualTo(end.X));
        Assert.That(result.Last().Y, Is.EqualTo(end.Y));
    }

    /// <summary>
    /// Tests if no path can be found if the end is on an unreachable coordinate.
    /// </summary>
    [Test]
    public void TestNoPathFound()
    {
        var start = new Point(110, 100);
        var end = new Point(115, 99);
        var result = this._pathFinder.FindPath(start, end, this._grid, false);
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that a path crossing the old 255 byte-boundary can be found on a 512x512 grid.
    /// This validates that pathfinding is not limited to coordinates 0..255 anymore.
    /// </summary>
    [Test]
    public void TestStraightPath_Crossing255Boundary()
    {
        var largeGrid = new byte[512, 512];
        for (int x = 0; x < 512; x++)
        {
            for (int y = 0; y < 512; y++)
            {
                largeGrid[x, y] = 10;
            }
        }

        var start = new Point(250, 100);
        var end = new Point(260, 100);
        var result = this._pathFinder.FindPath(start, end, largeGrid, false);
        Assert.That(result, Is.Not.Null);
        var lastNode = result!.LastOrDefault();
        Assert.That(lastNode, Is.Not.Null);
        Assert.That(lastNode.X, Is.EqualTo(end.X));
        Assert.That(lastNode.Y, Is.EqualTo(end.Y));
        Assert.That(result!.Any(n => n.X > 255), Is.True);
    }

    /// <summary>
    /// Tests that a path can be found at the far edge of a 512x512 grid (near coordinate 511).
    /// </summary>
    [Test]
    public void TestStraightPath_FarEdge()
    {
        var largeGrid = new byte[512, 512];
        for (int x = 0; x < 512; x++)
        {
            for (int y = 0; y < 512; y++)
            {
                largeGrid[x, y] = 10;
            }
        }

        var start = new Point(500, 500);
        var end = new Point(510, 500);
        var result = this._pathFinder.FindPath(start, end, largeGrid, false);
        Assert.That(result, Is.Not.Null);
        var lastNode = result!.LastOrDefault();
        Assert.That(lastNode, Is.Not.Null);
        Assert.That(lastNode.X, Is.EqualTo(end.X));
        Assert.That(lastNode.Y, Is.EqualTo(end.Y));
    }
}